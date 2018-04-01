using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO:
// Solo regar plantas que no tenga agua o que tenga muy poca
public class Huerto : Estructura, IEstructura {

    public Cultivo cultivo = null;
    public Fluido agua = new Fluido(0, 0, 0, 0);
    public float tiempoCreciendo = 0;
    public float tiempoAgua = 0;
    
    //Se el contenido proximamente aquí
    public SpriteRenderer renderCultivo;

    public Sprite sueloSeco, sueloMojado;

    new void Awake() {
        base.Awake();

        cultivo = null;
    }

    public void OnStart () {

    }

    public void OnUpdate(float delta) {
        if(cultivo != null && agua.litrosTotales > 0) {
            tiempoCreciendo += Time.deltaTime;
            tiempoAgua += Time.deltaTime;

            if(tiempoAgua > (60 / cultivo.litrosPorMinuto)) {
                tiempoAgua = 0;
                if(agua.ConsumirAgua(1)) {
                    renderCultivo.sprite = sueloSeco;
                }
            }

            SetSprite(Mathf.Clamp(tiempoCreciendo / cultivo.tiempoCrecer, 0, 1));
        }
    }

    void SetSprite(float porc) {
        int pos = Mathf.CeilToInt(((float) cultivo.sprite.Length) * porc);
        pos = Mathf.Clamp(pos, 0, cultivo.sprite.Length - 1);

        renderCultivo.sprite = cultivo.sprite[pos];

        if(porc == 1) {
            Debug.Log("Ya ha crecido");
            manager.RemoveBuildInMap(transform.position);

            manager.CreateBuild(transform.position, cultivo.cultivoPrefab);

            Destroy(this.gameObject);
        }
    }

    public void Regar (Fluido agua) {
        this.agua = new Fluido(1, agua);
        render.sprite = sueloMojado;
    }
    
    public void Cultivar (RECURSOS recurso) {
        int value = -1;
        Agricultura agri = manager.GetComponent<Agricultura>();
        for (int i = 0; i < agri.semillas.Length; i++) {
            if (agri.semillas[i].semilla == recurso) {
                value = i;
                break;
            }
        }

        if (value==-1) {
            return;
        }

        cultivo = new Cultivo (agri.semillas[value]);
        renderCultivo.sortingOrder = manager.SetSortingLayer(transform.position.y) + 1;
        renderCultivo.sprite = cultivo.sprite[0];
    }


    public string OnText() {
        if (cultivo==null) {
            //CULTIVAR
            manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Plantar), "Cultivar", true, () => {
                manager.GetComponent<Agricultura>().AbrirPanel(1);
            });
        } else {
            //FERTILIZAR
            manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Arar), "Fertilizar", false, () => {
                //Añadir aquí el contenido
            });
        }

        //REGAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Regar), "Regar", true, () => {
            manager.actions.CreateAction(transform.position, HERRAMIENTA.Custom, TIPOACCION.Regar, null, false);
        });

        return "";
    }

    public string OnTextGroup(Estructura[] estructuras) {
        int cantidadSinSembrar = 0;
        Huerto[] huertos = new Huerto[estructuras.Length];
        for (int i = 0; i < estructuras.Length; i++) {
            huertos[i] = estructuras[i].GetComponent<Huerto>();

            if (huertos[i].cultivo == null) {
                cantidadSinSembrar++;
            }
        }

        if (cantidadSinSembrar > 0) {
            //CULTIVAR
            manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Plantar), "Cultivar", true, () => {
                manager.GetComponent<Agricultura>().AbrirPanel(cantidadSinSembrar);
            });
        } else {
            //FERTILIZAR
            manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Arar), "Fertilizar", true, () => {
                //Añadir aquí el contenido
            });
        }

        //REGAR
        manager.info.AddActionButton(manager.GetIconSprite(TIPOACCION.Regar), "Regar", true, () => {
            for(int i = 0; i < huertos.Length; i++) {
                manager.actions.CreateAction(huertos[i].transform.position, HERRAMIENTA.Custom, TIPOACCION.Regar, null, false);
            }
        });

        return "";
    }

    public void OnDestroyBuild() {

    }
}
