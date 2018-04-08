using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour {

    /// <summary>
    /// 0 - STOP x0
    /// 1 - PLAY x1
    /// 2 - PLAY x2
    /// 3 - PLAY x3
    /// </summary>
    public Button[] botones = new Button[4];

    // Texto de dia y hora
    public Text textoDia;
    public Text textoHora;

    //Info del día
    [Range(0, 1)]
    public float initialDay = 0.5f;
    private float duracionDia = 900;
    private float value;

    //Otra info
    private int dia;
    private float hora;
    private float minuto;

    private List<IUpdatable> updateBuild;
    private float vel = 1;

    void Awake() {
        updateBuild = new List<IUpdatable>();
    }

    void Start() {
        for(int i = 0; i < botones.Length; i++) {
            float x = i;
            botones[i].onClick.AddListener(() => CambiarVelocidad(x));
        }

        value = initialDay;
        SetDay(1);

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

        //MODIFICACION DEL TIEMPO ACTUAL 
        value += delta / duracionDia;

        if(value > 1) {
            value--;
            NextDay();
        }

        UpdateTime(value);
    }

    void FixedUpdate() {
        float fixedDelta = Time.fixedDeltaTime;

        for(int i = 0; i < updateBuild.Count; i++) {
            updateBuild[i].OnFixedUpdate(fixedDelta);
        }
    }

    public void SetDay(int day) {
        dia = day;
        textoDia.text = dia.ToString();
    }

    public void NextDay() {
        dia++;
        textoDia.text = dia.ToString();
    }

    public void UpdateTime(float tiempo) {
        hora = tiempo * 24;
        minuto = Mathf.Floor((hora % 1) * 60);
        hora = Mathf.Floor(hora);
        
        string _min = (minuto.ToString().Length == 1) ? "0" + minuto : minuto.ToString();

        textoHora.text = hora + ":" + _min;
    }

    public float GetDayValue() {
        return value;
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
