using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManagement {

    public List<ResourceBool> lista { get; private set; }

    public ResourceManagement(GameManager manager) {
        lista = new List<ResourceBool>();

        foreach (ResourcePanel rec in manager.resourceController.panelRecurso) {
            lista.Add(new ResourceBool(rec.resource, true));
        }
    }

    public void AddResourceList (ResourceBool recurso) {
        lista.Add(recurso);
    }

    public void ChangeValue (RECURSOS recurso, bool newValue) {
        for (int i =0; i < lista.Count; i++) {
            if (lista[i].recurso == recurso) {
                lista[i].value = newValue;
                return;
            }
        }
    }

    public bool GetBool (RECURSOS recurso) {
        for(int i = 0; i < lista.Count; i++) {
            if(lista[i].recurso == recurso) {
                return lista[i].value;
            }
        }

        return true;
    }
}


public class ResourceBool {
    public RECURSOS recurso;
    
    public bool value;

    public ResourceBool (RECURSOS recurso, bool value) {
        this.recurso = recurso;
        this.value = value;
    }
}