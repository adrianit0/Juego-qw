using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafteable : Estructura, IEstructura {
    
    public List<Craft> crafteos { get; private set; }

    /// <summary>
    /// Si el crafteador es una mesa de trabajo, horno o cocina, por ejemplo.
    /// </summary>
    public CRAFTTYPE tipoCrafteador;
    public Sprite spriteCraftear;

    public bool repetir { get; private set; }
    
    public void OnStart () {
        crafteos = new List<Craft>();
    }

    public Craft GetThisCraft () {
        if (crafteos == null || crafteos.Count == 0) {
            Debug.LogWarning("Crafteable::GetThisCraft error: No hay nada que craftear.");
            return null;
        }
        return crafteos[0];
    }

    public bool HasMoreCrafts () {
        return (crafteos != null && crafteos.Count > 0);
    }

    public void AddCraft (Craft recipe) {
        if (crafteos.Count>=5) {
            Debug.LogWarning("Crafteable::AddCraft error: Has sobrepasado el límite impuesto actualmente de 5 crafteos máximo.");
            return;
        }

        crafteos.Add(recipe);
        if (manager.craft.panel.activeSelf) {
            manager.craft.SetCraftableTable(this);
        }

        if (crafteos.Count == 1) {
            tiempoTotal = crafteos[0].tiempo;

            manager.actions.CreateAction((IntVector2) transform.position, HERRAMIENTA.Custom, TIPOACCION.Craftear, null, false, -1, crafteos[0].requisitos);
        }
    }

    /// <summary>
    /// Termina el trabajo actual.
    /// </summary>
    public void FinishCraft () {
        if (crafteos == null || crafteos.Count==0) {
            Debug.LogWarning("Crafteable::FinishCraft: No se puede terminar este crafteo porque no existe.");
            return;
        }

        Craft _craft = crafteos[0];

        crafteos.RemoveAt(0);
        if (repetir) {
            AddCraft(_craft);
        } else {
            if(manager.craft.panel.activeSelf) {
                manager.craft.SetCraftableTable(this);
            }
        }

        if (crafteos.Count>0) {
            tiempoTotal = crafteos[0].tiempo;
        }
    }

    public void CancelCraft (int id) {
        if (id >= crafteos.Count) {
            //No pasa nada porque no hay nada que cancelar.
            Debug.LogWarning("Crafteable::CancelCraft: No se puede cancelar este crafteo porque no existe.");
            return;
        }

        crafteos.RemoveAt(id);
        if(manager.craft.panel.activeSelf) {
            manager.craft.SetCraftableTable(this);
        }

        //TODO:
        //Si quitas la acción que se está realizando. 
        //Eliminas la acción y la vuelves a activar si tiene más crafteos en la cola (Para que el personaje lo haga desde 0.
        if (id==0) {
            manager.actions.CreateAction((IntVector2) transform.position, HERRAMIENTA.Cancelar, TIPOACCION.Cocinar);

            if (crafteos.Count>0) {
                tiempoTotal = crafteos[0].tiempo;

                manager.actions.CreateAction((IntVector2) transform.position, HERRAMIENTA.Custom, TIPOACCION.Craftear, null, false, -1, crafteos[0].requisitos);
            }
        }
    }

    public void SetRepeatable (bool value) {
        if(repetir == value) {
            return;
        }

        repetir = value;
    }

    public string OnText () {
        AbrirPanel();
        manager.info.ActivarBoton(0, spriteCraftear, "Fabricar", true, () => {
            AbrirPanel();
        });

        return "Seleccionado una mesa de trabajo.";
    }

    void AbrirPanel () {
        manager.craft.OpenPanel();
        manager.craft.SetCraftableTable(this);
    }

    public string OnTextGroup (Estructura[] builds) {
        manager.info.ActivarBoton(0, spriteCraftear, "Fabricar", false, () => {
            //No pasa nada ya que los crafteos tienen que ser seleccionando solo 1 mesa.
        });

        return "Seleccionado varias mesas de trabajos.\n"+
            "Por favor, solo seleccione una mesa al mismo tiempo para crear.";
    }

    public void OnDestroyBuild () {

    }
}
