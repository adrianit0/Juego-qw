using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoguera : MonoBehaviour {

    public Light luz;

    public Light directionalLight;

    public float segundos = 7;

    public Gradient colores;
    public AnimationCurve curva;

    public float minLight = 1.5f, maxLight = 2f;

    float value = 0;

	void Update () {
        if(luz == null)
            return;

        luz.intensity = Mathf.PingPong(Time.time, maxLight - minLight) + minLight;
        /*
        directionalLight.transform.rotation = Quaternion.Euler(curva.Evaluate(value) * 360, -30, 0);
        RenderSettings.ambientLight = colores.Evaluate(value);

        value += Time.deltaTime/ segundos;

        if(value > 1)
            value = 0;*/
    }
}
