using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Almacen : Estructura, IEquipo {
    
    public int capacityTotal = 100, capacityActual = 0;

    List<ResourceInfo> inventario = new List<ResourceInfo>();

    public float tamLine = 0.750f;
    public float yPos = -0.383f;
    LineRenderer line;

    void Awake() {
        line = GetComponent<LineRenderer>();
    }

    public void PercentAction(float porc) {
        porc = Mathf.Clamp(porc, 0, 1);

        line.SetPosition(1, new Vector3((porc/tamLine) - tamLine/2, yPos, 0));
    }

    public int AddResource(Recurso recurso) {
        recurso.SetUsar(true);
        return AddResource(recurso.tipoRecurso, recurso.cantidad);
    }

    /// <summary>
    /// Añade recursos al almacen, y devuelve la cantidad de sobra (La que no puede almacenar).
    /// </summary>
    public int AddResource(RECURSOS recurso, int cantidad) {
        int sobrante = 0;
        if (cantidad > (capacityTotal-capacityActual)) {
            sobrante = cantidad - (capacityTotal - capacityActual);
            cantidad = capacityTotal - capacityActual;
        }

        capacityActual += cantidad;

        if(cantidad == 0)
            return sobrante;

        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity += cantidad;
                
                return sobrante;
            }
        }
        
        //Si no existe ese recurso en este inventario, lo crea.
        inventario.Add(new ResourceInfo(recurso, cantidad));
        PercentAction(((float) capacityActual / (float) capacityTotal));
        manager.AddResource(recurso, cantidad);

        return sobrante;
    }
}
