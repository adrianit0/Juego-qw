using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESTRUCTURA { Vivienda, Almacen, Recurso, Huerto }


public class Estructura : MonoBehaviour {

    public ESTRUCTURA tipo;
    public GameManager manager;

    public float tiempoTotal = 0.5f;

    bool awaken = false;

    protected SpriteRenderer render;

    protected void Awake() {
        if(awaken)
            return;

        awaken = true;

        render = GetComponent<SpriteRenderer>();
    } 

    void Start() {
        manager.AddBuildInMap(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), this);

        render.sortingOrder = manager.SetSortingLayer(transform.position.y);
    }
}
