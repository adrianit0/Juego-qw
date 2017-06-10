using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Almacen : Estructura, IEquipo, IEstructura {

    public int capacityTotal = 100, capacityActual = 0;

    public List<ResourceInfo> inventario = new List<ResourceInfo>();

    public float tamLine = 0.750f;
    public float yPos = -0.383f;
    LineRenderer line;

    new void Awake() {
        base.Awake();

        line = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Añade recursos al almacen, y devuelve la cantidad de sobra (La que no puede almacenar).
    /// </summary>
    public int AddResource(RECURSOS recurso, int cantidad) {
        if(cantidad == 0)
            return 0;

        int sobrante = 0;
        if(cantidad > (capacityTotal - capacityActual)) {
            sobrante = cantidad - (capacityTotal - capacityActual);
            cantidad = capacityTotal - capacityActual;
        }

        capacityActual += cantidad;

        if(cantidad == 0)
            return sobrante;

        bool encontrado = false;
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity += cantidad;
                encontrado = true;
            }
        }

        if(!encontrado) //Si no existe ese recurso en este inventario, lo crea.
            inventario.Add(new ResourceInfo(recurso, cantidad));


        Porcentaje(((float) capacityActual / (float) capacityTotal));
        manager.AddResource(recurso, cantidad);

        return sobrante;
    }

    public void RemoveResource(RECURSOS recurso, int cantidad) {
        if(cantidad == 0)
            return;
        
        bool encontrado = false;
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity -= cantidad;
                encontrado = true;
            }
        }

        if(!encontrado)
            return;
        
        capacityActual -= cantidad;

        Porcentaje(((float) capacityActual / (float) capacityTotal));
        manager.RemoveResource(recurso, cantidad);
    }

    public int GetResource(RECURSOS recurso, int cantidad, Personaje personaje) {
        if(cantidad == 0)
            return 0;

        int faltante = 0;

        int disponible = CountItem(recurso);

        if (cantidad > disponible) {
            faltante = cantidad - disponible;
            cantidad = disponible;
        }

        personaje.AddResource(recurso, cantidad);
        RemoveResource(recurso, cantidad);

        return faltante;
    }

    public int CountItem(RECURSOS recurso) {
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                return inventario[i].quantity;
            }
        }
        return 0;
    }

    public void Porcentaje(float porc) {
        porc = Mathf.Clamp(porc, 0, 1);

        line.SetPosition(1, new Vector3(porc * tamLine - tamLine / 2, yPos, 0));
    }

    public string OnText() {
        string text =  "<b>Contenido:</b> ["+capacityActual+"/"+capacityTotal+"]\n";

        if (capacityActual>0) {
            for(int i = 0; i < inventario.Count; i++) {
                if (inventario[i].quantity>0) {
                    text += "<b>" +inventario[i].type.ToString() + ":</b> " + inventario[i].quantity + "\n";
                }
            }
        } else {
            text += "Está vacío.";
        }

        return text;
    }
}
