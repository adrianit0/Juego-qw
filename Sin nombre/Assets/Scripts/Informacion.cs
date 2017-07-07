using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Informacion : MonoBehaviour {

    //PANEL INFORMACION
    public GameObject panelInformacion;
    public Text textoInformacion;

    //BOTONES
    public BotonInfo[] botones = new BotonInfo[2];

    //SELECCION
    public GameObject seleccionPrefab;
    public Estructura[] LastSelectionBuild;
    public GameObject[] LastSelection;

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    void Start () {
        panelInformacion.SetActive(false);
    }

    public void SeleccionarUnidades (Estructura firstSelection, Estructura[] builds, GameObject[] selections) {
        DesactivarBotones();

        textoInformacion.text = "";
        if(firstSelection != null)
            firstSelection.MostrarInformacion(builds);

        EliminarSeleccion();

        LastSelectionBuild = builds;
        LastSelection = selections;
    }

    public void SetText (string texto) {
        panelInformacion.SetActive(true); 
        textoInformacion.text = texto;
    }

    public void DesactivarBotones () {
        for (int i = 0; i < botones.Length; i++) {
            botones[i].boton.gameObject.SetActive(false);
        }

        manager.farm.panelCultivo.SetActive(false);
        manager.craft.panel.SetActive(false);
        manager.management.panelPrincipal.SetActive(false);
    }

    public void ActivarBoton (int boton, Sprite icono, string texto, bool activado, UnityAction accion) {
        if(boton < 0 || boton >= botones.Length)
            return;

        botones[boton].boton.gameObject.SetActive(true);
        botones[boton].boton.onClick.RemoveAllListeners();
        botones[boton].boton.onClick.AddListener(accion);
        botones[boton].boton.interactable = activado;

        botones[boton].icono.sprite = icono;
        botones[boton].texto.text = texto;
    }

    public void EliminarSeleccion() {
        for(int i = 0; i < LastSelection.Length; i++) {
            if (LastSelection[i] != null)
                Destroy(LastSelection[i]);
        }
    }
}

[System.Serializable]
public class BotonInfo {
    public Button boton;
    public Image icono;
    public Text texto;
}