using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager :MonoBehaviour {

    /// <summary>
    /// 0 - STOP x0
    /// 1 - PLAY x1
    /// 2 - PLAY x2
    /// 3 - PLAY x3
    /// </summary>
    public Button[] botones = new Button[4];

    private List<IUpdatable> updateBuild;
    public float vel = 1;

    void Awake() {
        updateBuild = new List<IUpdatable>();
    }

    void Start() {
        for(int i = 0; i < botones.Length; i++) {
            float x = i;
            botones[i].onClick.AddListener(() => CambiarVelocidad(x));
        }

        CambiarVelocidad(1);
    }

    //SUPER IMPORTANTE, SOLO EXISTIRÁ ESTE UPDATE
    void Update() {
        float delta = Time.deltaTime * vel;

        if(delta == 0)
            return;

        for(int i = 0; i < updateBuild.Count; i++) {
            updateBuild[i].OnUpdate(delta);
        }
    }

    void FixedUpdate() {
        float fixedDelta = Time.fixedDeltaTime;

        for(int i = 0; i < updateBuild.Count; i++) {
            updateBuild[i].OnFixedUpdate(fixedDelta);
        }
    }

    public void AddUpdatable(GameObject obj) {
        IUpdatable update = obj.GetComponent<IUpdatable>();
        if(update != null)
            AddUpdatable(update);
    }

    public void AddUpdatable(IUpdatable interfaz) {
        updateBuild.Add(interfaz);
    }

    public void AddUpdatable(IUpdatable[] interfaz) {
        if (interfaz!=null)
            updateBuild.AddRange(interfaz);
    }

    public void RemoveUpdatable(IUpdatable interfaz) {
        if(updateBuild.Contains(interfaz))
            updateBuild.Remove(interfaz);
    }

    public void RemoveUpdatable(IUpdatable[] interfaz) {
        for(int i = 0; i < interfaz.Length; i++) {
            if(interfaz[i] != null)
                RemoveUpdatable(interfaz[i]);
        }
    }

    void CambiarVelocidad(float nueva) {
        for(int i = 0; i < botones.Length; i++) {
            botones[i].image.color = Color.white;
        }

        vel = nueva;

        //Mandamos el evento de que ha sido cambiado la velocidad (Por ejemplo, para cambiar la velocidad de animación de los personajes).
        for(int i = 0; i < updateBuild.Count; i++) {
            updateBuild[i].OnVelocityChange(vel);
        }

        botones[(int)nueva].image.color = Color.green;
    }
}
