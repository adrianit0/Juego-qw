using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESTRUCTURA { Ninguno, Vivienda, Almacen, Recurso, Huerto, Bolsa, Agua, Muro, Suelo }


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
        IntVector2 pos = GetPosition();
        if (manager.GetNode(pos).GetBuild() == null)
            manager.AddBuildInMap(pos.x, pos.y, this);

        render.sortingOrder = manager.SetSortingLayer(transform.position.y);

        if(estructura != null) {
            estructura.OnStart();
        }
    }
    
    //Constructor, actualmente desactivado al ser una clase heredada del MonoBehvaiour
    //En un futuro, para seguir con el patron MVC, no será heredada de esta.
    /*
    public Estructura (GameManager manager) {
        this.manager = manager;
    }*/

    public IntVector2 GetPosition () {
        return new IntVector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    public ESTRUCTURA GetBuildType () {
        return tipo;
    }

    public void ChangeSprite (Sprite sprite) {
        render.sprite = sprite;
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