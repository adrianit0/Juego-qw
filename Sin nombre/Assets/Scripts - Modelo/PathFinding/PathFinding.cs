using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PATHTYPE { Posicion, AlmacenEspacio, AlmacenObjeto, huecoLibre, Agua }

public class PathFinding  {

    //Toda la información a guardar del sistema de PathFinding.
    List<NodePath> nodes;
    PathSetting settings;
    PathResult result;
    bool pathFound = false;

    Vector2[] directions = new Vector2[8] {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left,
        new Vector2 (1, 1), new Vector2 (1, -1), new Vector2 (-1, -1), new Vector2(-1, 1)
    };

    GameManager manager;

    //Constructor del pathfinding.
    public PathFinding (GameManager manager) {
        this.manager = manager;
    }

    public PathResult PathFind (IntVector2 position, int maxSteps, PathSetting settings) {
        //Reinicia los nodos del anterior pathfinding.
        nodes = new List<NodePath>();
        result = new PathResult();
        this.settings = settings;
        pathFound = false;

        //Ahora busca el pathFinding
        SearchNodes(new NodePath(position, maxSteps), true);
        while(!pathFound) {
            List<NodePath> _nodes = new List<NodePath>(nodes);
            int pathDesactivados = 0;
            for(int i = 0; i < _nodes.Count; i++) {
                if(!_nodes[i].IsActive()) {
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

    public PathResult PathFind(Personaje character, PathSetting settings) {
        return PathFind(character.transform.position, character.maxSteps, settings);
    }

    void SearchNodes(NodePath path, bool firstPath = false) {
        IntVector2 position = path.GetPosition();

        if(position.x < 0 || position.y < 0 || position.x >= manager.totalSize.x || position.y >= manager.totalSize.y)
            return;

        if(firstPath) {
            nodes.Add(path);
        }

        List<Node> foundNodes = new List<Node>();

        //Busca todos los nodos disponibles
        for(int i = 0; i < directions.Length; i++) {
            Node node = SearchPath(path.GetPosition(), directions[i]);

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
        foreach(Node node in foundNodes) {
            if(!ContainNode(node)) {

                if(validNodes == 0) {
                    //Si es primer nodo valido encontrado se 
                    path.AddNode(node);
                    path.AddStep();
                } else {
                    path = new NodePath(path);
                    path.OverwriteNode(node);
                    nodes.Add(path);
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
            path.Desactivate();
        }
    }

    //Encuentra todos los nodos de un camino
    Node SearchPath(IntVector2 actual, IntVector2 camino) {
        int _x = actual.x + camino.x;
        int _y = actual.y + camino.y;

        if(_x < 0 || _y < 0 || _x >= manager.totalSize.x || _y >= manager.totalSize.y)
            return null;

        Node node = manager.GetNode(_x, _y);

        //Si ha llegado a su objetivo, entonces traza el camino y da por concluido el PathFinding
        if(EvaluateNode (node)) {
            pathFound = true;
            
            return node;
        }

        if(!node.IsBlocked() && (!manager.GetNode (_x, actual.y).IsBlocked() || camino.x == 0) && (!manager.GetNode(actual.y, _y).IsBlocked() || camino.y == 0))
            return node;

        return null;
    }

    /// <summary>
    /// Evalua el nodo dependiendo de la configuración.
    /// </summary>
    bool EvaluateNode (Node node) {
        //Aquí se evaluará de todas las maneras posibles aptas en el juego los nodos del PathFinding
        switch (settings.GetPathType ()) {
            case PATHTYPE.Posicion:
                return node.GetPosition() == settings.GetPosition();

            case PATHTYPE.AlmacenEspacio:
                if(node.GetBuildType () == ESTRUCTURA.Almacen) {
                    Almacen _almacen = node.GetBuild().GetComponent<Almacen>();

                    return (_almacen.inventario.Count<_almacen.capacityTotal);
                }
                    

                return false;

            case PATHTYPE.AlmacenObjeto:
                if(node.GetBuildType() == ESTRUCTURA.Almacen && settings.ResourceCount()>0) {
                    Almacen _almacen = node.GetBuild().GetComponent<Almacen>();

                    foreach (ResourceInfo recurso in _almacen.inventario.inventario) {
                        if(settings.Value(recurso.type))
                            return true;
                    }
                }

                return false;

            case PATHTYPE.Agua:
                if (node.GetBuildType() == ESTRUCTURA.Agua) {
                    return node.GetBuild().GetComponent<Agua>().agua.GetWater(settings.GetWaterType()) > settings.GetWaterMin();
                }

                return false;

            case PATHTYPE.huecoLibre:
                return !node.IsBlocked() && node.GetBuildType() != ESTRUCTURA.Ninguno;
        }

        return false;
    }

    bool ContainNode(Node nodo) {
        for(int i = 0; i < nodes.Count; i++) {
            if(nodes[i].Contains(nodo))
                return true;
        }

        return false;
    }

    void PathFound(NodePath path, Node lastNode) {
        path.AddNode (lastNode);
        IntVector2[] posiciones = path.GetPositions();

        result = new PathResult(lastNode.GetPosition(), posiciones);
    }
}