using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aqui iran todas las acciones creadas 
/// </summary>
public class ActionMethods  {

    GameManager manager;
    ActionManager actions;

    public ActionMethods (GameManager manager, ActionManager actions) {
        this.manager = manager;
        this.actions = actions;
    }

    public bool ComprobarAcceso (GameAction action) {
        if(action == null) {
            Debug.LogWarning("ExtraerRecursos error: No existe acción.");
            return true;
        }

        if(action.worker == null) {
            Debug.LogWarning("ExtraerRecursos error: No tiene asignado ningún trabajador. ActionType: " + action.tipo + " ActionHerramienta: " + action.herramienta);
            return true;
        }

        return false;
    }

    //METODOS

    /// <summary>
    /// Extraer recursos
    /// </summary>
    public void ExtraerRecursos(Recurso recursos, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        action.worker.inventario.AddResource(recursos);

        if(!action.worker.inventario.IsFull() && recursos.actualQuantity > 0) {
            action.worker.AddAction(actions.CreateAction(action, action.worker, true));
        } else if(manager.ExistBuild(ESTRUCTURA.Almacen)) {
            action.worker.BuscarAlmacenCercano();

            if(recursos.actualQuantity > 0) {
                action.worker.AddAction(actions.CreateAction(action, action.worker, true));
            }
        }
    }

    /// <summary>
    /// Pesca un pez
    /// </summary>
    public void Pescar (Estructura build, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        Agua agua = build.GetComponent<Agua>();
        if(agua != null) {
            bool obtenido = agua.Pescar();
            if(obtenido) {
                action.worker.inventario.AddResource(RECURSOS.Pescado, 1);
                action.worker.BuscarAlmacenCercano();
            }

            action.worker.AddAction(actions.CreateAction(action, action.worker, true));
        }
    }

    /// <summary>
    /// Mete todo el contenido que tiene el personaje en el almacen.
    /// </summary>
    public void AlmacenarRecursos(Almacen almacen, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        action.worker.inventario.CleanResource(almacen.inventario);
        if(action.worker.inventario.Count > 0) {
            action.worker.BuscarAlmacenCercano();
        }
    }

    /// <summary>
    /// Extrae contenido del almacen y se lo da al personaje.
    /// </summary>
    public void SacarContenidoAlmacen (ResourceInfo[] objetos, Almacen almacen, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        almacen.inventario.GetResources(objetos, action.worker.inventario);
    }

    public void VaciarAlmacen (Almacen almacen, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        almacen.OnDestroyBuild();
    }

    /// <summary>
    /// Ara el suelo.
    /// </summary>
    public void ArarTierra (GameObject ararPrefab, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        manager.CreateBuild(action.node.GetPosition(), ararPrefab);
    }

    public void Plantar (Estructura build, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        Huerto huerto = build.GetComponent<Huerto>();

        if(huerto != null) {
            huerto.Cultivar(action.recursosNecesarios[0].type);
        }
    }

    /// <summary>
    /// Comprueba si el personaje tiene la suficiente agua para regar las plantas
    /// Si no tiene, va a por agua.
    /// </summary>
    public void ComprobarAgua (GameAction action) {
        if(ComprobarAcceso(action))
            return;
        
        if (action.worker.aguaTotal.litrosTotales == 0) {
            IntVector2 pos = manager.path.PathFind(action.worker, new PathSetting(TIPOAGUA.AguaDulce, 0.5f)).GetFinalPosition(); ;
            if(pos != IntVector2.Zero) {
                action.worker.AddAction (actions.CreateAction(pos, HERRAMIENTA.Custom, TIPOACCION.ExtraerAgua, null, false, null), 0);
            } else {
                //Si no encuentra el agua cancela la acción.
                Debug.LogWarning("ActionMethods::ComprobarAgua error: No ha encontrado agua en las proximidades del personaje. La acción se detendrá.");
                actions._actions.ReturnAction(action);
            }
        }
    }

    public void Regar (Estructura build, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        Huerto huerto = build.GetComponent<Huerto>();
        if(huerto != null) {
            huerto.Regar(action.worker.aguaTotal);
        }
    }

    public void ExtraerAgua (Estructura build, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        Agua agua = build.GetComponent<Agua>();

        action.worker.aguaTotal.TomarAgua (action.worker.aguaTotal.litrosMaximo, agua.agua);
    }

    /// <summary>
    /// Comprueba si puede construir.
    /// Si no puede tendrá que buscar los recursos necesarios para construir.
    /// Si no tiene los recursos necesarios desactivará la acción.
    /// </summary>
    /// <param name="action"></param>
    public void ComprobarInventarioAction (GameAction action) {
        if(ComprobarAcceso(action))
            return;

        Personaje worker = action.worker;

        //Pones la cantidad de objetos que tienes encima en el inventario
        //Si aún te falta va a buscar los objetos necesarios.
        //Recuperas los recursos sobrantes.
        ResourceInfo[] info = worker.inventario.GetResources(action.recursosNecesarios);
        if(info != null || info.Length != 0) {
            for(int i = 0; i < info.Length; i++) {
                int sobrante = action.AddResource(info[i].type, info[i].quantity);

                if(sobrante > 0) {
                    worker.inventario.AddResource(info[i].type, sobrante);
                }
            }
        }
        

        if (!action.CanBuild ()) {
            IntVector2 _pos = manager.path.PathFind(worker, new PathSetting(action.recursosNecesarios)).GetFinalPosition();
            
            if(_pos != IntVector2.Zero) {
                worker.AddAction(manager.actions.CreateAction(_pos, HERRAMIENTA.Custom, TIPOACCION.Almacenar, worker, true, null), 0);
                worker.AddAction(manager.actions.CreateAction(_pos, HERRAMIENTA.Custom, TIPOACCION.SacarAlmacen, worker, true, action.recursosNecesarios), 1);
            } else {
                Debug.LogWarning("ActionMethods::ComprobarConstruccion error: No ha encontrado los recursos necesarios en los baules actuales. La acción se detendrá.");
                actions._actions.ReturnAction(action);
            }
        }
    }

    /// <summary>
    /// Construye una edificación a partir de su ID en la lista en Construcciones.
    /// </summary>
    public void Construir (int buildID, GameAction action) {
        if(ComprobarAcceso(action))
            return;

        IntVector2 pos = action.node.GetPosition();
        Estructura _build = manager.CreateBuild(pos, manager.build.construcciones[buildID].prefab);
        for(int i = 0; i < manager.build.construcciones[buildID].posicionesExtras.Length; i++) {
            manager.AddBuildInMap(pos + manager.build.construcciones[buildID].posicionesExtras[i], _build);
        }

        if (manager.build.construcciones[buildID].spriteObjeto.Length>1) {
            _build.ChangeSprite(manager.build.CompareNeighbour(pos, true));
        }
    }

    /// <summary>
    /// Cancelar la construcción, devolviendo todo los recursos usados para la creación del mismo.
    /// </summary>
    /// <param name="action"></param>
    public void DevolverRecursos (GameAction action) {
        if(ComprobarAcceso(action))
            return;

        manager.CrearSaco(action.node.GetPosition(), 10, action.recursosActuales);
    }

    /// <summary>
    /// Craftea el objeto que estuviera en cola.
    /// </summary>
    public void Craftear (Estructura build, GameAction action) {
        if(ComprobarAcceso(action) || build == null)
            return;

        Crafteable craftTable = build.GetComponent<Crafteable>();

        if (craftTable == null) {
            Debug.LogWarning("Craftear::ActionMethods error: Eso no es mesa de crafteo...");
            return;
        }

        Craft crafteo = craftTable.GetThisCraft();
        if (crafteo==null) {
            return;
        }

        foreach (ResourceInfo info in crafteo.requisitos) {
            action.worker.inventario.RemoveResource(info.type, info.quantity);
        }
        action.worker.inventario.AddResource(crafteo.obtencion.type, crafteo.obtencion.quantity);

        craftTable.FinishCraft();

        if (craftTable.HasMoreCrafts ()) {
            actions.CreateAction(action, action.worker, true, craftTable.GetThisCraft ().requisitos);
        }
    }


    public void Destruir (Estructura estructura, GameAction action) {
        if (ComprobarAcceso (action) || estructura == null) {
            return;
        }
        estructura.AlDestuirse();

        GameObject.Destroy(estructura.gameObject);

        //Si hubiera alguien realizando alguna acción lo cancelaría automaticamente al destruirse.
        manager.actions.CreateAction(action.node.GetPosition (), HERRAMIENTA.Cancelar, TIPOACCION.Talar);
    }
}
