using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Almacen : Estructura, IEstructura, IEquipo {

    public int capacityTotal = 100;

    public Inventario _inventario;

    public float tamLine = 0.750f;
    public float yPos = -0.383f;
    LineRenderer line;

    new void Awake() {
        base.Awake();

        line = GetComponent<LineRenderer>();
        _inventario = new Inventario(capacityTotal);
        _inventario.equipo = (IEquipo) this;
    }

    public void OnCapacityChange (params ResourceInfo[] recursos) {
        manager._inventario.AddResource(recursos);

        Porcentaje(_inventario.GetPerc());
        //Mandar la orden al GameManager para que actualice el panel de recursos.
    }

    public void Porcentaje(float porc) {
        porc = Mathf.Clamp(porc, 0, 1);

        line.SetPosition(1, new Vector3(porc * tamLine - tamLine / 2, yPos, 0));
    }

    public string OnText() {
        string text =  "<b>Contenido:</b> ["+_inventario.Count+"/"+capacityTotal+"]\n";

        if (_inventario.Lenght > 0) {
            for(int i = 0; i < _inventario.Lenght; i++) {
                if (_inventario[i]>0) {
                    text += "<b>" +_inventario.inventario[i].type.ToString() + ":</b> " + _inventario[i] + "\n";
                }
            }
        } else {
            text += "Está vacío.";
        }

        return text;
    }

    public void OnDestroyBuild() {
        if (_inventario.Count > 0) {
            manager.CrearSaco(transform.position, 10, _inventario.ToArray());

            for (int i = 0; i < _inventario.Count; i++) {
                //manager.RemoveResource(inventario[i].type, inventario[i].quantity);
            }
        }
    }
}
