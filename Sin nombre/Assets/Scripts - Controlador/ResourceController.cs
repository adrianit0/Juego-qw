using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour {

    public ResourcePanel[] panelRecurso;

    public void ModifyResource (ResourceInfo resourceInfo) {
        for(int i = 0; i < panelRecurso.Length; i++) {
            if(panelRecurso[i].resource == resourceInfo.type) {
                panelRecurso[i].quantity += resourceInfo.quantity;
                UpdatePanel(i);
            }
        }
    }

    public Sprite GetSprite (RECURSOS resourceType) {
        for(int i = 0; i < panelRecurso.Length; i++) {
            if(panelRecurso[i].resource == resourceType) {
                return panelRecurso[i].image;
            }
        }

        return null;
    }

    void UpdatePanel (int index) {
        panelRecurso[index].text.text = panelRecurso[index].quantity.ToString();
    }
}

[System.Serializable]
public class ResourcePanel {
    public string name;

    public RECURSOS resource;
    public Sprite image;
    public Text text;
    public int quantity;
}