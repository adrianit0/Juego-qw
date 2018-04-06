using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour, IUpdatable {

    public Sprite[] sprites;
    public float velocidad = 1;
    float oldVelocidad = 0;
    float tiempo = 0, actualTime= 0;
    int pos;

    public bool isUpdatable = true;

    SpriteRenderer render;

    void Awake() {
        render = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if(isUpdatable)
            return;

        OnUpdate(Time.deltaTime);
    }

    public IUpdatable getUpdatable() {
        return this;
    }
    
    public void OnUpdate(float delta) {
        if(velocidad != oldVelocidad) {
            oldVelocidad = velocidad;

            tiempo = velocidad <= 0 ? int.MaxValue : 1 / velocidad;
        }

        actualTime += delta;
        if(actualTime > tiempo) {
            actualTime = 0;

            pos++;
            if(pos >= sprites.Length)
                pos = 0;

            render.sprite = sprites[pos];
        }
    }
    public void OnFixedUpdate(float delta) { }
    public void OnVelocityChange(float nueva) { }
}
