﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour {
    
    Light directionalLight;

    [Range(0,1)]
    public float initialDay = 0.5f;
    float duracionDia = 3600;

    public Gradient colores;
    public AnimationCurve curva;
    
    float value;

    void Awake() {
        directionalLight = GetComponent<Light>();
    }

    void Start() {
        value = initialDay;
    }

    void Update() {
        directionalLight.transform.rotation = Quaternion.Euler(curva.Evaluate(value) * 360, -30, 0);
        RenderSettings.ambientLight = colores.Evaluate(value);

        value += Time.deltaTime/ duracionDia;

        if(value > 1)
            value--;
    }
}