using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nodo : MonoBehaviour {

    public Recurso recusos;
    public SpriteRenderer render;
    public BoxCollider2D coll;

    public Nodo teletransporte;

    public bool bloqueado = false;
    public int valor = 50;
}

public class NodoPath {
    public bool activado = true;
    public int maxPasos = 10;
    public int actualStep = 0;

    public Vector3 pos;
    public List<Nodo> nodos;

    public NodoPath (Vector3 pos, int maxPasos) {
        this.pos = pos;
        activado = true;
        this.maxPasos = maxPasos;
        actualStep = 0;

        nodos = new List<Nodo>();
    }

    public NodoPath (NodoPath oldPath) {
        if (oldPath==null) {
            Debug.LogWarning("path vacio");
            return;
        }

        maxPasos = oldPath.maxPasos;
        actualStep = oldPath.actualStep;
        pos = oldPath.pos;
        activado = oldPath.activado;

        nodos = new List<Nodo>(oldPath.nodos);
    }

    public void AñadirPaso () {
        if(maxPasos == 0)
            return;

        actualStep++;
        if (actualStep>=maxPasos) {
            activado = false;
        }
    }
}