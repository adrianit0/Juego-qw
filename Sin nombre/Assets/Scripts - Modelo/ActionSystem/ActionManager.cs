using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager {
    
    public ActionsQueue _actions { get; private set;}

    GameManager manager;
    ActionMethods methods;

    public ActionManager (GameManager manager) {
        this.manager = manager;
        
        _actions = new ActionsQueue(manager);

        methods = new ActionMethods(manager, this);
    }

    public GameAction CreateAction(IntVector2 pos, HERRAMIENTA herramienta, TIPOACCION type, Personaje character = null, bool repeatable = false, ResourceInfo[] recNecesarios = null) {
        if(pos.x < 0 || pos.y < 0 || pos.x >= manager.totalSize.x || pos.y >= manager.totalSize.y)
            return null;

        if(herramienta == HERRAMIENTA.Cancelar)
            repeatable = true;

        //Mira si esta acción ya ha sido seleccionada por una acción anterior.
        //Se puede crear la acción para que se pueda realizar la misma acción con más de un personaje al mismo tiempo.
        if(!repeatable && _actions.IsActionCreated (pos)) {
            return null;
        }

        Node nodo = manager.GetNode(pos);
        Estructura build = (nodo!=null) ? nodo.GetBuild() : null;

        GameAction action = new GameAction(type, herramienta, manager.GetNode(pos), null, 0);

        float customTime = (build!=null) ? build.tiempoTotal : 1;
        Sprite customIcon = null;
        
        switch(herramienta) {
            case HERRAMIENTA.Recolectar:
                if(build == null)
                    return null;

                Recurso _resource = build.GetComponent<Recurso>();

                if(_resource != null) {
                    if(_resource.actualQuantity == 0)   //Si el recurso está vacio no te permite usarlo.
                        return null;
                    type = _resource.actionType;

                    action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.ExtraerRecursos(_resource, gameAction); });

                } else if(build.GetBuildType() == ESTRUCTURA.Agua) {
                    type = TIPOACCION.Pescar;

                    action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.Pescar(build, gameAction); });

                } else {
                    return null;
                }

                break;

            case HERRAMIENTA.Arar:
                if(build != null)
                    return null;

                type = TIPOACCION.Arar;
                customTime = manager.farm.tiempoArar;

                action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.ArarTierra(manager.farm.huertoPrefab, gameAction); });
                
                break;

            case HERRAMIENTA.Construir:
                if(build != null)
                    return null;

                type = TIPOACCION.Construir;

                int id = manager.build.selectID;

                //Almacena los recursos necesarios para la construcción de la estructura.
                recNecesarios = new ResourceInfo[manager.build.construcciones[id].recursosNecesarios.Length];
                for(int i = 0; i < recNecesarios.Length; i++) {
                    recNecesarios[i] = new ResourceInfo(manager.build.construcciones[id].recursosNecesarios[i].recurso, manager.build.construcciones[id].recursosNecesarios[i].cantidadNecesaria);
                }

                customTime = manager.build.construcciones[id].tiempo;
                customIcon = manager.build.construcciones[id].spriteModelo;

                action.RegisterAction(ACTIONEVENT.OnAwake, (gameAction) => { methods.ComprobarInventarioAction(gameAction); });
                action.RegisterAction(ACTIONEVENT.BeforeStart, (gameAction) => { methods.ComprobarInventarioAction(gameAction); });
                action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.Construir(id, gameAction); });
                action.RegisterAction(ACTIONEVENT.OnCanceled, (gameAction) => { methods.CancelarConstruccion(gameAction); });

                break;

            case HERRAMIENTA.Destruir:
                if(build == null || !build.esDestruible)
                    return null;

                type = TIPOACCION.Destruir;
                customTime = build.tiempoDestruccion;



                break;


            case HERRAMIENTA.Cancelar:
                foreach (GameAction _action in _actions.actions) { 
                    if(_action != null && (Vector2) _action.node.GetPosition () == pos) {
                        _action.RealizeAction(ACTIONEVENT.OnCanceled);
                        RemoveAction(_action);
                        return null;
                    }
                }

                return null; 

            case HERRAMIENTA.Custom:
                //Herramienta especial. Para realizar cosas que con las anteriores no se pueden (Como extraer cosas del almacen).

                switch(type) {
                    case TIPOACCION.Almacenar:
                        Almacen _almacen = build.GetComponent<Almacen>();

                        if (_almacen != null) {
                            action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.AlmacenarRecursos(_almacen, gameAction); });
                        } else {
                            return null;
                        }
                        

                        break;
                    case TIPOACCION.SacarAlmacen:
                        _almacen = build.GetComponent<Almacen>();

                        if (_almacen != null) {
                            action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.SacarContenidoAlmacen(recNecesarios, _almacen, gameAction); });
                        } else {
                            return null;
                        }

                        break;

                    case TIPOACCION.VaciarAlmacen:
                        _almacen = build.GetComponent<Almacen>();

                        if(_almacen.inventario.Count > 0) {
                            customTime = 0.5f;
                            customIcon = SearchIcon(TIPOACCION.SacarAlmacen);

                            action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.VaciarAlmacen(_almacen, gameAction); });

                        } else {
                            return null;
                        }
                        break;

                    case TIPOACCION.Plantar:
                        customTime = 0.50f;
                        customIcon = SearchIcon(TIPOACCION.Almacenar);

                        action.RegisterAction(ACTIONEVENT.OnAwake, (gameAction) => { methods.ComprobarInventarioAction(gameAction); });
                        action.RegisterAction(ACTIONEVENT.BeforeStart, (gameAction) => { methods.ComprobarInventarioAction(gameAction); });

                        action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.Plantar(build, gameAction); });

                        break;

                    case TIPOACCION.ExtraerAgua:
                        customTime = 0.5f;
                        customIcon = SearchIcon(TIPOACCION.SacarAlmacen);

                        action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.ExtraerAgua(build, gameAction); });

                        break;

                    case TIPOACCION.Regar:
                        customTime = 0.25f;
                        customIcon = SearchIcon(TIPOACCION.Almacenar);

                        action.RegisterAction(ACTIONEVENT.OnAwake, (gameAction) => { methods.ComprobarAgua(gameAction); });
                        action.RegisterAction(ACTIONEVENT.BeforeStart, (gameAction) => { methods.ComprobarAgua(gameAction); });

                        action.RegisterAction(ACTIONEVENT.OnCompleted, (gameAction) => { methods.Regar(build, gameAction); });

                        break;
                }

                break;

            default:
                Debug.LogWarning("Herramienta no programada aún");
                return null;
        }

        GameObject icon_go = GameObject.Instantiate(manager.actionIconPrefab);
        SpriteRenderer actionRender = icon_go.GetComponent<SpriteRenderer>();
        icon_go.transform.position = (Vector3) pos;

        actionRender.sprite = (customIcon==null) ? SearchIcon(type) : customIcon;

        action.SetTime(customTime);
        action.AddRender(actionRender);
        action.SetResources(recNecesarios);

        action.AssignCharacter(character);
        _actions.AddAction(action, action.renderIcon.gameObject);

        return action;
    }

    public GameAction CreateAction (GameAction clon, Personaje character, bool repeatable, ResourceInfo[] recNecesarios = null) {
        return CreateAction(clon.node.GetPosition(), clon.herramienta, clon.tipo, character, repeatable, recNecesarios);
    }
    
    public void RemoveAction(GameAction action) {
        _actions.RemoveAction(action);
    }

    public Sprite SearchIcon(TIPOACCION tipoAccion) {
        for(int i = 0; i < manager.iconos.Length; i++) {
            if(manager.iconos[i].type == tipoAccion)
                return manager.iconos[i].sprite;
        }

        //Si no encuentra el icono devuelve el primero.
        return manager.iconos[0].sprite;
    }
}

[System.Serializable]
public class IconInfo {
    public TIPOACCION type;
    public Sprite sprite;
}