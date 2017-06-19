﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El nodo es donde estará la información de un tile, ya sea la posición, el coste de movimiento o la estructura construida encima.
/// </summary>
public class Node {

    //Posicion del nodo
    int X;
    public int x {
        get {
            return X;
        }
    }

    int Y;
    public int y {
        get {
            return Y;
        }
    }

    //El manager para acceder directamente ahí
    GameManager manager;

    Estructura build;

    //El valor de movimiento para el pathFinding.
    int movementCost = 100;

    public Node (int x, int y, int movementCost, GameManager manager) {
        X = x;
        Y = y;

        this.manager = manager;
        this.movementCost = movementCost;
    }

    public bool CreateBuild (Estructura createdbuild) {
        //Solo permite construir estructuras, si das de entrada una estructura nula, no te dejará, en lugar de eso deberia usar el método "RemoveBuild ()"
        if (createdbuild==null) {
            Debug.LogWarning("Este método es unicamente para construir, para destruir use \"RemoveBuild()\" en lugar de este método.");
            return false;
        }

        //Si ya hubiera una construcción de antes, devuelve false ya que no puede construir encima.
        if (build != null) {
            Debug.LogWarning("Actualmente ya hay una construcción construida.");
            return false;
        }

        build = createdbuild;

        return true;
    }

    public bool RemoveBuild () {
        //Si no hubiera estructura no habría nada que destruir.
        if (build==null) {
            Debug.LogWarning("No hay nada que destruir");
            return false;
        }

        build = null;

        return true;
    }

    public int GetMovementCost () {
        return movementCost * ((build != null && build.bloquear) ? 0 : 1);
    }

    public IntVector2 GetPosition () {
        return new IntVector2(x, y);
    }

    public Estructura GetBuild () {
        return build;
    }

    public ESTRUCTURA GetBuildType() {
        if(build == null)
            return ESTRUCTURA.Ninguno;

        return build.GetBuildType ();
    }

    public bool IsBlocked () {
        return movementCost <= 0;
    }
}