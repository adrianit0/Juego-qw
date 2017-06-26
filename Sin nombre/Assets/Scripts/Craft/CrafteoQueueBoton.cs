using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrafteoQueueBoton : MonoBehaviour {

    public Button boton;

    public IconoCrafteoBoton obtencion;

    public void Configurar (Craft info, GameManager manager) {
        obtencion.cantidad.text = info.obtencion.quantity.ToString();
        obtencion.imagen.sprite = manager.resourceController.GetSprite(info.obtencion.type);

        boton.onClick.AddListener(() => {
            //Cancelarlo
        });
    }

}
