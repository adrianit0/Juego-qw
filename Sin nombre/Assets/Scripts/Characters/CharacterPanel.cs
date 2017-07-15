using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour {

    public Button boton;

    public Text nombre;

    public Text nivel;
    public Text salud;
    public Text hambre;
    public Text stress;

    public Image estadoAnimo;

    //Personaje cuerpo
    public Image body;
    public Image head;
    public Image mask;

    public GameObject subPanel;

    Personaje character;

    public void SetCharacter (Personaje character) {
        this.character = character;

        ActualizarContenido(true);
    }

    public void ActualizarContenido (bool actualizarCuerpo = false) {
        if (character==null) {
            Debug.LogWarning("CharacterPanel::ActualizarContenido error: No existe personaje.");
            return;
        }

        nombre.text = character.nombre.ToString();
        nivel.text = character.attributes.GetLevel().ToString();
        salud.text = character.attributes.GetLevel (ATRIBUTO.Salud).ToString();
        hambre.text = character.attributes.GetLevel(ATRIBUTO.Hambre).ToString();
        stress.text = character.attributes.GetLevel(ATRIBUTO.Estres).ToString();

        estadoAnimo.sprite = character.manager.characterController.GetMood(character.GetMood());

        if (actualizarCuerpo) {
            body.color = character.body.color;
            head.sprite = character.head.sprite;
            head.color = character.head.color;
            mask.sprite = character.mask.sprite;
            mask.color = character.mask.color;
        }
    }
}
