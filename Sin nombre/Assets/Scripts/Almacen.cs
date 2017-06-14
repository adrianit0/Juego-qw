using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Almacen : Estructura, IEstructura, IEquipo {

    public int capacityTotal = 100;

    public Inventario inventario;

    public float tamLine = 0.750f;
    public float yPos = -0.383f;
    LineRenderer line;

    public Sprite spriteVaciar;
    public Sprite spriteAdmin;

    new void Awake() {
        base.Awake();

        line = GetComponent<LineRenderer>();
        inventario = new Inventario(capacityTotal);
        inventario.equipo = (IEquipo) this;
    }

    public void OnStart () {

    }

    public void OnCapacityChange (params ResourceInfo[] recursos) {
        manager._inventario.AddResource(recursos);

        Porcentaje(inventario.GetPerc());
        //Mandar la orden al GameManager para que actualice el panel de recursos.
    }

    public void Porcentaje(float porc) {
        porc = Mathf.Clamp(porc, 0, 1);

        line.SetPosition(1, new Vector3(porc * tamLine - tamLine / 2, yPos, 0));
    }

    public string OnText() {

        manager.info.ActivarBoton(0, spriteVaciar, "Vaciar", inventario.Count>0, () => manager.AddAction (transform.position, HERRAMIENTA.Custom, new CustomAction (TIPOACCION.VaciarAlmacen, false, null)));
        manager.info.ActivarBoton(1, spriteAdmin, "Gestionar", false, () => OnDestroyBuild());

        return RecibirTexto(inventario); ;
    }

    public string OnTextGroup(Estructura[] estructuras) {
        int capTotal = 0;
        Almacen[] almacenes = new Almacen[estructuras.Length];
        for (int i = 0; i < estructuras.Length; i++) {
            almacenes[i] = estructuras[i].GetComponent<Almacen>();
            capTotal += almacenes[i].inventario.Count;
        }
        Inventario _inventario = new Inventario(capTotal);

        for (int i = 0; i < almacenes.Length; i++) {
            _inventario.CopyContent(almacenes[i].inventario);
        }

        manager.info.ActivarBoton(0, spriteVaciar, "Vaciar", _inventario.Count > 0, () => {
            for (int i = 0; i < almacenes.Length; i++) {
                manager.AddAction(almacenes[i].transform.position, HERRAMIENTA.Custom, new CustomAction(TIPOACCION.VaciarAlmacen, false, null));
            }
            });

        return RecibirTexto(_inventario); ;
    }

    string RecibirTexto (Inventario _inventario) {
        string text = "<b>Contenido:</b> [" + _inventario.Count + "/" + capacityTotal + "]\n";

        if(_inventario.Lenght > 0) {
            for(int i = 0; i < _inventario.Lenght; i++) {
                if(_inventario[i] > 0) {
                    text += "<b>" + _inventario.inventario[i].type.ToString() + ":</b> " + _inventario[i] + "\n";
                }
            }
        } else {
            text += "Está vacío.";
        }

        return text;
    }

    public void OnDestroyBuild() {
        if (inventario.Count > 0) {
            manager.CrearSaco(transform.position, 10, inventario.ToArray());

            inventario.CleanResource();
        }

        MostrarInformacion();
    }
}
