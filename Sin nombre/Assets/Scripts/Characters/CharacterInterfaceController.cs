using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ESTADOANIMO { Feliz, Contento, Neutral, Descontento, Enfermo, Enfadado, Desmayado, Muerto }

public class CharacterInterfaceController : MonoBehaviour {

    //Panel personaje
    public GameObject characterInterfacePrefab;
    public GameObject panelCharacterInterface;

    public EstadoAnimo[] estadoAnimos = new EstadoAnimo[8];

    //Sistema de actualización
    public GameObject panelCharacter;
    public BarraAtributo[] barraAtributo = new BarraAtributo[11];
    public InputField textoNombre;
    public Text textoNivel;
    public Image imagenCuerpo;
    public Image imagenCara;
    public Image imagenMascara;
    public Image imagenAnimo;

    Personaje personajeLigado;


    public CharacterUpdate characters { get; private set; }
    GameManager manager;

     void Awake() {
        manager = GetComponent<GameManager>();

        characters = new CharacterUpdate(manager, this);
    }

    void Start() {
        textoNombre.onValueChanged.AddListener((value) => { OnNameChange(value); });

        panelCharacter.SetActive(false);
    }

    public Sprite GetMood (ESTADOANIMO estado) {
        for (int i =0; i < estadoAnimos.Length; i++){
            if (estadoAnimos[i].animo == estado) {
                return estadoAnimos[i].sprite;
            }
        }

        Debug.LogWarning("CharacterInterfaceController::GetMood error: Estado de ánimo no reconocido");
        return null;
    }

    public void Actualizar (Personaje personaje) {
        personajeLigado = null;
        textoNombre.text = personaje.nombre;
        textoNivel.text = personaje.attributes.GetLevel().ToString();

        imagenCuerpo.color = personaje.body.color;
        imagenCara.sprite = personaje.head.sprite;
        imagenCara.color = personaje.head.color;
        imagenMascara.sprite = personaje.mask.sprite;
        imagenMascara.color = personaje.mask.color;

        imagenAnimo.sprite = GetMood(personaje.GetMood());

        for (int i = 0; i < barraAtributo.Length; i++) {
            barraAtributo[i].GetValue(personaje.attributes.GetLevel(barraAtributo[i].estado), personaje.attributes.GetPorc (barraAtributo[i].estado));
        }

        personajeLigado = personaje;

        panelCharacter.SetActive(true);
    }

    void OnNameChange (string newName) {
        if (personajeLigado == null) {
            return;
        }

        personajeLigado.nombre = newName;

        characters.ActualizarPersonaje(personajeLigado);
    }
}

[System.Serializable]
public class EstadoAnimo {
    public ESTADOANIMO animo;
    public Sprite sprite;
}

[System.Serializable]
public class BarraAtributo {
    public ATRIBUTO estado;

    public Image[] imagen;
    public Text textoNivel;
    public Image experiencia;

    public void GetValue (int nivel, float porc) {
        textoNivel.text = nivel.ToString();

        for (int i = 0; i < imagen.Length; i++) {
            imagen[i].color = new Color(imagen[i].color.r, imagen[i].color.g, imagen[i].color.b, 0.25f);
        }

        if (nivel > 0) {
            for(int i = 3; i < imagen.Length; i++) {
                if (nivel > i-3) {
                    imagen[i].color = new Color(imagen[i].color.r, imagen[i].color.g, imagen[i].color.b, 1.0f);
                }
            }
        } else if (nivel < 0) {
            for(int i = 0; i < 3; i++) {
                if(nivel <= i - 3) {
                    imagen[i].color = new Color(imagen[i].color.r, imagen[i].color.g, imagen[i].color.b, 1.0f);
                }
            }
        }
        
        if (experiencia!=null)
            experiencia.rectTransform.SetSizeWithCurrentAnchors  (RectTransform.Axis.Horizontal, 490*porc);
    }
}