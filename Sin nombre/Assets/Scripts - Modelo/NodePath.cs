using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// La clase que contiene los nodos donde toma el recorrido para el PathFinding.
/// </summary>
public class NodePath {
    bool activado = true;

    int maxPasos = 10;
    int actualStep = 0;

    IntVector2 actualPosition;
    List<Node> nodes;

    public NodePath(IntVector2 pos, int maxPasos) {
        this.actualPosition = pos;
        activado = true;
        this.maxPasos = maxPasos;
        actualStep = 0;

        nodes = new List<Node>();
    }

    public NodePath (NodePath oldPath) {
        if(oldPath == null) {
            Debug.LogWarning("Path vacio");
            return;
        }

        maxPasos = oldPath.maxPasos;
        actualStep = oldPath.actualStep;
        actualPosition = oldPath.actualPosition;
        activado = oldPath.activado;

        nodes = new List<Node>(oldPath.nodes);
    }

    /// <summary>
    /// Añade un nodo al camino
    /// </summary>
    public void AddNode (Node node) {
        nodes.Add(node);

        actualPosition = new IntVector2(node.x, node.y);
    }

    public void OverwriteNode (Node node) {
        nodes[nodes.Count - 1] = node;

        actualPosition = new IntVector2(node.x, node.y);
    }

    public bool Contains(Node node) {
        return nodes.Contains(node);
    }


    public IntVector2[] GetPositions () {
        IntVector2[] posiciones = new IntVector2[CountNodes()];
        for(int i = 0; i < posiciones.Length; i++) {
            posiciones[i] = nodes[i].GetPosition();
        }
        return posiciones;

    }

    public int CountNodes () {
        if(nodes == null)
            return 0;

        return nodes.Count;
    }

    public void AddStep() {
        if(maxPasos == 0)
            return;

        actualStep++;
        if(actualStep >= maxPasos) {
            activado = false;
        }
    }

    public IntVector2 GetPosition () {
        return actualPosition;
    }

    public void Desactivate () {
        actualStep = maxPasos;
    }

    public bool IsActive () {
        return actualStep < maxPasos;
    }
}