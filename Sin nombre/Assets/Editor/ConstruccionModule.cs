using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ConstruccionModule :IModulo {

    int id;

    Construccion controller;
    MainEditor mainEditor;

    public ConstruccionModule(MainEditor mainEditor) {
        this.mainEditor = mainEditor;

        controller = (Construccion) GameObject.FindObjectOfType(typeof(Construccion));
    }

    public void OnList() {
        id = mainEditor.ListaBotones<ObjetoTienda>(ref controller.construcciones, id);
    }

    public void OnBody() {
        if(id >= controller.construcciones.Length) {
            EditorGUILayout.HelpBox("El id es superior al valor real...", MessageType.Warning);
            return;
        }
        ObjetoTienda panel = controller.construcciones[id];
        
        panel.nombre = EditorGUILayout.TextField("Nombre recurso: ", panel.nombre);
        panel.descripcion = EditorGUILayout.TextArea(panel.descripcion);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Actualmente desactivado la inclusión de arrays de Sprites", MessageType.Warning);

        GUILayout.Space(10);

        panel.categoria = (CONSTRUCCION) EditorGUILayout.EnumPopup("Tipo recurso: ", panel.categoria);
        panel.tipoConstruccion = (TIPOCONSTRUCCION) EditorGUILayout.EnumPopup("Tipo recurso: ", panel.tipoConstruccion);
        panel.investigacionNec = (INVESTIGACION) EditorGUILayout.EnumPopup("Investigación: ", panel.investigacionNec);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("El prefab con la construcción necesaria.", MessageType.Info);
        panel.prefab = (GameObject) EditorGUILayout.ObjectField("Prefab: ", panel.prefab, typeof(GameObject), false);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Actualmente desactivado la inclusión de arrays de ObjetoRecursos", MessageType.Warning);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("La posición de los vectores", MessageType.Info);
        panel.entrada = EditorGUILayout.Vector2Field("Posición entrada rel:", panel.entrada);
        EditorGUILayout.HelpBox("Actualmente desactivado la inclusión de arrays de Vector2", MessageType.Warning);

    }

    public string GetName(int i) {
        return controller.construcciones[i].nombre;
    }
}