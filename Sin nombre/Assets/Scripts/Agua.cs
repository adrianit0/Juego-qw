using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agua : Estructura , IEstructura {

    public int aguaDulce = 0;
    public int aguaSalada = 879;
    public int aguaContaminada = 0;

    public float cantPeces = 0.67f;

    public bool Pescar () {
        return (Random.value < cantPeces);
    }

    public string OnText() {

        return
            "<b>Litros totales</b>: " + aguaSalada + " L \n\n" +
            "<b>Agua dulce</b>: 0% \n" +
            "<b>Agua salada</b>: 100%  \n" +
            "<b>Agua contam</b>: 0%  \n" +
            "<b>Otras sustancias</b>: 0%  \n\n" +
            "<b>Peces:</b> Lleno. \n" +
            " - <b>Sardina</b>: 87%\n"+
            " - <b>Lubina</b>: 13%\n";
                
    }

    public void OnDestroyBuild() {

    }
}
