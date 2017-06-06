using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

    //Toda la información a guardar del sistema de PathFinding.
    List<NodoPath> nodes = new List<NodoPath>();
    Vector3[] positionsPathFinding;
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

    public Vector3[] PathFind(Personaje character, Vector2 position) {
        //Reinicia los nodos del anterior pathfinding.
        nodes = new List<NodoPath>();

        pathFound = false;
        positionsPathFinding = new Vector3[0];

        if(map == null)
            map = manager.map;

        position = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));

        //Ahora busca el pathFinding
        SearchNodes(new NodoPath(character.transform.position, character.maxSteps), position, true);
        while(!pathFound) {
            List<NodoPath> _nodes = new List<NodoPath>(nodes);
            int pathDesactivados = 0;
            for(int i = 0; i < _nodes.Count; i++) {
                if(!_nodes[i].activado) {
                    pathDesactivados++;
                    continue;
                }
                SearchNodes(_nodes[i], position);

                if(pathFound)
                    break;
            }

            if(nodes.Count == 0 || _nodes.Count == pathDesactivados) {
                Debug.Log("No se ha encontrado caminos");
                break;
            }
        }

        return positionsPathFinding;
    }

    void SearchNodes(NodoPath path, Vector2 objetive, bool firstPath = false) {
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

        if(!map[_x, _y].bloqueado && (!map[_x, (int) actual.y].bloqueado || camino.x == 0) && (!map[(int) actual.x, _y].bloqueado || camino.y == 0))
            return map[_x, _y];

        return null;
    }

    bool ContainNode(Nodo nodo) {
        for(int i = 0; i < nodes.Count; i++) {
            if(nodes[i].nodos.Contains(nodo))
                return true;
        }

        return false;
    }

    void PathFound(NodoPath path) {
        pathFound = true;

        positionsPathFinding = new Vector3[path.nodos.ToArray().Length];
        for(int i = 0; i < positionsPathFinding.Length; i++) {
            positionsPathFinding[i] = path.nodos[i].transform.position;
        }
    }
}
