using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ResourceModule : IModulo {

    int id;

    ResourceController controller;
    MainEditor mainEditor;

    public ResourceModule (MainEditor mainEditor) {
        this.mainEditor = mainEditor;

        controller = (ResourceController) GameObject.FindObjectOfType(typeof(ResourceController));
    }

    public void OnList() {
        id = mainEditor.ListaBotones<ResourcePanel>(ref controller.panelRecurso, id);
    }

    public void OnBody() {
        if (id >= controller.panelRecurso.Length) {
            EditorGUILayout.HelpBox("El id es superior al valor real...", MessageType.Warning);
            return;
        }
        ResourcePanel panel = controller.panelRecurso[id];

        panel.name = EditorGUILayout.TextField("Nombre recurso: ", panel.name);

        GUILayout.Space(10);

        panel.resource = (RECURSOS) EditorGUILayout.EnumPopup("Tipo recurso: ", panel.resource);
        panel.tipo = (TIPORECURSO) EditorGUILayout.EnumPopup("Tipo recurso: ", panel.tipo);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Un sprite para la imagen", MessageType.Info);
        panel.image = (Sprite) EditorGUILayout.ObjectField("Sprite: ", panel.image, typeof(Sprite), false);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("El componente text para poder mostrarse en pantalla", MessageType.Info);
        panel.text = (Text) EditorGUILayout.ObjectField("Texto: ", panel.text, typeof(Text), true);
    }

    public string GetName(int i) {
        return controller.panelRecurso[i].name;
    }
}