using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrafteoBoton : MonoBehaviour {
    
    public IconoCrafteoBoton[] requisitos = new IconoCrafteoBoton[4];
    public IconoCrafteoBoton obtencion;

    public Image fondo;
    public Text tiempoTotal;

    public Button botonAñadir;

    public void Configurar (Craft info, GameManager manager) {
        for (int i = 0; i < requisitos.Length; i++) {
            if (i >= info.requisitos.Length) 
                continue;

            requisitos[i].cantidad.text = info.requisitos[i].quantity.ToString();
            requisitos[i].imagen.sprite = manager.resourceController.GetSprite(info.requisitos[i].type);
        }

        obtencion.cantidad.text = info.obtencion.quantity.ToString();
        obtencion.imagen.sprite = manager.resourceController.GetSprite(info.obtencion.type);

        tiempoTotal.text = info.tiempo.ToString() + "s";
    }

    public void SetInteractable (bool value) {
        if(value == botonAñadir.interactable)
            return;

        fondo.color = (value) ? new Color(0.5f, 1, 0.5f, 0.67f) : new Color(1f, 0.5f, 0.5f, 0.67f);
        botonAñadir.interactable = value;
    }
}

[System.Serializable]
public class IconoCrafteoBoton {
    public Image imagen;
    public Text cantidad;

    public void SetImage (Sprite sprite, int cantidad) {
        imagen.sprite = sprite;
        this.cantidad.text = cantidad.ToString();
    }
}
