using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recurso : MonoBehaviour {

    public bool usado = false;

    public RECURSOS tipo = RECURSOS.Madera;
    public int cantidad = 25;
    public float tiempoTotal = 2;

    public Sprite spriteNormal;
    public Sprite spriteUsado;

    public GameManager manager;
    public SpriteRenderer render;

    void Awake() {
        render = GetComponent<SpriteRenderer>();
        render.sprite = usado ? spriteUsado : spriteNormal;
    }

    void Start() {
        manager.CrearRecurso(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), this);
    } 

    public void SetUsar (bool usar) {
        usado = usar;

        render.sprite = usado ? spriteUsado : spriteNormal;
    }
}
