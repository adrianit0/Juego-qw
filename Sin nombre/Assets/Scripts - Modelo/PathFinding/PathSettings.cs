using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class PathSetting {
    PATHTYPE type;

    IntVector2 position;
    ESTRUCTURA build;

    ResourceInfo[] recursos;

    TIPOAGUA agua;
    float minimoNec;

    //CONSTRUCTORES

    //Busca el mejor camino a una posición en concreto.
    public PathSetting(IntVector2 posicion) {
        type = PATHTYPE.Posicion;

        this.position = posicion;
    }

    //Hace uso de un tipoPath genérico que no necesite más información, por ejemplo, el de buscar un baúl que esté vacío.
    public PathSetting(PATHTYPE type) {
        this.type = type;
    }

    //Busca el mejor camino a un baul con el contenido 
    public PathSetting(ResourceInfo[] recursos) {
        type = PATHTYPE.AlmacenObjeto;

        this.recursos = recursos;
    }

    //Busca el agua más cercano partiendo del tipo de agua con mínimo de porcentaje (Por ejemplo, buscar agua que contenga más de 75% de agua dulce).
    public PathSetting(TIPOAGUA agua, float minimoNecesario) {
        type = PATHTYPE.Agua;

        this.agua = agua;
        minimoNec = minimoNecesario;
    }

    public IntVector2 GetPosition () {
        return position;
    }

    public PATHTYPE GetPathType () {
        return type;
    }

    public TIPOAGUA GetWaterType() {
        return agua;
    }

    public float GetWaterMin () {
        return minimoNec;
    }

    public int ResourceCount () {
        if(recursos == null)
            return 0;

        return recursos.Length;
    }

    public bool Value(RECURSOS type) {
        for(int i = 0; i < ResourceCount(); i++) {
            if(type == recursos[i].type && recursos[i].quantity > 0) {
                return true;
            }
        }

        return false;
    }
}