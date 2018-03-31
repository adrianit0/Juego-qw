using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TIPOAGUA { AguaDulce, AguaSalada, AguaContaminada }

public class Agua : Estructura , IEstructura {

    public Fluido agua;

    [Range(0, 1)]
    public float cantPeces = 0.67f;

    public Sprite spriteExtraer;

    public void OnStart() {
        agua = new Fluido(875, 1, 0, 0);
    }

    public bool Pescar () {
        bool obtenido = Random.value < cantPeces;

        if (obtenido) {
            float peces = agua.litrosTotales * cantPeces;
            peces--;

            cantPeces = peces / agua.litrosTotales;

        }

        return obtenido;
    }

    public string OnText() {
        //EXTRAER AGUA
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.ExtraerAgua), "Extraer", true, () => {
            manager.actions.CreateAction(transform.position, HERRAMIENTA.Custom, TIPOACCION.ExtraerAgua, null, false, -1, null);
        });

        //PESCAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Pescar), "Pescar", true, () => {
            manager.actions.CreateAction(transform.position, HERRAMIENTA.Custom, TIPOACCION.Pescar, null, false, -1, null);
        });

        return
            "<b>Litros totales</b>: " + agua.litrosTotales + " L \n\n" +
            "<b>Agua dulce</b>: "+ Mathf.RoundToInt (agua.GetWater (TIPOAGUA.AguaDulce) * 100 ) +"% \n" +
            "<b>Agua salada</b>: " + Mathf.RoundToInt(agua.GetWater(TIPOAGUA.AguaSalada) * 100) + "%  \n" +
            "<b>Agua contam</b>: " + Mathf.RoundToInt(agua.GetWater(TIPOAGUA.AguaContaminada) * 100) + "%  \n" +
            "<b>Otras sustancias</b>: 0%  \n\n" +
            "<b>Peces:</b> " + TextoCantidadPeces (cantPeces) + ".";
                
    }

    public string TextoCantidadPeces (float cantidad) {
        if (cantidad == 0) {
            return "Ninguno";
        } else if (cantidad < 0.05f) {
            return "Mínimo";
        } else if (cantidad < 0.15f) {
            return "Pocos";
        } else if (cantidad < 0.35f) {
            return "Normal";
        } else if (cantidad < 0.5f) {
            return "Bastante";
        } else {
            return "Lleno";
        }
    }

    public string OnTextGroup(Estructura[] estructuras) {
        int litrosTotales = 0;
        for (int i = 0;i < estructuras.Length;i++) {
            litrosTotales += estructuras[i].GetComponent<Agua>().agua.litrosTotales;
        }

        return
            "<b>Litros totales</b>: " + litrosTotales + " L \n\n" +
            "???";
    }

    public void OnDestroyBuild() {

    }
}