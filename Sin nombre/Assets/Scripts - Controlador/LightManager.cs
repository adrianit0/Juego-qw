using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour, IUpdatable {
    
    public Light directionalLight;
    
    public Gradient colores;
    public AnimationCurve curva;
    
    private TimeManager time;

    private void Awake() {
        time = GetComponent<TimeManager>();
    }

    void Start() {
        
    }

    public void OnUpdate(float delta) {
        float value = time.GetDayValue();

        directionalLight.transform.rotation = Quaternion.Euler(curva.Evaluate(value) * 360, -30, 0);
        RenderSettings.ambientLight = colores.Evaluate(value);
    }

    //public void OnUpdate(float delta) { }
    public void OnFixedUpdate(float delta) { }
    public void OnVelocityChange(float nueva) { }
    

}
