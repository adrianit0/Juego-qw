using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESTRUCTURA { Vivienda, Almacen, Recurso, Huerto, Bolsa, Agua }


public class Estructura : MonoBehaviour {

    public ESTRUCTURA tipo;

    public string nombre = "";

    public bool bloquear = true;
    public GameManager manager;

    public bool esDestruible = false;
    public float tiempoTotal = 0.5f;
    public float tiempoDestruccion = 1f;

    bool awaken = false;

    IEstructura estructura;
    protected SpriteRenderer render;

    protected void Awake() {
        if(awaken)
            return;

        awaken = true;

        render = GetComponent<SpriteRenderer>();
        estructura = GetComponent<IEstructura>();
    } 

    void Start() {
        manager.AddBuildInMap(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), this);

        render.sortingOrder = manager.SetSortingLayer(transform.position.y);

        if(estructura != null) {
            estructura.OnStart();
        }
    }

    public void MostrarInformacion (params Estructura[] estructuras) {
        if(manager == null)
            return;

        string texto = "Sin selección.";

        if(estructura != null) {
            if(estructuras == null || estructuras.Length <= 1) {
                texto = "<b>" + nombre + ".</b>\n\n";
                texto += estructura.OnText();

            } else {
                texto = "<b>" + tipo + ".</b>\n["+estructuras.Length+" seleccionados]\n\n";
                texto += estructura.OnTextGroup(estructuras);
            }
        }

        manager.info.SetText(texto);
    }

    public void AlDestuirse () {
        //Que hacer cuando se destruye 

        if (estructura != null) {
            estructura.OnDestroyBuild();
        }
    }
}