using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recurso : Estructura {

    public bool usado = false;

    public RECURSOS tipoRecurso = RECURSOS.Madera;
    public int cantidad = 25;

    public Sprite spriteNormal;
    public Sprite spriteUsado;
    
    public SpriteRenderer render;

    void Awake() {
        render = GetComponent<SpriteRenderer>();
        render.sprite = usado ? spriteUsado : spriteNormal;
    }

    public void SetUsar (bool usar) {
        usado = usar;

        render.sprite = usado ? spriteUsado : spriteNormal;
    }
}
