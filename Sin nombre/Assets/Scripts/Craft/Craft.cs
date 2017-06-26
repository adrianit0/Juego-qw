using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CRAFTTYPE { Mesa, Horno, Cocina }

public class Craft {
    public CRAFTTYPE tipoCraft { get; private set; }
    
    public ResourceInfo[] requisitos { get; private set; }

    public ResourceInfo obtencion { get; private set; }

    //El tiempo que tardas en 
    public int tiempo { get; private set; }

    //Es el consumo del objeto. 
    //1 es la velocidad normal.
    //0.5 es que consume la mitad de combustible.
    //0 no consume nada.
    //2 consume el doble para crear esto.
    public float consumo { get; private set; }

    public Craft (CRAFTTYPE type, ResourceInfo[] requisitos, ResourceInfo obtencion, int tiempo, float consumo) {
        tipoCraft = type;
        this.requisitos = requisitos;
        this.obtencion = obtencion;
        this.tiempo = tiempo;
        this.consumo = consumo;
    }
}