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

    }

    public bool Pescar () {
        return (Random.value < cantPeces);
    }

    public string OnText() {
        //EXTRAER AGUA
        manager.info.ActivarBoton(0, spriteExtraer, "Extraer", true, () => {
            manager.AddAction(transform.position, HERRAMIENTA.Custom, new CustomAction(TIPOACCION.ExtraerAgua, false, null));
        });

        return
            "<b>Litros totales</b>: " + agua.litrosTotales + " L \n\n" +
            "<b>Agua dulce</b>: "+ Mathf.RoundToInt (agua.porcAguaDulce*100) +"% \n" +
            "<b>Agua salada</b>: " + Mathf.RoundToInt(agua.porcAguaSalada * 100) + "%  \n" +
            "<b>Agua contam</b>: " + Mathf.RoundToInt(agua.porcAguaCont * 100) + "%  \n" +
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

[System.Serializable]
public struct Fluido {
    public int litrosTotales;

    public float porcAguaDulce;
    public float porcAguaSalada;
    public float porcAguaCont;


    public Fluido (int litros, float porcDulce, float porcSalada, float porcCont) {
        float total = porcDulce + porcSalada + porcCont;

        litrosTotales = total > 0 ? litros : 0;

        porcAguaDulce = total>0 ? porcDulce / total : 0;
        porcAguaSalada = total > 0 ? porcSalada / total : 0;
        porcAguaCont = total > 0 ? porcCont / total : 0;
    }

    public Fluido (float litrosDulce, float litrosSalada, float litrosCont) {
        float total = litrosDulce + litrosSalada + litrosCont;
        litrosTotales = Mathf.RoundToInt(total);

        porcAguaDulce = litrosDulce / total;
        porcAguaSalada = litrosSalada / total;
        porcAguaCont = litrosCont / total;
    }

    public Fluido (Fluido otroFluido) {
        litrosTotales = otroFluido.litrosTotales ;

        porcAguaDulce = otroFluido.porcAguaDulce;
        porcAguaSalada = otroFluido.porcAguaSalada;
        porcAguaCont = otroFluido.porcAguaCont;
    }

    public Fluido(int litrosTomados, Fluido otroFluido) {
        //QUE HACER:
        // - Quitar los litros tomados al otro fluido
        litrosTotales = litrosTomados;
        otroFluido.litrosTotales -= litrosTomados;

        porcAguaDulce = otroFluido.porcAguaDulce;
        porcAguaSalada = otroFluido.porcAguaSalada;
        porcAguaCont = otroFluido.porcAguaCont;
    }

    public float GetWater (TIPOAGUA agua) {
        switch (agua) {
            case TIPOAGUA.AguaDulce:
                return porcAguaDulce;
            case TIPOAGUA.AguaSalada:
                return porcAguaSalada;
            case TIPOAGUA.AguaContaminada:
                return porcAguaCont;
        }

        return 0;
    } 
}