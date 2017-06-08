using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum RECURSOS {
    //RECURSOS
    Madera, Piedra, Cobre, Plata, Oro,
    //COMIDA
    Manzana, ManzanaDorada
}
public enum TIPOACCION { Talar, Construir, Investigar, Cocinar, Minar, Cosechar, Almacenar, Pescar, Socializar, Arar, SacarAlmacen }
public enum HERRAMIENTA { Seleccionar = 0, Recolectar = 1, Arar = 2, Priorizar = 3, Destruir = 4, Construir = 5, Custom = 6 }

public class GameManager : MonoBehaviour {

    public Vector2 totalSize = new Vector2(20, 20);
    
    //CONTENIDO PARTIDA
    public ResourceInfo[] resource = new ResourceInfo[2];
    public IconInfo[] iconos = new IconInfo[9];

    public List<Estructura> builds = new List<Estructura>();

    //PANEL HERRAMIENTAS
    public HERRAMIENTA herramientaSeleccionada = HERRAMIENTA.Seleccionar;
    public Sprite[] iconosHerramientas = new Sprite[4];
    public Image imagenCentral;
    
    //PANEL RECURSOS
    public GameObject[] panelesRecursos = new GameObject[2];

    public GameObject nodoPrefab;
    public GameObject objetivoPrefab;

    GameObject objParent;
    public Nodo[,] map;
    
    public List<Personaje> characters = new List<Personaje>();
    public List<Action> actions = new List<Action>();
    
    public Sprite[] spriteTierra = new Sprite[16];
    public Sprite spriteAgua;

    public bool desactivarBotonDerecho = false;

    PathFinding path;
    Agricultura farm;
    public Construccion build;  //Necesario en el personaje.
    
	void Awake () {
        path = GetComponent<PathFinding>();
        farm = GetComponent<Agricultura>();
        build = GetComponent<Construccion>();

        CrearMapa();
	}

    void Start () {
        InvokeRepeating("SearchAction", 0.25f, 0.25f);
    }

    /// <summary>
    /// Devuelve el camino más corto desde la posición del personaje hasta una posición
    /// </summary>
    public Vector3[] PathFinding(Personaje character, Vector2 position) {
        return path.PathFind(character, position);
    }

    void Update() {
        if (build.construyendo) {
            //Si va a construir hace uso de su propio Update().
            build.BuildUpdate();
            return;
        }
        if(!desactivarBotonDerecho && Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject()) {
            int _x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y)
                return;

            map[_x, _y].bloqueado = !map[_x, _y].bloqueado;
            map[_x, _y].coll.isTrigger = !map[_x, _y].bloqueado;
            //mapa[_x, _y].render.sprite = (!mapa[_x, _y].bloqueado) ? spriteTierra : spriteAgua;

            UpdateMap();
        }

        if(Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
            int _x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            

            actions.Add(CreateAction (_x, _y, herramientaSeleccionada));
        }
    }

    public Action CreateAction (int _x, int _y, HERRAMIENTA herramienta, CustomAction customAction = null) {
        if(_x < 0 || _y < 0 || _x >= totalSize.x || _y >= totalSize.y)
                return null;

        Vector2 _pos = new Vector2(_x, _y);
        
        //Buscamos que tipo de acción queremos hacer.
        TIPOACCION accion = TIPOACCION.Talar;

        //Valores personalizados, si el valor es diferente se tomará en lugar del valor genérico.
        GameObject customPrefab = null;
        float customTime = -1;
        Sprite customIcon = null;
        List<ResourceInfo> recNecesario = null;

        switch (herramienta) {
            case HERRAMIENTA.Recolectar:
                if(map[_x, _y].estructura == null)
                    return null;

                Recurso _resource = map[_x, _y].estructura.GetComponent<Recurso>();

                if(_resource != null) {
                    if(_resource.actualQuantity == 0)   //Si el recurso está vacio no te permite usarlo.
                        return null;
                    accion = _resource.actionType;
                } else {
                    return null;
                }
                break;

            case HERRAMIENTA.Arar:
                if(map[_x, _y].estructura != null)
                    return null;

                accion = TIPOACCION.Arar;

                customPrefab = farm.huertoPrefab;
                customTime = farm.tiempoArar; 

                break;

            case HERRAMIENTA.Construir:
                if(map[_x, _y].estructura != null)
                    return null;

                accion = TIPOACCION.Construir;

                customPrefab = build.construcciones[build.selectID].prefab;
                customTime = build.construcciones[build.selectID].tiempo;
                customIcon = build.construcciones[build.selectID].spriteModelo;

                recNecesario = new List<ResourceInfo>();
                for (int i = 0; i < build.construcciones[build.selectID].recursosNecesarios.Length; i++) {
                    recNecesario.Add(new ResourceInfo(build.construcciones[build.selectID].recursosNecesarios[i].recurso, build.construcciones[build.selectID].recursosNecesarios[i].cantidadNecesaria));
                }

                break;

            case HERRAMIENTA.Custom:
                //Herramienta especial. Para realizar cosas que con las anteriores no se pueden (Como extraer cosas del almacen).
                if(customAction == null)
                    return null;

                accion = customAction.tipo;

                switch (accion) {
                    case TIPOACCION.Almacenar:
                    case TIPOACCION.SacarAlmacen:
                        recNecesario = new List<ResourceInfo>(customAction.recNecesarios);
                        break;
                }

                break;

            default:
                Debug.LogWarning("Herramienta no programada aún");
                return null;
        }

        GameObject _obj = Instantiate(objetivoPrefab);
        SpriteRenderer actionRender = _obj.GetComponent<SpriteRenderer>();
        _obj.transform.position = _pos;
        
        actionRender.sprite = customIcon != null ? customIcon : SearchIcon(accion);

        if (accion == TIPOACCION.Construir) {
            return new Action(customPrefab, build.selectID, new Vector3(_x, _y), customTime, actionRender, recNecesario);
        } else if (customPrefab == null) {
            return new Action(map[_x, _y].estructura, accion, new Vector3(_x, _y), map[_x, _y].estructura.tiempoTotal, actionRender, recNecesario);
        } else {
            return new Action(customPrefab, accion, new Vector3(_x, _y), customTime, actionRender, recNecesario);
        }

    }

    Sprite SearchIcon (TIPOACCION tipoAccion) {
        for (int i=0; i < iconos.Length; i++) {
            if(iconos[i].type == tipoAccion)
                return iconos[i].sprite;
        }

        return null;
    }

    //Busca la acción idonea para realizar para cada personaje.
    void SearchAction () {
        if(actions.Count == 0)
            return;

        //Los personajes que no estuvieran realizando ninguna acción.
        List<Personaje> freeWorkers = new List<Personaje>();

        foreach (Personaje worker in characters) {
            if(worker.GetActionsCount() == 0)
                freeWorkers.Add(worker);
        }

        if(freeWorkers.Count == 0)
            return;
        
        for (int i = 0; i < actions.Count; i++) {
            //Calcularemos que personaje es el mejor para realizar cada acción libre, como actualmente no hay nada de eso programado, el personaje más cercano será quien se ocupe de la acción.
            if (actions[i]==null) {
                actions.RemoveAt(i);
                i--;
                continue;
            }

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
            freeWorkers[nearWorkers].AddAction (actions[i]);

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

    /// <summary>
    /// Busca una estructura y dice si existe o no.
    /// </summary>
    public bool ExistBuild (ESTRUCTURA buildType) {
        for (int i = 0; i < builds.Count; i++) {
            if(builds[i].tipo == buildType)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Busca la construcción más cercana.
    /// </summary>
    public Vector2 GetNearBuild (Vector2 initialPos, ESTRUCTURA buildType) {
        Vector2 nearest = Vector3.zero;
        int foundCount = 0;
        for(int i = 0; i < builds.Count; i++) {
            if(builds[i].tipo == buildType && (foundCount==0 || Vector2.Distance (builds[i].transform.position, initialPos) < Vector2.Distance (nearest, initialPos))) {
                nearest = builds[i].transform.position;

                foundCount++;
            }
        }

        if(foundCount == 0)
            Debug.LogWarning("No ha encontrado ninguna construcción cercana");

        return nearest;
    }


    public void AddResource(RECURSOS type, int quantity) {
        if(quantity == 0)
            return;

        for(int i = 0; i < resource.Length; i++) {
            if(resource[i].type == type) {
                resource[i].quantity += quantity;
                if (resource[i].quantityText != null)
                resource[i].quantityText.text = resource[i].quantity.ToString();

                build.ShopUpdate();
                return;
            }
        }

        //Se debería poderse añadir automaticamente
        Debug.LogWarning("No se ha encontrado ese recurso");
    }

    public void RemoveResource(RECURSOS type, int quantity) {
        if(quantity == 0)
            return;

        for(int i = 0; i < resource.Length; i++) {
            if(resource[i].type == type) {
                resource[i].quantity -= quantity;
                if(resource[i].quantityText != null)
                    resource[i].quantityText.text = resource[i].quantity.ToString();

                build.ShopUpdate();
                return;
            }
        }

        //Se debería poderse añadir automaticamente
        Debug.LogWarning("No se ha encontrado ese recurso");
    }

    public int GetResource (RECURSOS type) {
        for(int i = 0; i < resource.Length; i++) {
            if(resource[i].type == type) {
                return resource[i].quantity;
            }
        }

        return 0;
    }

    public Estructura CreateBuild (Vector3 position, GameObject prefab) {
        GameObject _build = Instantiate(prefab);
        _build.transform.position = position;

        _build.GetComponent<SpriteRenderer>().sortingOrder = 3; //PONER AQUI EL LAYER PERSONALIZADO
        Estructura _buildScript = _build.GetComponent<Estructura>();
        _buildScript.manager = this;
        
        AddBuildInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), _buildScript);

        return _buildScript;
    }

    /// <summary>
    /// Crea una estructura en el mapa.
    /// </summary>
    public void AddBuildInMap (int x, int y, Estructura estructura) {
        if(x < 0 || y < 0 || x >= totalSize.x || y >= totalSize.y)
            return;

        map[x, y].estructura = estructura;

        if (!builds.Contains (estructura))
            builds.Add(estructura);
    }

    public void AddBuildInMap(Vector3 position, Estructura estructura) {
        AddBuildInMap(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), estructura);
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
    
    public void CambiarPestañaRecursos (int index) {
        for (int i = 0; i < panelesRecursos.Length; i++) {
            panelesRecursos[i].SetActive(index == i);
        }
    }

    public void SeleccionarHerramienta (int herramienta) {
        herramientaSeleccionada = (HERRAMIENTA) herramienta;
        imagenCentral.sprite = iconosHerramientas[herramienta];
    }
}

[System.Serializable]
public class ResourceInfo {
    public string name;

    public RECURSOS type;

    public int quantity;
    public Sprite sprite;

    public Text quantityText;

    public ResourceInfo (RECURSOS type, int initialQuantity) {
        name = type.ToString();
        this.type = type;

        quantity = initialQuantity;
    }
}

[System.Serializable]
public class IconInfo {
    public TIPOACCION type;
    public Sprite sprite;
    
}