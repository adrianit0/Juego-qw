using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterUpdate {

    Dictionary<Personaje, CharacterPanel> personajes;
    GameManager manager;
    CharacterInterfaceController controller;

    public CharacterUpdate (GameManager manager, CharacterInterfaceController controller) {
        this.manager = manager;
        this.controller = controller;

        personajes = new Dictionary<Personaje, CharacterPanel>();
    }

    public void AñadirPersonaje(Personaje character) {
        if(personajes.ContainsKey(character)) {
            Debug.LogWarning("CharacterInterfaceController::AñadirPersonaje error: Ya existe un personaje así.");
            return;
        }

        if(manager == null) {
            Debug.LogWarning("CharacterInterfaceController::AñadirPersonaje error: No existe el manager.");
            return;
        }

        GameObject obj = GameObject.Instantiate(controller.characterInterfacePrefab);
        obj.transform.SetParent(controller.panelCharacterInterface.transform);
        obj.transform.localScale = new Vector3(1, 1, 1);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(60 + 100 * personajes.Count, -25);

        CharacterPanel script = obj.GetComponent<CharacterPanel>();
        script.SetCharacter(character);

        script.boton.onClick.AddListener(() => { manager.characterController.Actualizar(character); });

        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        //Entrar
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => { script.subPanel.SetActive(true); });
        trigger.triggers.Add(entry);
        //Salir
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((eventData) => { script.subPanel.SetActive(false); });
        trigger.triggers.Add(exit);

        script.subPanel.SetActive(false);

        personajes.Add(character, script);
    }

    public void ActualizarPersonaje(Personaje character) {
        if(!personajes.ContainsKey(character)) {
            Debug.LogWarning("CharacterInterfaceController::character error: Ese personaje no está en la lista.");
            return;
        }

        personajes[character].ActualizarContenido();
    }
}
