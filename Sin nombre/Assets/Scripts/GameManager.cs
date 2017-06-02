using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public Vector2 tamañoTotal = new Vector2(20, 20);

    public GameObject nodoPrefab;
    public GameObject objetivoPrefab;

    GameObject objPadre;
    Nodo[,] mapa;

    Vector3[] posiciones;

    public Personaje personaje;
    public GameObject target;

    Vector2 posTarget;

    public Sprite[] spriteTierra = new Sprite[16];
    public Sprite spriteAgua;
    
    bool encontrado = false;

    Vector2[] direcciones = new Vector2[8] {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left,
        new Vector2 (1, 1), new Vector2 (1, -1), new Vector2 (-1, -1), new Vector2(-1, 1)
    };

    List<NodoPath> nodos = new List<NodoPath>();

	void Start () {
        CrearMapa();
	}
	
    void CrearMapa () {
        mapa = new Nodo[(int) tamañoTotal.x, (int) tamañoTotal.y];
        objPadre = new GameObject();
        objPadre.transform.position = Vector3.zero;
        objPadre.name = "Nodos";

        for (int x = 0; x < tamañoTotal.x; x++) {
            for (int y = 0; y < tamañoTotal.y; y++) {
                GameObject _obj = Instantiate (nodoPrefab);
                _obj.transform.position = new Vector3(x, y, 0);
                _obj.transform.parent = objPadre.transform;
                Nodo _nodo = mapa[x, y] = _obj.GetComponent<Nodo>();
                
                _nodo.render.sprite = spriteTierra[4];
            }
        }
    }

	void Update () {
		if (Input.GetMouseButtonUp (0)) {
            int _x = Mathf.RoundToInt (Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt (Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            Vector2 _pos = new Vector2(_x, _y);
            
            if(_x < 0 || _y < 0 || _x >= tamañoTotal.x || _y >= tamañoTotal.y)
                return;

            mapa[_x, _y].bloqueado = !mapa[_x, _y].bloqueado;
            mapa[_x, _y].coll.isTrigger = !mapa[_x, _y].bloqueado;
            //mapa[_x, _y].render.sprite = (!mapa[_x, _y].bloqueado) ? spriteTierra : spriteAgua;

            ActualizarMapa();
        }

        if (Input.GetMouseButtonUp(1)) {
            int _x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            Vector2 _pos = new Vector2(_x, _y);

            if(_x < 0 || _y < 0 || _x >= tamañoTotal.x || _y >= tamañoTotal.y)
                return;

            GameObject _obj = Instantiate(objetivoPrefab);
            _obj.transform.position = _pos;

            Destroy(target.gameObject);
            target = _obj;

            personaje.SetPositions (PathFinding());
        }
	}
    
    public void Reiniciar () {
        nodos = new List<NodoPath>();
        
        posTarget = new Vector3(Mathf.Round(target.transform.position.x), Mathf.Round(target.transform.position.y));
        encontrado = false;
        posiciones = new Vector3[0];
    }

    public Vector3[] PathFinding() {
        Reiniciar();

        BuscarNodos(new NodoPath(personaje.transform.position, personaje.maxPasos), true);
        while (!encontrado) {
            List<NodoPath> _nodos = new List<NodoPath>(nodos);
            int pathDesactivados = 0;
            for (int i = 0; i < _nodos.Count; i++) {
                if (!_nodos[i].activado) {
                    pathDesactivados++;
                    continue;
                }
                BuscarNodos(_nodos[i]);

                if(encontrado)
                    break;
            }

            if (nodos.Count==0 || _nodos.Count == pathDesactivados) {
                Debug.Log("No se ha encontrado caminos");
                break;
            }
        }

        return posiciones;
    }

    void BuscarNodos (NodoPath path, bool primero = false) {
        int _x = Mathf.RoundToInt(path.pos.x);
        int _y = Mathf.RoundToInt(path.pos.y);

        if(_x < 0 || _y < 0 || _x >= tamañoTotal.x || _y >= tamañoTotal.y)
            return;
        
        if (primero) {
            nodos.Add(path);
        }

        List<Nodo> nodosEncontrados = new List<Nodo>();

        //Busca todos los nodos disponibles
        for (int i = 0; i < direcciones.Length; i++) {
            Nodo nodo = BuscarCamino(path.pos, direcciones[i]);

            if(nodo == null)
                continue;

            nodosEncontrados.Add(nodo);
        }

        //Busca si el nodo pertenece a otro camino, y busca si ya no ha sido trazado por otro camino (De ser así pasa al siguiente).
        //Si encuentra más de un camino válido, se clona.
        //Si no encuentra un camino válido, el camino se borra.
        int nodosValidos = 0;
        int nodosNoValidos = 0;
        foreach (Nodo nodo in nodosEncontrados) {

            if (!ContieneNodo(nodo)) {
                if (nodosValidos>0) {
                    path = new NodoPath(path);
                    path.nodos[path.nodos.Count-1] = nodo;
                    nodos.Add(path);
                } else {
                    path.nodos.Add(nodo);
                    path.AñadirPaso();
                }
                
                path.pos = nodo.transform.position;

                //Si ha llegado a su objetivo, entonces traza el camino y da por concluido el PathFinding
                if(new Vector2(nodo.transform.position.x, nodo.transform.position.y) == posTarget) {
                    CaminoEncontrado(path);
                    return;
                }

                nodosValidos++;
            } else {
                nodosNoValidos++;
            }
        }

        //Si no ha encontrado nodo, borra el camino
        if(nodosEncontrados.Count == 0) {
            nodos.Remove(path);
            return;
        } else if (nodosEncontrados.Count == nodosNoValidos) {
            path.activado = false;
        }
    }

    Nodo BuscarCamino (Vector2 actual, Vector2 camino) {
        int _x = Mathf.RoundToInt(actual.x + camino.x);
        int _y = Mathf.RoundToInt(actual.y + camino.y);

        if(_x < 0 || _y < 0 || _x >= tamañoTotal.x || _y >= tamañoTotal.y)
            return null;

        if(!mapa[_x, _y].bloqueado && (!mapa[_x, (int) actual.y].bloqueado||camino.x==0) && (!mapa[(int) actual.x, _y].bloqueado||camino.y==0))
            return mapa[_x, _y];

        return null;
    }

    bool ContieneNodo (Nodo nodo) {
        for (int i = 0; i < nodos.Count; i++) {
            if(nodos[i].nodos.Contains(nodo))
                return true;
        }

        return false;
    }

    void CaminoEncontrado (NodoPath path) {
        encontrado = true;

        posiciones = new Vector3[path.nodos.ToArray().Length];
        for (int i = 0; i < posiciones.Length; i++) {
            posiciones[i] = path.nodos[i].transform.position;
        }
    }

    //Actualiza los sprites de todo el mapa.
    public void ActualizarMapa() {
        for(int x = 0; x < mapa.GetLength(0); x++) {
            for(int y = 0; y < mapa.GetLength(1); y++) {
                mapa[x, y].render.sprite = SeleccionarTileSet(mapa[x, y].bloqueado ? 1 : 0, new Vector2(x, y));
            }
        }
    }

    Sprite SeleccionarTileSet(int valor, Vector2 position) {
        int _direccion = tipoSprite(valor, Mathf.RoundToInt(position.x), Mathf.RoundToInt (position.y));

        return (valor==0) ? spriteTierra[_direccion] : spriteAgua;
    }

    int tipoSprite(int valor, int x, int y) {
        bool[] value = new bool[4];

        if(y<(mapa.GetLength(1)-1) && (mapa[x, y + 1].bloqueado ? 1 : 0) == valor)
            value[0] = true;
        if(x>0 && (mapa[x - 1, y].bloqueado ? 1 : 0) == valor)
            value[1] = true;
        if(x<(mapa.GetLength(0)-1) && (mapa[x + 1, y].bloqueado ? 1 : 0) == valor)
            value[2] = true;
        if(y>0 && (mapa[x, y - 1].bloqueado ? 1 : 0) == valor)
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

