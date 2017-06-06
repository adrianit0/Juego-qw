using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESTRUCTURA { Vivienda, Almacen, Recurso, Huerto }


public class Estructura : MonoBehaviour {

    public ESTRUCTURA tipo;
    public GameManager manager;

    public float tiempoTotal = 0.5f;

    void Start() {
        manager.CreateBuild(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), this);
    }
}
