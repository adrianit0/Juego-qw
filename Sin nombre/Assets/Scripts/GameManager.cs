using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RECURSOS { Madera, Piedra }

public class GameManager : MonoBehaviour {

    public Vector2 totalSize = new Vector2(20, 20);

    public ResourceInfo[] resource = new ResourceInfo[2];

    public GameObject nodoPrefab;
    public GameObject objetivoPrefab;

    GameObject objParent;
    Nodo[,] map;

    //Toda la información a guardar del sistema de PathFinding.
    List<NodoPath> nodes = new List<NodoPath>();
    Vector3[] positionsPathFinding;
    bool pathFound = false;
    Vector2[] directions = new Vector2[8] {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left,
        new Vector2 (1, 1), new Vector2 (1, -1), new Vector2 (-1, -1), new Vector2(-1, 1)
    };

    public List<Personaje> characters = new List<Personaje>();
    public List<Action> actions = new List<Action>();
    
    public Sprite[] spriteTierra = new Sprite[16];
    public Sprite spriteAgua;
    
	void Awake () {
        CrearMapa();
	}

    void Start () {
        InvokeRepeating("SearchAction", 0.25f, 0.25f);
    }
    
    //Busca la acción idonea para realizar para cada personaje.
    void SearchAction () {
        if(actions.Count == 0)
            return;

        //Los personajes que no estuvieran realizando ninguna acción.
        List<Personaje> freeWorkers = new List<Personaje>();

        foreach (Personaje worker in characters) {
            if(worker.action == null)
                freeWorkers.Add(worker);
        }

        if(freeWorkers.Count == 0)
            return;
        
        for (int i = 0; i < actions.Count; i++) {
            //Calcularemos que personaje es el mejor para realizar cada acción libre, como actualmente no hay nada de eso programado, el personaje más cercano será quien se ocupe de la acción.
            int nearWorkers = 0;
            Vector3[] nearPositions = null;
            for(int x = 0; x < freeWorkers.Count; x++) {
                Vector3[] ActualPositions = PathFinding(freeWorkers[x], actions[i].position) ;

                if (nearPositions==null || ActualPositions.Length<nearPositions.Length) {
                    nearPositions = ActualPositions;
                    nearWorkers = x;
                }
            }

            freeWorkers[nearWorkers].SetPositions(nearPositions);
            freeWorkers[nearWorkers].action = actions[i];
            actions[i].renderIcon.color = Color.gray;

            actions.RemoveAt(i);
            freeWorkers.RemoveAt(nearWorkers);
            i--;

            if(freeWorkers.Count == 0)
                return;
        }
    }
	
    void CrearMapa () {
        map = new Nodo[(int) totalSize.x, (int) totalSize.y];
        objParent = new GameObject();
        objParent.transform.position = Vector3.zero;
        objParent.name = "Nodes";

        for (int x = 0; x < totalSize.x; x++) {
            for (int y = 0; y < totalSize.y; y++) {
                GameObject _obj = Instantiate (nodoPrefab);
                _obj.transform.position = new Vector3(x, y, 0);
                _obj.transform.parent = objParent.transform;
                Nodo _nodo = map[x, y] = _obj.GetComponent<Nodo>();
                
                _nodo.render.sprite = spriteTierra[4];
            }
        }
    }

	void Update () {
		if (Input.GetMouseButtonUp (0)) {
            int _x = Mathf.RoundToInt (Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt (Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y)
                return;

            map[_x, _y].bloqueado = !map[_x, _y].bloqueado;
            map[_x, _y].coll.isTrigger = !map[_x, _y].bloqueado;
            //mapa[_x, _y].render.sprite = (!mapa[_x, _y].bloqueado) ? spriteTierra : spriteAgua;

            UpdateMap();
        }

        if (Input.GetMouseButtonUp(1)) {
            int _x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            Vector2 _pos = new Vector2(_x, _y);

            if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y || map[_x, _y].recusos == null || map[_x, _y].recusos.usado)
                return;

            

            GameObject _obj = Instantiate(objetivoPrefab);
            _obj.transform.position = _pos;
            
            Action _action = new Action(map[_x, _y].recusos, new Vector3(_x, _y), _obj.GetComponent<SpriteRenderer>());
            
            actions.Add(_action);
        }
	}
    
    public void Restart () {
        nodes = new List<NodoPath>();
        
        pathFound = false;
        positionsPathFinding = new Vector3[0];
    }

    public Vector3[] PathFinding(Personaje character, Vector2 position) {
        Restart();
        position = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));

        SearchNodes(new NodoPath(character.transform.position, character.maxSteps), position, true);
        while (!pathFound) {
            List<NodoPath> _nodes = new List<NodoPath>(nodes);
            int pathDesactivados = 0;
            for (int i = 0; i < _nodes.Count; i++) {
                if (!_nodes[i].activado) {
                    pathDesactivados++;
                    continue;
                }
                SearchNodes(_nodes[i], position);

                if(pathFound)
                    break;
            }

            if (nodes.Count==0 || _nodes.Count == pathDesactivados) {
                Debug.Log("No se ha encontrado caminos");
                break;
            }
        }

        return positionsPathFinding;
    }
    
    void SearchNodes (NodoPath path, Vector2 objetive, bool firstPath = false) {
        int _x = Mathf.RoundToInt(path.pos.x);
        int _y = Mathf.RoundToInt(path.pos.y);

        if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y)
            return;
        
        if (firstPath) {
            nodes.Add(path);
        }

        List<Nodo> foundNodes = new List<Nodo>();

        //Busca todos los nodos disponibles
        for (int i = 0; i < directions.Length; i++) {
            Nodo node = SearchPath(path.pos, directions[i]);

            if(node == null)
                continue;

            foundNodes.Add(node);
        }

        //Busca si el nodo pertenece a otro camino, y busca si ya no ha sido trazado por otro camino (De ser así pasa al siguiente).
        //Si encuentra más de un camino válido, se clona.
        //Si no encuentra un camino válido, el camino se borra.
        int validNodes = 0;
        int invalidNodes = 0;
        foreach (Nodo node in foundNodes) {

            if (!ContainNode(node)) {
                if (validNodes>0) {
                    path = new NodoPath(path);
                    path.nodos[path.nodos.Count-1] = node;
                    nodes.Add(path);
                } else {
                    path.nodos.Add(node);
                    path.AñadirPaso();
                }
                
                path.pos = node.transform.position;

                //Si ha llegado a su objetivo, entonces traza el camino y da por concluido el PathFinding
                if(new Vector2(node.transform.position.x, node.transform.position.y) == objetive) {
                    PathFound(path);
                    return;
                }

                validNodes++;
            } else {
                invalidNodes++;
            }
        }

        //Si no ha encontrado nodo, borra el camino
        if(foundNodes.Count == 0) {
            nodes.Remove(path);
            return;
        } else if (foundNodes.Count == invalidNodes) {
            path.activado = false;
        }
    }

    //Encuentra todos los nodos de un camino
    Nodo SearchPath (Vector2 actual, Vector2 camino) {
        int _x = Mathf.RoundToInt(actual.x + camino.x);
        int _y = Mathf.RoundToInt(actual.y + camino.y);

        if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y)
            return null;

        if(!map[_x, _y].bloqueado && (!map[_x, (int) actual.y].bloqueado||camino.x==0) && (!map[(int) actual.x, _y].bloqueado||camino.y==0))
            return map[_x, _y];

        return null;
    }

    bool ContainNode (Nodo nodo) {
        for (int i = 0; i < nodes.Count; i++) {
            if(nodes[i].nodos.Contains(nodo))
                return true;
        }

        return false;
    }

    void PathFound (NodoPath path) {
        pathFound = true;

        positionsPathFinding = new Vector3[path.nodos.ToArray().Length];
        for (int i = 0; i < positionsPathFinding.Length; i++) {
            positionsPathFinding[i] = path.nodos[i].transform.position;
        }
    }

    /// <summary>
    /// Añade un recurso a tu inventario.
    /// </summary>
    public void AddResource (Recurso recurso) {
        for (int i = 0; i < resource.Length; i++) {
            if (resource[i].type == recurso.tipo) {
                resource[i].quantity += recurso.cantidad;
                resource[i].quantityText.text = resource[i].quantity.ToString();

                recurso.SetUsar(true);
                return;
            }
        }

        Debug.LogWarning("No se ha encontrado ese recurso");
    }

    /// <summary>
    /// Crea un recurso en el mapa.
    /// </summary>
    public void CreateResource (int x, int y, Recurso recurso) {
        if(x < 0 || y < 0 || x >= totalSize.x || y >= totalSize.y)
            return;

        map[x, y].recusos = recurso;
    }

    ///Actualiza los sprites de todo el mapa.
    public void UpdateMap() {
        for(int x = 0; x < map.GetLength(0); x++) {
            for(int y = 0; y < map.GetLength(1); y++) {
                map[x, y].render.sprite = SelectTileset(map[x, y].bloqueado ? 1 : 0, new Vector2(x, y));
            }
        }
    }

    Sprite SelectTileset(int valor, Vector2 position) {
        int _direccion = tipoSprite(valor, Mathf.RoundToInt(position.x), Mathf.RoundToInt (position.y));

        return (valor==0) ? spriteTierra[_direccion] : spriteAgua;
    }

    int tipoSprite(int valor, int x, int y) {
        bool[] value = new bool[4];

        if(y<(map.GetLength(1)-1) && (map[x, y + 1].bloqueado ? 1 : 0) == valor)
            value[0] = true;
        if(x>0 && (map[x - 1, y].bloqueado ? 1 : 0) == valor)
            value[1] = true;
        if(x<(map.GetLength(0)-1) && (map[x + 1, y].bloqueado ? 1 : 0) == valor)
            value[2] = true;
        if(y>0 && (map[x, y - 1].bloqueado ? 1 : 0) == valor)
            value[3] = true;
        
        if(value[0] && value[1] && value[2] && value[3])
            return 4;
        else if(value[0] && value[1] && value[2] && !value[3])
            return 7;
        else if(value[0] && value[1] && !value[2] && value[3])
            return 5;
        else if(value[0] && value[1] && !value[2] && !value[3])
            return 8;
        else if(value[0] && !value[1] && value[2] && value[3])
            return 3;
        else if(value[0] && !value[1] && value[2] && !value[3])
            return 6;
        else if(value[0] && !value[1] && !value[2] && value[3])
            return 10;
        else if(value[0] && !value[1] && !value[2] && !value[3])
            return 11;
        else if(!value[0] && value[1] && value[2] && value[3])
            return 1;
        else if(!value[0] && value[1] && value[2] && !value[3])
            return 13;
        else if(!value[0] && value[1] && !value[2] && value[3])
            return 2;
        else if(!value[0] && value[1] && !value[2] && !value[3])
            return 14;
        else if(!value[0] && !value[1] && value[2] && value[3])
            return 0;
        else if(!value[0] && !value[1] && value[2] && !value[3])
            return 12;
        else if(!value[0] && !value[1] && !value[2] && value[3])
            return 9;
        else if(!value[0] && !value[1] && !value[2] && !value[3])
            return 15;

        return 4;
    }
}

[System.Serializable]
public class ResourceInfo {
    public string name;

    public RECURSOS type;

    public int quantity;
    public Sprite sprite;

    public Text quantityText;
}

/// <summary>
/// Acciones
/// </summary>
public class Action {
    public Recurso resourceAction;
    public SpriteRenderer renderIcon;

    public Vector3 position;

    public Action (Recurso resourceAction, Vector3 position, SpriteRenderer render) {
        this.resourceAction = resourceAction;
        this.position = position;

        renderIcon = render;
    }
}