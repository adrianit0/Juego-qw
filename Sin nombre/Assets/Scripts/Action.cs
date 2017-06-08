using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acciones
/// </summary>
public class Action {
    public float totalTime;
    public float actualTime;
    public SpriteRenderer renderIcon;
    public TIPOACCION tipo;
    public Vector3 position;

    //Tipos de acciones (Según las diferentes estructuras).
    public Estructura estructure;
    public GameObject prefab;
    public Recurso resourceAction; //Solo si es un recurso.
    public Almacen warehouseAction; //Para almacenar informacion

    //Construccion
    public int buildID; //La ID de la construcción, solo cuando va a construir.
    bool canBuild = false;
    public List<ResourceInfo> recursosNecesarios;

    // El genérico para la mayoria de las acciones.
    public Action(Estructura estructure, TIPOACCION tipoAccion, Vector3 position, float duracion, SpriteRenderer render, List<ResourceInfo> recursos = null) {
        this.estructure = estructure;
        prefab = estructure.gameObject;
        resourceAction = estructure.GetComponent<Recurso>();
        warehouseAction = estructure.GetComponent<Almacen>();

        this.position = position;
        this.tipo = tipoAccion;

        totalTime = duracion;

        renderIcon = render;

        if (recursos != null) {
            recursosNecesarios = new List<ResourceInfo>(recursos);
        }
    }

    // Usar prefab en lugar de una estructura.
    public Action(GameObject build, TIPOACCION tipoAccion, Vector3 position, float duracion, SpriteRenderer render, List<ResourceInfo> recursos = null) {
        prefab = build;
        estructure = build.GetComponent<Estructura>();
        if(estructure != null) {
            resourceAction = build.GetComponent<Recurso>();
            warehouseAction = build.GetComponent<Almacen>();
        }

        this.position = position;
        this.tipo = tipoAccion;

        totalTime = duracion;
        actualTime = 0;

        renderIcon = render;

        if(recursos != null) {
            recursosNecesarios = new List<ResourceInfo>(recursos);
        }
    }

    //Especial para construcción.
    public Action(GameObject build, int buildID, Vector3 position, float duracion, SpriteRenderer render, List<ResourceInfo> recursos) {
        prefab = build;

        this.buildID = buildID;
        this.position = position;
        this.tipo = TIPOACCION.Construir;

        totalTime = duracion;
        actualTime = 0;

        renderIcon = render;

        recursosNecesarios = recursos != null ? new List<ResourceInfo>(recursos) : new List<ResourceInfo>();
    }

    public int AddResource (RECURSOS tipo, int cantidad) {
        int sobrante = 0;
        for (int i = 0; i < recursosNecesarios.Count; i++) {
            if (recursosNecesarios[i].type == tipo) {
                recursosNecesarios[i].quantity -= cantidad;

                if(recursosNecesarios[i].quantity < 0) {
                    sobrante = recursosNecesarios[i].quantity*-1;
                    recursosNecesarios[i].quantity = 0;
                }
            }
        }
        
        return sobrante;
    }

    public bool CanBuild () {
        canBuild = true;
        for(int i = 0; i < recursosNecesarios.Count; i++) {
            if(recursosNecesarios[i].quantity > 0) {
                canBuild = false;
                break;
            }
        }

        return canBuild;
    }
}

public class CustomAction {
    public TIPOACCION tipo;
    public List<ResourceInfo> recNecesarios;

    //Accion personalizada para sacar contenido del almacén.
    public CustomAction (TIPOACCION tipo, List<ResourceInfo> recNecesarios) {
        this.tipo = tipo;
        this.recNecesarios = recNecesarios;
    }
}