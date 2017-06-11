using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESTRUCTURA { Vivienda, Almacen, Recurso, Huerto, Bolsa }


public class Estructura : MonoBehaviour {

    public ESTRUCTURA tipo;

    public string nombre = "";
    public float estado = 1f;

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
    }

    public void MostrarInformacion () {
        if(manager == null)
            return;

        manager.panelInformacion.SetActive(true);

        manager.textoInformacion.text =
            "<b>" + nombre + ".</b>\n\n";
            /*"<b>Estado: </b>" + Mathf.Round(estado*100) + "%\n\n";*/

        if (estructura!=null) {
            manager.textoInformacion.text += estructura.OnText();
        }
    }

    public void AlDestuirse () {
        //Que hacer cuando se destruye 

        if (estructura != null) {
            estructura.OnDestroyBuild();
        }
    }
}