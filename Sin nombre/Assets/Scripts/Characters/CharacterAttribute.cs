using System;
using System.Collections.Generic;
using UnityEngine;

public enum ATRIBUTO { Salud, Estres, Hambre, Constitucion, Atletismo, Mineria, Recoleccion, Construccion, Ingenio, Carisma, Culinario }

public class CharacterAttribute {

    public int this [ATRIBUTO atributo] {
        get {
            return GetExperience (atributo);
        }
    }

    Dictionary<ATRIBUTO, CharAttribute> attributes;
    int level = 0;
    int firstLevel = 24;
    Action actionUpdate;

    int[] levelRequeriment = new int[] {
        -472, -471, -469, -466, -462, -457, -451, -444, -436, -427, -417, -405, -391,
        -375, -357, -337, -314, -288, -259, -227, -191, -151, -106, -56, 0, 62, 131,
        207, 291, 384, 487, 601, 727, 866, 1019, 1188, 1374, 1579, 1805, 2054, 2328,
        2630, 2963, 3330, 3734, 4179, 4669, 5209, 5804, 6459, 7180, 7974, 8848, 9810,
        10869, 12034, 13316, 14727, 16280, 17989, 19869, 21938, 24214, 26718, 29473,
    };

    public CharacterAttribute (GameManager manager) {
        attributes = new Dictionary<ATRIBUTO, CharAttribute>();

        foreach(ATRIBUTO tipo in System.Enum.GetValues(typeof(ATRIBUTO))) {
            CharAttribute script = null;

            if (tipo == ATRIBUTO.Salud || tipo == ATRIBUTO.Estres || tipo == ATRIBUTO.Hambre) {
                script = new CharAttribute(tipo);
            } else {
                script = new CharAttribute(tipo, UnityEngine.Random.Range (-2, 6), 0);
            }

            attributes.Add(tipo, script);
        }

        level = 0;

        actionUpdate += () => manager.characterController.Actualizar();
    }

    public int GetLevel () {
        int quantity = 0;
        foreach (KeyValuePair<ATRIBUTO, CharAttribute> value in attributes) {
            if (value.Value.hasExperience)
                quantity += value.Value.level;
        }

        level = quantity;
        return quantity;
    }

    public int GetLevel(ATRIBUTO atributo) {
        return attributes[atributo].level;
    }

    public int GetExperience(ATRIBUTO atributo) {
        return attributes[atributo].experiencia;
    }

    public void AddExperiencia (ATRIBUTO atributo, int experiencia) {
        CharAttribute atribute = attributes[atributo];
        int thisExperience = GetFixedExperience(level);
        atribute.AddExperiencie(experiencia);

        Debug.Log("Has ganado " + experiencia + " de experiencia en el atributo " + atributo.ToString() + ". Total experiencia: " + atribute.experiencia + "/" + thisExperience);

        if (atribute.experiencia > thisExperience) {
            int newExperience = GetFixedExperience(level + 1);

            float porc = GetAumento(level, thisExperience, newExperience);
            atribute.AddLevel(newExperience);

            Debug.Log("Ha subido el atributo " + atributo + "Al nivel " + atribute.level);

            foreach(KeyValuePair<ATRIBUTO, CharAttribute> value in attributes) {
                if(value.Value.hasExperience)
                    value.Value.RecalculateExperience (porc);
            }

            level++;
        }

        //Actualizar el gráfico.
        actionUpdate();
    }

    public float GetAumento (int nivel, int thisExperience, int newExperience) {
        return ((float) newExperience) / ((float) thisExperience);
    }

    public float GetPorc(ATRIBUTO atributo) {
        int nivel = GetLevel(atributo);
        int actualExperience = GetExperience(atributo);

        int nextLevel = GetFixedExperience(nivel + 1);

        if(nextLevel == 0) {
            return 1;
        }

        return (((float) actualExperience) / ((float) nextLevel));
    }

    public int GetFixedExperience (int level) {
        level += firstLevel;

        if(level + 1 >= levelRequeriment.Length) {
            return 0;
        } else if (level < 0) {
            level = 0;
        }

        return Mathf.Abs(levelRequeriment[level] - levelRequeriment[level + 1]);
    }
}