using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actualmente desactivado
/// </summary>
public class GameController : MonoBehaviour {

    //CLASES SERIALIZADAS
    //Clases sin monoBehaviour.
    public GameManager manager { get; private set; }
    public PathFinding path;
    public ActionManager actions;

    //Clases con monoBehaviour.
    public Agricultura farm;
    public Construccion build;  //Necesario en el personaje.
    public Informacion info;    //Necesario en las estructuras.


    //INSTANCIA ESTÁTICA
    public static GameController Instance { get; private set; }

    //MÉTODOS
    void Awake() {
        if (Instance != null) {
            Debug.LogError("No pueden existir 2 instancias, se procede a eliminar esta");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    //inicializa el juego.
    void Start() {
        path = new PathFinding(manager);
        actions = new ActionManager(manager);


    }

}
