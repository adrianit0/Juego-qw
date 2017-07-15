using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MainEditor : EditorWindow {

    Vector2 scrollPositionBarra;
    Vector2 scrollPositionContent;
    float buttonListWidth = 180;
    float buttonWidth = 20;


    GameManager manager;

    ResourceModule resource;
    ConstruccionModule build;

    IModulo modulo;

    [MenuItem("Juego/Editor", false, 140)]
    static void Init() {
        MainEditor window = (MainEditor) EditorWindow.GetWindow(typeof(MainEditor));

        window.position = new Rect(0, 20, 800, 400);
        window.Crear();

        window.Show();
    }

    void Crear() {
        minSize = new Vector2(800, 400);
        manager = (GameManager) GameObject.FindObjectOfType(typeof(GameManager));

        resource = new ResourceModule(this);
        build = new ConstruccionModule(this);
    }

    bool PedirManager() {
        if(manager != null) {
            return true;
        } else {
            manager = (GameManager) GameObject.FindObjectOfType(typeof(GameManager));
            if(manager == null) {
                GUILayout.Label("Hace falta un script \"GameManager\" para hacer funcionar este plugin y así poder editar las cartas.", EditorStyles.boldLabel/*, EditorStyles.largeLabel*/);
                GUILayout.Label("No se ha encontrdo el Script, insertalo aquí:", EditorStyles.boldLabel);
                manager = (GameManager) EditorGUILayout.ObjectField("GameManager: ", manager, typeof(GameManager), true, GUILayout.MinWidth(50), GUILayout.MaxWidth(300));
                GUILayout.Label("Si no tienes ninguno puedes crear uno aqui:", EditorStyles.boldLabel);
                if(GUILayout.Button("Crear nuevo", GUILayout.MinWidth(20), GUILayout.MaxWidth(100))) {
                    GameObject _obj = new GameObject();
                    _obj.name = "GameManager";
                    _obj.AddComponent<GameManager>();

                    return true;
                }
            }

            return false;
        }
    }

    void OnGUI() {
        if (!PedirManager ()) {
            return;
        }

        GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true), GUILayout.Height(50));
        //Zona de módulos: Incluirlos todos aquí.
        if (GUILayout.Button ("Inicio", GUILayout.Height (45), GUILayout.Width (100))) {
            modulo = null;
        }
        if(GUILayout.Button("Recursos", GUILayout.Height(45), GUILayout.Width(100))) {
            modulo = (IModulo) resource;
        }
        if(GUILayout.Button("Construcción", GUILayout.Height(45), GUILayout.Width(100))) {
            modulo = (IModulo) build;
        }
        //Fin de zona de módulos
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginVertical("box", GUILayout.Width(210), GUILayout.ExpandHeight(true));
        //Zona de la lista.
        if (modulo != null) {
            modulo.OnList();
        } else {
            GUILayout.Label("Sin contenido.", EditorStyles.boldLabel);
        }
        //Fin zona de la lista.
        GUILayout.EndVertical();
        GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        //La otra zona.
        if(modulo != null) {
            scrollPositionContent = GUILayout.BeginScrollView(scrollPositionContent, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            modulo.OnBody();
            GUILayout.EndScrollView();
        }
        //Fin zona de la lista.
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public int ListaBotones <T> (ref T[] array, int selected) where T : new() {
        scrollPositionBarra = GUILayout.BeginScrollView(scrollPositionBarra, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        for(int i = 0; i < array.Length; i++) {
            string nombre = modulo.GetName(i);
            nombre = (nombre == "") ? "Array #" + i : nombre;

            EditorGUILayout.BeginHorizontal();
            if(selected == i) {
                GUILayout.Label(nombre, GUILayout.Width(buttonListWidth - (buttonWidth * 3)));
            } else {
                if(GUILayout.Button(nombre, GUILayout.Width(buttonListWidth - (buttonWidth * 3)))) {
                    GUIUtility.keyboardControl = 0;
                    selected = i;
                }
            }

            if(array.Length > 1) {
                if(GUILayout.Button("-", GUILayout.Width(buttonWidth))) {
                    if(EditorUtility.DisplayDialog("Confirmar", "¿Deseas eliminar el elemento " + nombre + "?", "Sí", "No")) {
                        GUIUtility.keyboardControl = 0;
                        array = BorrarValor<T>(array, i);
                        selected = Mathf.Clamp(selected - 1, 0, array.Length - 1);
                    }
                }
            }

            if(i == array.Length - 1) {
                GUILayout.Space(buttonWidth + 4);
            } else {
                if(GUILayout.Button("↓", GUILayout.Width(buttonWidth))) {
                    array = CambiarValor<T>(array, i, i + 1);
                }
            }
            if(i == 0) {
                GUILayout.Space(buttonWidth);
            } else {
                if(GUILayout.Button("↑", GUILayout.Width(buttonWidth))) {
                    array = CambiarValor<T>(array, i, i - 1);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(5);
        if(GUILayout.Button("Crear nuevo elemento", GUILayout.Width(200)) || array.Length == 0) {
            GUIUtility.keyboardControl = 0;
            
            array = NuevoValor<T>(array);
            selected = array.Length - 1;
        }
        GUILayout.EndScrollView();

        return selected;
    }

    T[] NuevoValor<T>(T[] array) where T : new() {
        System.Array.Resize<T>(ref array, array.Length + 1);
        array[array.Length-1] = new T();
        return array;
    }

    T[] BorrarValor<T>(T[] array, int value) {
        if(value < 0 || value >= array.Length)
            return array;
        
        T[] nuevoArray = new T[array.Length - 1];

        int x = 0;
        for(int i = 0; i < nuevoArray.Length; i++) {
            if(x == value)
                x++;
            nuevoArray[i] = array[x];
            x++;
        }

        return nuevoArray;
    }

    T[] CambiarValor<T>(T[] array, int value1, int value2) {
        if(value1 < 0 || value1 >= array.Length || value2 < 0 || value2 >= array.Length)
            return array;

        T value = array[value1];
        array[value1] = array[value2];
        array[value2] = value;
        return array;
    }
}