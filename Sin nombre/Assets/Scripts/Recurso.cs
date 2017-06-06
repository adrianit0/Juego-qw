using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recurso : Estructura {

    public TIPOACCION actionType;
    public int maxQuantity = 25, actualQuantity = 25, maxWaveQuantity = 25;

    public Sprite[] sprites;
    public RecursoObject[] recursos;
    
    public SpriteRenderer render;

    void Awake() {
        render = GetComponent<SpriteRenderer>();
        SetSprite();
    }

    void SetSprite () {
        float porc = ((float) actualQuantity) / ((float) maxQuantity);

        int pos = sprites.Length - Mathf.CeilToInt(((float) sprites.Length) * porc) -1 ;
        pos = Mathf.Clamp(pos, 0, sprites.Length-1);

        render.sprite = sprites[pos];
    }

    public ResourceInfo[] TomarRecursos (int cantidad) {
        if(cantidad == 0 || actualQuantity == 0 || recursos.Length == 0)
            return null;

        if(cantidad > maxWaveQuantity)
            cantidad = maxWaveQuantity;

        if(actualQuantity < cantidad)
            cantidad = actualQuantity;

        float probTotal = 0;
        float[] probReal = new float[recursos.Length];

        for (int i = 0; i < recursos.Length; i++) {
            probTotal += recursos[i].ratio;
        }

        if(probTotal == 0)  //No hay posibilidad de obtener ese recurso.
            return null;
        
        for(int i = 0; i < recursos.Length; i++) {
            probReal[i] = recursos[i].ratio / probTotal;
        }

        ResourceInfo[] resources = new ResourceInfo[probReal.Length];

        for (int i = 0; i < resources.Length; i++) {
            int quantity = Mathf.RoundToInt((float) cantidad * probReal[i]);
            resources[i] = new ResourceInfo(recursos[i].tipoRecurso, quantity);
        }

        actualQuantity -= cantidad;
        SetSprite();


        return resources;
    }

    public ResourceInfo[] TomarRecursos() {
        return TomarRecursos(maxWaveQuantity);
    }
}

//El objeto a obtener en los recursos
[System.Serializable]
public class RecursoObject {
    public RECURSOS tipoRecurso = RECURSOS.Madera;

    public float ratio = 5f;   //Porcentaje de que aparezca este objeto y no otro
}
