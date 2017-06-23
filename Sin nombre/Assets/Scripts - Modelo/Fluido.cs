using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluido  {


    public int litrosTotales { get; private set; }
    public int litrosMaximo { get; private set; }

    float porcAguaDulce;
    float porcAguaSalada;
    float porcAguaCont;

    public Fluido(int litrosMaximo) {
        this.litrosMaximo = litrosMaximo;

        litrosTotales = 0;

        porcAguaDulce = 0;
        porcAguaSalada = 0;
        porcAguaCont = 0;
    }

    public Fluido(int litros, float porcDulce, float porcSalada, float porcCont, int maxLitros) {
        float total = porcDulce + porcSalada + porcCont;

        litrosTotales = total > 0 ? litros : 0;
        litrosMaximo = maxLitros;

        porcAguaDulce = total > 0 ? porcDulce / total : 0;
        porcAguaSalada = total > 0 ? porcSalada / total : 0;
        porcAguaCont = total > 0 ? porcCont / total : 0;
    }

    public Fluido(float litrosDulce, float litrosSalada, float litrosCont, int maxLitros = 1000) {
        float total = litrosDulce + litrosSalada + litrosCont;
        litrosTotales = Mathf.RoundToInt(total);
        litrosMaximo = maxLitros;

        porcAguaDulce = litrosDulce / total;
        porcAguaSalada = litrosSalada / total;
        porcAguaCont = litrosCont / total;
    }

    public Fluido(Fluido otroFluido) {
        litrosTotales = otroFluido.litrosTotales;
        litrosMaximo = otroFluido.litrosMaximo;

        porcAguaDulce = otroFluido.porcAguaDulce;
        porcAguaSalada = otroFluido.porcAguaSalada;
        porcAguaCont = otroFluido.porcAguaCont;
    }

    public Fluido (int litrosTomados, Fluido otroFluido, int maxLitros = 1000) {
        litrosMaximo = maxLitros;

        TomarAgua(litrosTomados, otroFluido);
    }

    public void TomarAgua (int litrosTomados, Fluido otroFluido) {
        //QUE HACER:
        // - Quitar los litros tomados al otro fluido
        litrosTotales = litrosTomados;
        otroFluido.litrosTotales -= litrosTomados;

        otroFluido.ConsumirAgua(litrosTomados);

        porcAguaDulce = otroFluido.porcAguaDulce;
        porcAguaSalada = otroFluido.porcAguaSalada;
        porcAguaCont = otroFluido.porcAguaCont;
    }

    public void SetMaxWater(int max) {
        litrosMaximo = max;
    }

    public float GetWater(TIPOAGUA agua) {
        switch(agua) {
            case TIPOAGUA.AguaDulce:
                return porcAguaDulce;
            case TIPOAGUA.AguaSalada:
                return porcAguaSalada;
            case TIPOAGUA.AguaContaminada:
                return porcAguaCont;
        }

        return 0;
    }

    /// <summary>
    /// Consume agua, devuelve true si ha agotado todo el agua.
    /// </summary>
    /// <param name="litrosConsumidos"></param>
    /// <returns></returns>
    public bool ConsumirAgua (float litrosConsumidos) {
        litrosTotales = Mathf.Clamp(litrosTotales - litrosTotales, 0, litrosMaximo);

        return litrosTotales == 0;
    }

}
