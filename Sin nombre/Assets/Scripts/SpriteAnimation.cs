using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour {

    public Sprite[] sprites;
    public float velocidad = 1;
    float oldVelocidad = 0;
    float tiempo = 0, actualTime= 0;
    int pos;

    SpriteRenderer render;

    void Awake() {
        render = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (velocidad != oldVelocidad) {
            oldVelocidad = velocidad;

            tiempo = velocidad <= 0 ? int.MaxValue : 1 / velocidad;
        }

        actualTime += Time.deltaTime;
        if (actualTime>tiempo) {
            actualTime = 0;

            pos++;
            if(pos >= sprites.Length)
                pos = 0;

            render.sprite = sprites[pos];
        }
    }
}
