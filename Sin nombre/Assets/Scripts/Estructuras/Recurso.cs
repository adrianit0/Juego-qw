using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recurso : Estructura, IEstructura {

    public TIPOACCION actionType;
    public bool fixedContent = false;           //Si esto está activado el ratio se convierte en la cantidad total a recoger.
    public bool destruirTrasUsarse = false;     //Si esto está activado el objeto se destruye tras ser usado al 100%.
    public int maxQuantity = 25, actualQuantity = 25, maxWaveQuantity = 25;

    public Sprite[] sprites;
    public RecursoObject[] recursos;

    new void Awake() {
        base.Awake();
    }

    public void OnStart () {
        SetSprite();
    }

    void SetSprite () {
        float porc = ((float) actualQuantity) / ((float) maxQuantity);

        int pos = sprites.Length - Mathf.CeilToInt(((float) sprites.Length) * porc) -1 ;
        pos = Mathf.Clamp(pos, 0, sprites.Length-1);

        render.sprite = sprites[pos];

        if (porc==0 && destruirTrasUsarse) {
            manager.RemoveBuildInMap(transform.position);
        }
    }

    public ResourceInfo[] GetResource (int cantidad) {
        if(cantidad == 0 || actualQuantity == 0 || recursos.Length == 0)
            return null;

        if(cantidad > maxWaveQuantity)
            cantidad = maxWaveQuantity;

        if(actualQuantity < cantidad)
            cantidad = actualQuantity;

        ResourceInfo[] resources = (fixedContent) ? GetResourceFixed(cantidad) : GetResourceWave(cantidad);
        
        actualQuantity -= cantidad;
        SetSprite();

        return resources;
    }

    public ResourceInfo[] GetResource() {
        return GetResource(maxWaveQuantity);
    }

    ResourceInfo[] GetResourceWave (int cantidad) {
        float probTotal = 0;
        float[] probActual = new float[recursos.Length];
        float[] probReal = new float[recursos.Length];

        for(int i = 0; i < recursos.Length; i++) {
            probActual[i] = Random.Range(recursos[i].ratio - recursos[i].ratioRange, recursos[i].ratio + recursos[i].ratioRange);
            probTotal += probActual[i];
        }

        if(probTotal == 0)  //No hay posibilidad de obtener ese recurso.
            return null;

        for(int i = 0; i < recursos.Length; i++) {
            probReal[i] = probActual[i] / probTotal;
        }

        ResourceInfo[] resources = new ResourceInfo[probReal.Length];

        for(int i = 0; i < resources.Length; i++) {
            int quantity = Mathf.RoundToInt((float) cantidad * probReal[i]);
            resources[i] = new ResourceInfo(recursos[i].tipoRecurso, quantity);
        }

        return resources;
    }

    ResourceInfo[] GetResourceFixed(int cantidad) {
        ResourceInfo[] resources = new ResourceInfo[recursos.Length];

        int total = 0;
        for(int i = 0; i < resources.Length; i++) {
            int quantity = Mathf.Clamp (Mathf.RoundToInt(recursos[i].ratio), 0, cantidad-total);
            resources[i] = new ResourceInfo(recursos[i].tipoRecurso, quantity);
            recursos[i].ratio -= quantity;
            total += quantity;
        }

        return resources;
    }

    public void CreateResource (params ResourceInfo[] info) {
        if(info == null || info.Length == 0)
            return;

        recursos = new RecursoObject[info.Length];

        int total = 0;
        for (int i = 0; i < info.Length; i++) {
            recursos[i] = new RecursoObject(info[i].type, info[i].quantity);
            total += info[i].quantity;
        }

        actualQuantity = total;
        maxQuantity = total;
    }

    public string OnText() {
        string text = "<b>Estado:</b> " + Mathf.Round(((float) actualQuantity) / ((float) maxQuantity)*100) + "%\n\n";
        text += "<b>Contiene:</b>\n";

        if (fixedContent) {
            if(actualQuantity > 0) {
                for(int i = 0; i < recursos.Length; i++) {
                    if(recursos[i].ratio > 0) {
                        text += "<b>" + recursos[i].tipoRecurso.ToString() + ":</b> " + recursos[i].ratio + "\n";
                    }
                }
            } else {
                text += "Está vacío.";
            }

            text += "\n\n<b>Atención:</b> Esta bolsa desaparecerá si no la recoges antes de media noche";
        } else {
            float probTotal = 0;

            for(int i = 0; i < recursos.Length; i++) {
                probTotal += recursos[i].ratio;
            }

            if(recursos.Length > 0) {
                for(int i = 0; i < recursos.Length; i++) {
                    text += "<b>" + recursos[i].tipoRecurso.ToString() + ":</b> " + Mathf.RoundToInt((recursos[i].ratio / probTotal) * 100) + "%\n";
                }
            } else {
                text += "No produce nada.";
            }
        }

        return text;
    }

    public string OnTextGroup(Estructura[] estructuras) {

        return "";
    }

    public void OnDestroyBuild() {

    }
}

//El objeto a obtener en los recursos
[System.Serializable]
public class RecursoObject {
    public RECURSOS tipoRecurso = RECURSOS.Madera;

    public float ratio = 5f;        //Porcentaje de que aparezca este objeto y no otro
    public float ratioRange = 0f;   //Esto es para hacer aleatorio lo que da

    public RecursoObject () {
        tipoRecurso = RECURSOS.Madera;

        ratio = 5;
        ratioRange = 0f;
    }

    public RecursoObject (RECURSOS tipo, float cantidad) {
        tipoRecurso = tipo;

        ratio = cantidad;
        ratioRange = 0;
    }
}
