using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TIPOPATH { Posicion, AlmacenEspacio, AlmacenObjeto, huecoLibre }

public class PathFinding : MonoBehaviour {

    //Toda la información a guardar del sistema de PathFinding.

    List<NodoPath> nodes = new List<NodoPath>();
    PathSetting settings;
    PathResult result;
    bool pathFound = false;

    Vector2[] directions = new Vector2[8] {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left,
        new Vector2 (1, 1), new Vector2 (1, -1), new Vector2 (-1, -1), new Vector2(-1, 1)
    };

    Nodo[,] map;

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    public PathResult PathFind(Personaje character, PathSetting settings) {
        //Reinicia los nodos del anterior pathfinding.
        nodes = new List<NodoPath>();
        result = new PathResult();
        this.settings = settings;
        pathFound = false;
        
        if(map == null)
            map = manager.map;

        settings.position = new Vector2(Mathf.Round(settings.position.x), Mathf.Round(settings.position.y));

        //Ahora busca el pathFinding
        SearchNodes(new NodoPath(character.transform.position, character.maxSteps), true);
        while(!pathFound) {
            List<NodoPath> _nodes = new List<NodoPath>(nodes);
            int pathDesactivados = 0;
            for(int i = 0; i < _nodes.Count; i++) {
                if(!_nodes[i].activado) {
                    pathDesactivados++;
                    continue;
                }
                SearchNodes(_nodes[i]);

                if(pathFound)
                    break;
            }

            if(nodes.Count == 0 || _nodes.Count == pathDesactivados) {
                Debug.Log("No se ha encontrado caminos");
                break;
            }
        }

        return result;
    }

    void SearchNodes(NodoPath path, bool firstPath = false) {
        int _x = Mathf.RoundToInt(path.pos.x);
        int _y = Mathf.RoundToInt(path.pos.y);

        if(_x < 0 || _y < 0 || _x >= manager.totalSize.x || _y >= manager.totalSize.y)
            return;

        if(firstPath) {
            nodes.Add(path);
        }

        List<Nodo> foundNodes = new List<Nodo>();

        //Busca todos los nodos disponibles
        for(int i = 0; i < directions.Length; i++) {
            Nodo node = SearchPath(path.pos, directions[i]);

            if(pathFound) {
                PathFound(path, node);
                return;
            }

            if(node == null)
                continue;

            foundNodes.Add(node);
        }

        //Busca si el nodo pertenece a otro camino, y busca si ya no ha sido trazado por otro camino (De ser así pasa al siguiente).
        //Si encuentra más de un camino válido, se clona.
        //Si no encuentra un camino válido, el camino se borra.
        int validNodes = 0;
        int invalidNodes = 0;
        foreach(Nodo node in foundNodes) {

            if(!ContainNode(node)) {
                if(validNodes > 0) {
                    path = new NodoPath(path);
                    path.nodos[path.nodos.Count - 1] = node;
                    nodes.Add(path);
                } else {
                    path.nodos.Add(node);
                    path.AddStep();
                }

                path.pos = node.transform.position;
                
                validNodes++;
            } else {
                invalidNodes++;
            }
        }

        //Si no ha encontrado nodo, borra el camino
        if(foundNodes.Count == 0) {
            nodes.Remove(path);
            return;
        } else if(foundNodes.Count == invalidNodes) {
            path.activado = false;
        }
    }

    //Encuentra todos los nodos de un camino
    Nodo SearchPath(Vector2 actual, Vector2 camino) {
        int _x = Mathf.RoundToInt(actual.x + camino.x);
        int _y = Mathf.RoundToInt(actual.y + camino.y);

        if(_x < 0 || _y < 0 || _x >= manager.totalSize.x || _y >= manager.totalSize.y)
            return null;

        //Si ha llegado a su objetivo, entonces traza el camino y da por concluido el PathFinding
        if(EvaluarNodo (map[_x, _y])) {
            pathFound = true;
            
            return map[_x, _y];
        }

        if(!map[_x, _y].bloqueado && (!map[_x, (int) actual.y].bloqueado || camino.x == 0) && (!map[(int) actual.x, _y].bloqueado || camino.y == 0))
            return map[_x, _y];

        return null;
    }

    bool EvaluarNodo (Nodo nodo) {
        //Aquí se evaluará de todas las maneras posibles aptas en el juego los nodos del PathFinding
        switch (settings.type) {
            case TIPOPATH.Posicion:
                return nodo.transform.position == settings.position;

            case TIPOPATH.AlmacenEspacio:
                if(nodo.estructura != null && nodo.estructura.tipo == ESTRUCTURA.Almacen) {
                    Almacen _almacen = nodo.estructura.GetComponent<Almacen>();

                    return (_almacen.capacityActual<_almacen.capacityTotal);
                }
                    

                return false;

            case TIPOPATH.AlmacenObjeto:
                if(nodo.estructura != null && nodo.estructura.tipo == ESTRUCTURA.Almacen && settings.recursos != null) {
                    Almacen _almacen = nodo.estructura.GetComponent<Almacen>();

                    foreach (ResourceInfo recurso in _almacen.inventario) {
                        for (int i = 0; i < settings.recursos.Count; i++) {
                            if (recurso.type == settings.recursos[i].type && recurso.quantity > 0) {
                                return true;
                            }
                        }
                    }
                }

                return false;

            case TIPOPATH.huecoLibre:
                return !nodo.bloqueado&&nodo.estructura==null;
        }

        return false;
    }

    bool ContainNode(Nodo nodo) {
        for(int i = 0; i < nodes.Count; i++) {
            if(nodes[i].nodos.Contains(nodo))
                return true;
        }

        return false;
    }

    void PathFound(NodoPath path, Nodo lastNode) {
        path.nodos.Add(lastNode);
        Vector3[] posiciones = new Vector3[path.nodos.ToArray().Length];
        for(int i = 0; i < posiciones.Length; i++) {
            posiciones[i] = path.nodos[i].transform.position;
        }


        result.path = posiciones;
        result.finalPosition = lastNode.transform.position;
    }
}

/// <summary>
/// Mejorando el sistema del PathFinding.
/// </summary>
public class PathSetting {
    public TIPOPATH type;
    
    public Vector3 position;
    public ESTRUCTURA build;

    public List<ResourceInfo> recursos;

    //Busca el mejor camino a una posición en concreto.
    public PathSetting(Vector3 posicion) {
        type = TIPOPATH.Posicion;

        this.position = new Vector3 (Mathf.Round (posicion.x), Mathf.Round(posicion.y), 0);
    }

    //Busca el mejor camino a un baul que no esté lleno
    public PathSetting (TIPOPATH type) {
        this.type = type;
    }

    //Busca el mejor camino a un baul con el contenido 
    public PathSetting (List<ResourceInfo> recursos) {
        type = TIPOPATH.AlmacenObjeto;

        this.recursos = recursos;
    }
}

public class PathResult {
    public Vector3 finalPosition;

    public Vector3[] path;

    public PathResult (Vector3 finalPosition, Vector3[] path) {
        this.finalPosition = finalPosition;

        this.path = path;
    }

    public PathResult() {
        this.finalPosition = Vector3.zero;

        this.path = null;
    }
}