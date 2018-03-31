using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class Informacion : MonoBehaviour {

    //PANEL INFORMACION
    public GameObject panelInformacion;
    public Text textoInformacion;

    //BOTONES
    public BotonInfo[] botones;
    private int autoButton;         //Para no tener que saber su posición, se pondrá automaticamente

    //SELECCION
    public GameObject seleccionPrefab;
    private Estructura[] LastSelectionBuild;
    private GameObject[] LastSelection;

    private List<GameObject> poolSeleccionar;

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    void Start () {
        panelInformacion.SetActive(false);
        poolSeleccionar = new List<GameObject>();
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

    public void SeleccionarTerreno(GameObject[] selections) {
        DesactivarBotones();

        textoInformacion.text = "";
        
        //Muestra el texto
        if(manager == null)
            return;

        string texto = "Sin selección.";

        //TODO:
        //Hacer que especifique el tipo de suelo que es
        if(selections != null) {
            if(selections == null || selections.Length <= 1) {
                texto = "<b>Suelo.</b>\n\n";
                texto += "Suelo fertil, es posible arar y cultivar";
            } else {
                texto = "<b>Suelo.</b>\n[" + selections.Length + " seleccionados]\n\n";
                texto += "Suelo fertil, es posible arar y cultivar";
            }
        }

        AddActionButton(manager.GetIconSprite(TIPOACCION.Construir), "Construir", true, () => { manager.interfaz.ActivarDesactivar(manager.build.panelConstruir); });
        AddActionButton(manager.GetIconSprite(TIPOACCION.Arar), "Arar la tierra", true, () => {
            for(int i = 0; i < selections.Length; i++) {
                manager.actions.CreateAction(selections[i].transform.position, HERRAMIENTA.Arar, TIPOACCION.Almacenar);
            }
        });
        AddActionButton(manager.GetIconSprite(TIPOACCION.Construir), "Cavar", false, () => { });

        SetText(texto);

        //CONTINUA POR AQUÍ
        EliminarSeleccion();

        LastSelectionBuild = new Estructura[0];
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

        autoButton = 0;
        manager.farm.panelCultivo.SetActive(false);
        manager.craft.panel.SetActive(false);
        manager.management.panelPrincipal.SetActive(false);
    }

    public void AddActionButton(Sprite icono, string texto, bool activado, UnityAction accion) {
        int boton = autoButton;

        if(boton >= botones.Length) {
            //TODO:
            //CREAR UN NUEVO BOTON
            //DE MOMENTO SIMPLEMENTE IMPIDE QUE SE PUEDA REALIZAR ESTA ACCION
            return;
        }

        botones[boton].boton.gameObject.SetActive(true);
        botones[boton].boton.onClick.RemoveAllListeners();
        botones[boton].boton.onClick.AddListener(accion);
        botones[boton].boton.interactable = activado;

        botones[boton].icono.sprite = icono;
        botones[boton].texto.text = texto;

        autoButton++;
    }

    public Estructura[] GetSelectedBuild() {
        return LastSelectionBuild;
    }

    public GameObject GetSeleccion() {
        if(poolSeleccionar.Count == 0) {
            return Instantiate(seleccionPrefab);
        }

        GameObject sel = poolSeleccionar[0];
        poolSeleccionar.RemoveAt(0);
        sel.SetActive(true);
        return sel;
    }

    public void EliminarSeleccion() {
        if(LastSelection == null)
            return;

        for(int i = 0; i < LastSelection.Length; i++) {
            if(LastSelection[i] != null) {
                LastSelection[i].SetActive(false);
                poolSeleccionar.Add(LastSelection[i]);
            }
        }
    }
}

[System.Serializable]
public class BotonInfo {
    public Button boton;
    public Image icono;
    public Text texto;
}