using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ACTIONEVENT { OnAwake, BeforeStart, OnCompleted, OnCanceled }

/// <summary>
/// Acciones
/// </summary>
public class GameAction {
    public float totalTime;

    public Sprite originalSprite { get; private set; }
    public HERRAMIENTA herramienta { get; private set; }
    public TIPOACCION tipo { get; private set; }
    public int prioridad { get; private set; }
    public Node node { get; private set; }

    public Personaje worker { get; private set; }   // Personaje asignado esta acción

    Action<GameAction> actionOnStart;               // Justo en el momento de asignar la acción a algún personaje.
    Action<GameAction> actionBeforeStart;           // Que hacer justo antes de empezar la mision. Ejemplo, mira si tienes los recursos necesarios para construir una base.
    Action<GameAction> actionCompleted;             // Que hacer tras completar la acción
    Action<GameAction> actionCanceled;              // Que hacer si se cancela la acción

    public ResourceInfo[] recursosNecesarios { get; private set; }
    public ResourceInfo[] recursosActuales { get; private set; }

    //Variables de control
    public bool desactivado { get; private set; }
    float tiempoNec = 0f;

    ActionsQueue queue;

    // El genérico para la mayoria de las acciones.
    public GameAction(TIPOACCION tipoAccion, HERRAMIENTA herramienta, Node node, Personaje worker, float duration, int prioridad, ActionsQueue queue) {
        this.node = node;
        this.tipo = tipoAccion;
        this.herramienta = herramienta;
        this.prioridad = prioridad;
        this.queue = queue;

        totalTime = duration;

        if(worker != null)
            AssignCharacter(worker);
    }
    
    public void AssignCharacter (Personaje character) {
        if (character == null) {
            return;
        }

        if (worker != null) {
            Debug.LogWarning("AssignCharacter: La accion: "+ tipo.ToString() + " con herramienta: "+ herramienta.ToString() +" ya tiene personaje ligado. Ligado: "+worker.name+" Nuevo: "+character.name);
            return;
        }

        worker = character;
        //Añade la acción directamente a su cola de acciones.
        character.AddAction(this);

        if (originalSprite != null)
            queue.ChangeColor (this, new Color(0.6f, 0.6f, 0.6f, 0.8f));
    }

    public void UnassingCharacter() {
        if(worker == null) {
            Debug.LogWarning("UnassingCharacter: Actualmente no existe trabajador seleccionado para esta acción");
            return;
        }

        //Elimina la accion directamente desde su cola de acciones.
        worker = null;
    }

    public void SetTime (float newTime) {
        totalTime = newTime;
    }

    public void SetSprite (Sprite sprite, SpriteRenderer render = null) {
        if(sprite == null) {
            Debug.Log("GameAction::SetSprite error: No hay ningún sprite para añadir a la acción.");
        }

        originalSprite = sprite;

        if (render != null) {
            render.sprite = sprite;
        }
    }

    public void ChangeSpriteToOriginal (SpriteRenderer render) {
        if (render == null) {
            Debug.Log("GameAction::AddRender error: No hay ningún render para añadir a la acción.");
        }

        render.sprite = originalSprite;
        //render.color = new Color(1, 1, 1, 0.8f);
    }

    public void ChangePriority (int newPriority) {
        newPriority = Mathf.Clamp(newPriority, 0, 4);
        prioridad = newPriority;
    }
    
    //Añade el contenido
    public void SetResources (ResourceInfo[] recursos) {
        if(recursos == null)
            return;

        recursosNecesarios = new ResourceInfo[recursos.Length];
        recursosActuales = new ResourceInfo[recursos.Length];

        for (int i = 0; i < recursosActuales.Length; i++) {
            recursosNecesarios[i] = new ResourceInfo(recursos[i].type, recursos[i].quantity);
            recursosActuales[i] = new ResourceInfo(recursos[i].type, 0);
        }
    }

    public int AddResource (RECURSOS tipo, int cantidad) {
        int sobrante = 0;
        for (int i = 0; i < recursosNecesarios.Length; i++) {
            if (recursosNecesarios[i].type == tipo) {
                recursosNecesarios[i].quantity -= cantidad;
                
                if(recursosNecesarios[i].quantity < 0) {
                    sobrante = recursosNecesarios[i].quantity*-1;
                    recursosNecesarios[i].quantity = 0;
                }

                recursosActuales[i].quantity += cantidad - sobrante;
            }
        }
        
        return sobrante;
    }

    public bool CanBuild () {
        if (recursosNecesarios==null) {
            Debug.LogWarning("GameAction::CanBuild: No puede saber si se quiere construir algo porque no te pide ningún objeto");
            return false;
        }

        for(int i = 0; i < recursosNecesarios.Length; i++) {
            if(recursosNecesarios[i].quantity > 0) {
                return false;
            }
        }

        return true;
    }

    //Desactiva la accion.
    public void Desactivar (float tiempo) {
        desactivado = true;
        tiempoNec = tiempo;

        if(originalSprite != null)
            queue.ChangeColor(this, new Color(1, 0, 0, 0.8f));
    }

    //Actualiza cada cierto tiempo hasta volver 
    public void ActualizarDesactivar (float suma) {
        tiempoNec -= suma;
        if (tiempoNec<0) {
            desactivado = false;

            if(originalSprite != null)
                queue.ChangeColor(this, new Color(0.6f, 0.6f, 0.6f, 0.8f));
        }
    }

    public void RegisterAction (ACTIONEVENT tipo, Action<GameAction> action) {
        switch (tipo) {
            case ACTIONEVENT.OnAwake:
                actionOnStart += action;
                break;

            case ACTIONEVENT.BeforeStart:
                actionBeforeStart += action;
                break;

            case ACTIONEVENT.OnCompleted:
                actionCompleted += action;
                break;

            case ACTIONEVENT.OnCanceled:
                actionCanceled += action;
                break;
        }
    }

    public void UnregisterAction(ACTIONEVENT tipo, Action<GameAction> action) {
        switch(tipo) {
            case ACTIONEVENT.OnAwake:
                actionOnStart -= action;
                break;

            case ACTIONEVENT.BeforeStart:
                actionBeforeStart -= action;
                break;

            case ACTIONEVENT.OnCompleted:
                actionCompleted -= action;
                break;

            case ACTIONEVENT.OnCanceled:
                actionCanceled -= action;
                break;
        }
    }

    public void RealizeAction (ACTIONEVENT tipo) {
        switch(tipo) {
            case ACTIONEVENT.OnAwake:
                if (actionOnStart != null)
                    actionOnStart(this);
                break;

            case ACTIONEVENT.BeforeStart:
                if(actionBeforeStart != null)
                    actionBeforeStart(this);
                break;

            case ACTIONEVENT.OnCompleted:
                if(actionCompleted != null)
                    actionCompleted(this);
                break;

            case ACTIONEVENT.OnCanceled:
                if(actionCanceled != null)
                    actionCanceled(this);
                break;
        }
    }
}