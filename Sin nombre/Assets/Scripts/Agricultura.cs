using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agricultura : MonoBehaviour {

    public Cultivo[] semillas;

    public GameObject huertoPrefab;
    public int tiempoArar = 1;

    //PANEL
    public GameObject panelCultivo;
    public Text textoNecesario;

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    void Start() {
        panelCultivo.SetActive(false);

        for(int i = 0; i < semillas.Length; i++) {
            int x = i;
            semillas[i].boton.boton.onClick.AddListener(() => {
                Cultivar(x);
            });
        }
    }

    public void Cultivar (int id) {
        panelCultivo.SetActive(false);
        Estructura[] objetos = manager.info.GetSelectedBuild();

        for (int i = 0; i < objetos.Length; i++) {
            Huerto _huerto = objetos[i].GetComponent<Huerto>();
            if(_huerto == null)
                continue;

            if (_huerto.cultivo==null) {
                //Planta el cultivo seleccionado
                manager.actions.CreateAction(_huerto.transform.position, HERRAMIENTA.Custom, TIPOACCION.Plantar, null, false, -1, new ResourceInfo[] { new ResourceInfo(semillas[id].semilla, 1) } );
            }
        }
    }

    public void AbrirPanel (int cantidad) {
        Actualizar(cantidad);
        panelCultivo.SetActive(true);
    }

    public void Actualizar (int necesarias) {
        textoNecesario.text = necesarias.ToString();
        for (int i = 0; i < semillas.Length; i++) {
            int actual = manager.inventario.GetResourceCount(semillas[i].semilla);
            semillas[i].boton.cantidad.text = actual.ToString();
            semillas[i].boton.cantidad.color = (actual >= necesarias) ? Color.black : Color.red;
            semillas[i].boton.boton.interactable = actual >= necesarias;
        }
    }
}

[System.Serializable]
public class Cultivo {
    public RECURSOS semilla;
    public int tiempoCrecer = 1;
    public float litrosPorMinuto = 1;   //Consumo de agua, por minuto.
    public Sprite[] sprite;
    public GameObject cultivoPrefab;

    public BotonCultivo boton;

    public Cultivo (Cultivo otroCultivo) {
        semilla = otroCultivo.semilla;
        tiempoCrecer = otroCultivo.tiempoCrecer;
        litrosPorMinuto = otroCultivo.litrosPorMinuto;
        sprite = otroCultivo.sprite;
        cultivoPrefab = otroCultivo.cultivoPrefab;
    }
}