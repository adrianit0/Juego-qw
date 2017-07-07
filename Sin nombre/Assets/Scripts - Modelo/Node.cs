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

    GameManager manager;

    public Estructura build { get; private set; }

    public string floorName { get; private set; }

    //El valor de movimiento para el pathFinding.
    public float movementCost {
        get {
            if (build==null) {
                return 1;
            }

            return build.bloquear ? 0 : 1;
        }
    }

    public Node (int x, int y, GameManager manager) {
        X = x;
        Y = y;

        this.manager = manager;
    }

    public bool CanBuild () {
        return build == null;
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

    /// <summary>
    /// Pregunta si puede construir suelo.
    /// No se podrá construir suelo debajo de los huertos, agua ni paredes.
    /// </summary>
    /// <returns></returns>
    public bool CanBuildFloor () {
        if(build == null)
            return true;

        if(build.tipo == ESTRUCTURA.Agua || build.tipo == ESTRUCTURA.Huerto || build.tipo == ESTRUCTURA.Muro)
            return false;

        return true;
    }

    /// <summary>
    /// Cambia el sprite del suelo. Usado para poner suelos personalizados como el de madera.
    /// </summary>
    public void ChangeFloorSprite (Sprite sprite, bool isCarpetFloor) {
        if(sprite == null) {
            Debug.LogWarning("Node::ChangeFloorSprite error 404: Sprite not found.");
            return;
        }
        
        SpriteRenderer render = manager.tiles[this];
        render.sprite = sprite;
        render.sortingLayerName = (isCarpetFloor) ? "Alfombra" : "Suelo";
    }

    public void ChangeFloorName (string name) {
        floorName = name;
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

    public bool IsEmpty () {
        return build == null;
    }
}