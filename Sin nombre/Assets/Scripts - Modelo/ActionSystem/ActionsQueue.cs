using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsQueue {

    public Dictionary<GameAction, SpriteRenderer> actions { get; private set; }

    GameManager manager;

    float lastTime = 0, timeMin = 0.1f;

    public ActionsQueue(GameManager manager) {
        this.manager = manager;
        
        actions = new Dictionary<GameAction, SpriteRenderer>();
    }

    public void AddAction (GameAction action, SpriteRenderer icon) {
        if(action == null)
            return;
        
        actions.Add(action, icon);

        SearchCharacter();

        //Debug.Log("Actualmente hay una lista con "+actions.Count+" acciones");
    }

    /// <summary>
    /// Elimina una accion.
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAction (GameAction action) {
        if(actions.ContainsKey (action)) {
            GameObject.Destroy(actions[action].gameObject);

            actions.Remove(action);
        }

        if(action.worker != null) {
            action.UnassingCharacter();
        }
    }

    /// <summary>
    /// Utilizado cuando se devuelve una acción DESACTIVADA a la cola a la espera de que se active.
    /// </summary>
    public void ReturnAction(GameAction action) {
        if(actions.ContainsKey(action)) {
            //TODO: Arreglar esto
            action.worker.RemoveAction(action, false);
            action.UnassingCharacter();
            action.Desactivar(5f);
        }
    }

    /// <summary>
    /// Al crear una accion busca si hay algún personaje sin nada que hacer, solo se puede hacer 1 vez cada 0.1 segundos.
    /// </summary>
    void SearchCharacter () {
        if(lastTime - Time.timeSinceLevelLoad > timeMin)
            return;

        lastTime = Time.timeSinceLevelLoad;

        for (int i = 0; i < manager.characters.Count; i++) {
            if (!manager.characters[i].IsWorking ()) {
                AssignActionCharacter(manager.characters[i]);
            }
        }
    }

    /// <summary>
    /// Cambia los iconos de las acciones al de prioridad.
    /// </summary>
    public void ChangeIconToPriority () {
        foreach(KeyValuePair<GameAction, SpriteRenderer> action in actions) {
            action.Value.sprite = manager.iconosPrioridad[action.Key.prioridad];
        }
    }

    /// <summary>
    /// Cambia un único icono de las acciones al de prioridad.
    /// </summary>
    public void ChangeSingleIconToPriority (GameAction action) {
        if (!actions.ContainsKey (action)) {
            Debug.LogWarning("ActionsQueue::ChangeSingleIconToPriority error: Acción no encontrado.");
            return;
        }

        actions[action].sprite = manager.iconosPrioridad[action.prioridad];
    }

    /// <summary>
    /// Cambia los iconos de las acciones al original.
    /// </summary>
    public void ChangeIconToOriginal () {
        foreach (KeyValuePair<GameAction, SpriteRenderer> action in actions) {
            action.Key.ChangeSpriteToOriginal(action.Value);
        }
    }

    /// <summary>
    /// Asigna una acción al personaje de manera automatica de todas las que haya. De momento el criterio a seguir será el siguiente:
    /// - Prioridad más alta.
    /// - Distancia más cercana al personaje.
    /// TODO: 
    /// - Despues de prioridad, que busque la acción que más se adapte a sus habilidades.
    /// </summary>
    /// <param name="character"></param>
    public void AssignActionCharacter(Personaje character) {
        if(character.IsWorking()) {
            Debug.Log("AssignActionCharacter error: Actualmente el personaje tiene alguna acción que reallizar");
            return;
        }

        //Ahora buscará la mejor acción para el personaje.
        GameAction ChosenAction = null;
        int priority = 0;
        float distance = 10000;
        int i = 0;
        foreach (GameAction action in actions.Keys) {
            if (action.worker != null || action.desactivado) {
                continue;
            }

            float thisDistance = Vector3.Distance(character.transform.position, (Vector3) action.node.GetPosition());
            if (action.prioridad>priority) {
                priority = action.prioridad;
                ChosenAction = action;
                distance = thisDistance;
            } else if(thisDistance < distance) {
                ChosenAction = action;
                distance = thisDistance;
            }

            i++;
        }

        //No ha encontrado una acción
        if (ChosenAction == null) {
            Debug.Log("AssignActionCharacter error: No ha encontrado una buena acción para el personaje.");
            return;
        }

        //Devuelve el camino más cercano entre el personaje y el nodo donde se encuentra la acción.
        //De no encontrar el lugar devolverá [0, 0] y desactivará la acción durante 5s.
        PathResult resultado = manager.path.PathFind(character, new PathSetting(ChosenAction.node.GetPosition()));
        if(resultado.GetFinalPosition() == new IntVector2(0,0)) {
            ChosenAction.Desactivar(5f);
            return;
        }

        //Por último se añade la acción al personaje para que realice la acción y le traza la ruta más cercana.
        ChosenAction.AssignCharacter(character);
        character.SetPositions(resultado.path);
    }
    
    public bool IsActionCreated (IntVector2 pos) {
        foreach(GameAction action in actions.Keys) {
            if(action != null && action.node.GetPosition() == pos) {
                return true;
            }
        }

        return false;
    }
}
