using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharAttribute {
    public ATRIBUTO atributo { get; private set; }
    public int experiencia { get; private set; }
    public int level { get; private set; }

    public bool hasExperience { get; private set; }

    public CharAttribute(ATRIBUTO atributo, int nivel,  int experiencia) {
        this.atributo = atributo;
        this.experiencia = experiencia;
        level = nivel;
        hasExperience = true;
    }

    public CharAttribute(ATRIBUTO atributo) {
        this.atributo = atributo;
        experiencia = 0;
        level = 5;
        hasExperience = false;
    }

    public void AddExperiencie(int amount) {
        if(!hasExperience) {
            Debug.LogWarning("No puede tener experiencia");
            return;
        }
        experiencia += amount;
    }

    public void AddLevel(int nextLevelExperiencie) {
        if(level >= 5) {
            Debug.Log("Tienes el máximo nivel");
            return;
        }

        level++;
        experiencia -= nextLevelExperiencie;

        if(experiencia < 0) {
            Debug.Log("Tienes experiencia negativa");
        }
    }

    public void ChangeLevel(int newLevel) {
        if(hasExperience) {
            Debug.LogWarning("No puede cambiar nivel");
            return;
        }

        level = newLevel;
    }

    public void RecalculateExperience(float porc) {
        if(!hasExperience) {
            return;
        }

        experiencia = Mathf.RoundToInt(((float) experiencia) * porc);
    }
}
