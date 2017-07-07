using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Almacen : Estructura, IEstructura, IEquipo {

    public int capacityTotal = 100;

    public Inventario inventario { get; private set; }

    public float tamLine = 0.750f;
    public float yPos = -0.383f;
    LineRenderer line;

    public Sprite spriteVaciar;
    public Sprite spriteAdmin;

    new void Awake() {
        base.Awake();

        line = GetComponent<LineRenderer>();
    }

    public void OnStart () {
        inventario = new Inventario(capacityTotal, manager);
        inventario.SetInterface((IEquipo) this);
    }

    public void OnCapacityChange (params ResourceInfo[] recursos) {
        manager.inventario.AddResource(recursos);

        Porcentaje(inventario.GetPerc());
        //Mandar la orden al GameManager para que actualice el panel de recursos.
    }

    public void Porcentaje(float porc) {
        porc = Mathf.Clamp(porc, 0, 1);

        line.SetPosition(1, new Vector3(porc * tamLine - tamLine / 2, yPos, 0));
    }

    public string OnText() {
        manager.management.AbrirBaul(this);
        manager.info.ActivarBoton(0, spriteAdmin, "Gestionar", true, () => { manager.management.AbrirBaul(this); });
        manager.info.ActivarBoton(1, spriteVaciar, "Vaciar", inventario.Count>0, () => manager.actions.CreateAction (transform.position, HERRAMIENTA.Custom, TIPOACCION.VaciarAlmacen, null, false));
        
        return RecibirTexto(inventario);
    }

    public string OnTextGroup(Estructura[] estructuras) {
        int capTotal = 0;
        Almacen[] almacenes = new Almacen[estructuras.Length];
        for (int i = 0; i < estructuras.Length; i++) {
            almacenes[i] = estructuras[i].GetComponent<Almacen>();
            capTotal += almacenes[i].inventario.MaxCapacity;
        }
        Inventario _inventario = new Inventario(capTotal, manager);

        for (int i = 0; i < almacenes.Length; i++) {
            _inventario.CopyContent(almacenes[i].inventario);
        }
        
        manager.info.ActivarBoton(0, spriteVaciar, "Vaciar", _inventario.Count > 0, () => {
            for (int i = 0; i < almacenes.Length; i++) {
                manager.actions.CreateAction(almacenes[i].transform.position, HERRAMIENTA.Custom, TIPOACCION.VaciarAlmacen, null, false);
            }
        });

        return RecibirTexto(_inventario); ;
    }

    string RecibirTexto (Inventario _inventario) {
        string text = "<b>Contenido:</b> [" + _inventario.Count + "/" + _inventario.MaxCapacity + "]\n";

        if(_inventario.Lenght > 0) {
            for(int i = 0; i < _inventario.Lenght; i++) {
                if(_inventario[i] > 0) {
                    text += "<b>" + _inventario.GetResourceType(i).ToString() + ":</b> " + _inventario[i] + "\n";
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
