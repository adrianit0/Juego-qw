﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum CONSTRUCCION { Estructuras = 0, Agricultura = 1, Investigacion = 2, Electricidad = 3, Agua = 4 }
public enum TIPOCONSTRUCCION { Estructura, Suelo, Cableado, Tuberia }
public enum INVESTIGACION { Ninguno = 0 }

//TODO: Volver a hacer funcional lo de los iconos de los recursos en la construccion

public class Construccion : MonoBehaviour {

    public ObjetoTienda[] construcciones = new ObjetoTienda[1];
    public GameObject[] panelesConstruccion = new GameObject[5];

    //Panel
    public GameObject panelConstruir;

    //Construyendo
    public bool construyendo = false;
    public int selectID = 0;
    public SpriteRenderer interfazConstructor;
    Vector3 lastPositionBuild;

    //Configurar estructuras
    public GameObject buttonPrefab;
    Button[] createdButtons;

    //Información estructura (Cuando pones el ratón encima del botón).
    public RectTransform cartel;
    public Text textoNombre;
    public Text textoDescripcion;
    public Text textoTiempo;
    public Text[] textosRequisitos = new Text[3];

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    void Start() {
        CreateButtons();

        cartel.gameObject.SetActive(false);
    }

    void CreateButtons() {
        createdButtons = new Button[construcciones.Length];

        for(int i = 0; i < construcciones.Length; i++) {
            if(construcciones[i] == null)
                continue;

            GameObject _obj = Instantiate(buttonPrefab);
            _obj.transform.SetParent(panelesConstruccion[(int) construcciones[i].categoria].transform);
            _obj.transform.localScale = new Vector3(1, 1, 1);

            Button _button = _obj.GetComponent<Button>();
            int x = i;
            _button.onClick.AddListener(() => SelectBuild(x));
            createdButtons[i] = _button;

            _obj.transform.GetChild(0).GetComponent<Image>().sprite = ShowSprite(i, construcciones[i].showSprite);

            //CONFIGURAR EL EVENT TRIGGER

            EventTrigger trigger = _obj.GetComponent<EventTrigger>();
            //Entrar
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { ShowInformation(x); });
            trigger.triggers.Add(entry);
            //Salir
            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((eventData) => { HideInformation(); });
            trigger.triggers.Add(exit);
        }

        ShopUpdate();
    }

    /// <summary>
    /// Cuando se añada o se elimine algún recurso se actualizará para saber que construcciones puedes producir.
    /// </summary>
    public void ShopUpdate() {
        for (int i = 0; i < createdButtons.Length; i++) {
            bool activar = true;
            
            for (int x = 0; x < construcciones[i].recursosNecesarios.Length; x++) {
                if (construcciones[i].recursosNecesarios[x].cantidadNecesaria > manager.inventario.GetResourceCount (construcciones[i].recursosNecesarios[x].recurso)) {
                    activar = false;
                    break;
                }
            }

            createdButtons[i].interactable = activar;
        }
    }

    /// <summary>
    /// Se actualizará una vez cada frame.
    /// </summary>
    public void BuildUpdate() {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (pos != lastPositionBuild) {
            lastPositionBuild = pos;

            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);
            interfazConstructor.transform.position = new Vector3(x, y);
            interfazConstructor.color = new Color(1, 1, 1, 0.75f);


            switch (construcciones[selectID].tipoConstruccion) {
                case TIPOCONSTRUCCION.Estructura:
                    for(int i = 0; i < construcciones[selectID].posicionesExtras.Length; i++) {
                        int x2 = Mathf.RoundToInt(construcciones[selectID].posicionesExtras[i].x), y2 = Mathf.RoundToInt(construcciones[selectID].posicionesExtras[i].y);

                        Node nodo = manager.GetNode(x + x2, y + y2);
                        if(x + x2 < 0 || y + y2 < 0 || x + x2 >= manager.totalSize.x || y + y2 >= manager.totalSize.y || nodo.GetBuildType() != ESTRUCTURA.Ninguno || nodo.movementCost == 0) {
                            interfazConstructor.color = new Color(1, 0, 0, 0.75f);
                            break;
                        }
                    }
                    break;

                case TIPOCONSTRUCCION.Suelo:
                    if(x < 0 || y < 0 || x >= manager.totalSize.x || y >= manager.totalSize.y || !manager.GetNode(x, y).CanBuildFloor ()) {
                        interfazConstructor.color = new Color(1, 0, 0, 0.75f);
                        break;
                    }
                    break;

                default:
                    Debug.LogWarning("Herramienta no programada.");
                    break;
            }
            
        }

        if(Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) {
            StartBuild();
        }

        if(Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject()) {
            CancelBuild();
        }
    }

    public void SelectBuild (int ID) {
        construyendo = true;
        selectID = ID;

        interfazConstructor.sprite = construcciones[ID].spriteModelo;
        interfazConstructor.gameObject.SetActive(true);

    }

    public void StartBuild () {
        manager.actions.CreateAction(interfazConstructor.transform.position, HERRAMIENTA.Construir, TIPOACCION.Construir);

        if (!construcciones[selectID].seguirConstruyendo)
            CancelBuild();
    }

    public void CancelBuild () {
        construyendo = false;

        interfazConstructor.gameObject.SetActive(false);
    }

    /// <summary>
    /// Muestra la información en un cartel emergente
    /// </summary>
    void ShowInformation(int ID) {
        cartel.gameObject.SetActive(true);

        textoNombre.text = construcciones[ID].nombre;
        textoDescripcion.text = construcciones[ID].descripcion;
        textoTiempo.text = "<b>Tiempo: </b>" + construcciones[ID].tiempo + "s";

        cartel.position = new Vector3 (cartel.position.x, createdButtons[ID].transform.position.y-30, cartel.position.z);

        for (int i = 0; i < textosRequisitos.Length; i++) {
            if (i < construcciones[ID].recursosNecesarios.Length) {
                textosRequisitos[i].gameObject.SetActive(true);
                textosRequisitos[i].text = "x" + construcciones[ID].recursosNecesarios[i].cantidadNecesaria;

                //Pones el sprite necesario para que se pueda mostrar correctamente.
                textosRequisitos[i].GetComponentInChildren<Image>().sprite = manager.resourceController.GetSprite(construcciones[ID].recursosNecesarios[i].recurso);

                textosRequisitos[i].color = (construcciones[ID].recursosNecesarios[i].cantidadNecesaria > manager.inventario.GetResourceCount(construcciones[ID].recursosNecesarios[i].recurso)) ? Color.red : Color.white;
            } else {
                textosRequisitos[i].gameObject.SetActive(false);
            }
        }
    }

    public Sprite CompareNeighbour (IntVector2 position, int selectID, bool compareNeighbours = true) {
        ObjetoTienda objeto = construcciones[selectID];
        ESTRUCTURA thisType = manager.GetNode(position).build.GetBuildType();

        
        //Si no dispone de sprite, devuelve error.
        if(objeto.spriteObjeto == null || objeto.spriteObjeto.Length==0) {
            Debug.LogWarning("Construccion::CompareNeighbour error: "+ objeto.nombre +" no tiene sprites asignados.");
            return null;
        }

        if (objeto.spriteObjeto.Length==1) {
            return objeto.spriteObjeto[0];
        }

        if(thisType == ESTRUCTURA.Ninguno) {
            Debug.LogWarning("Construccion::CompareNeighbour error: No es ningun tipo de estructura.");
            return objeto.spriteObjeto[0];
        }

        string name = objeto.nombre + "_";
        Node node = manager.GetNode(position.x, position.y+1);
        if (node != null && node.build != null && node.build.GetBuildType() == thisType) {
            name += "N";
            if (compareNeighbours) {
                node.build.ChangeSprite(CompareNeighbour(node.GetPosition(), selectID, false));
            }
        }

        node = manager.GetNode(position.x+1, position.y);
        if(node != null && node.build != null && node.build.GetBuildType() == thisType) {
            name += "E";
            if(compareNeighbours) {
                node.build.ChangeSprite(CompareNeighbour(node.GetPosition(), selectID, false));
            }
        }

        node = manager.GetNode(position.x, position.y - 1);
        if(node != null && node.build != null && node.build.GetBuildType() == thisType) {
            name += "S";
            if(compareNeighbours) {
                node.build.ChangeSprite(CompareNeighbour(node.GetPosition(), selectID, false));
            }
        }

        node = manager.GetNode(position.x - 1, position.y);
        if(node != null && node.build != null && node.build.GetBuildType() == thisType) {
            name += "W";
            if(compareNeighbours) {
                node.build.ChangeSprite(CompareNeighbour(node.GetPosition(), selectID, false));
            }
        }

        for (int i = 0;i < objeto.spriteObjeto.Length; i++) {
            if (objeto.spriteObjeto[i].name == name) {
                return objeto.spriteObjeto[i];
            }
        }

        return objeto.spriteObjeto[0];
    }

    public Sprite CompareNeighbourFloor(IntVector2 position, int selectID, bool compareNeighbours = true) {
        ObjetoTienda objeto = construcciones[selectID];
        string thisFloor = objeto.nombre;

        //Si no dispone de sprite, devuelve error.
        if(objeto.spriteObjeto == null || objeto.spriteObjeto.Length == 0) {
            Debug.LogWarning("Construccion::CompareNeighbour error: " + objeto.nombre + " no tiene sprites asignados.");
            return null;
        }

        if(objeto.spriteObjeto.Length == 1) {
            return objeto.spriteObjeto[0];
        }

        if(thisFloor == "") {
            Debug.LogWarning("Construccion::CompareNeighbourFloor error: No tiene ningún suelo.");
            return objeto.spriteObjeto[0];
        }

        string name = thisFloor + "_";
        Node node = manager.GetNode(position.x, position.y + 1);
        if(node != null && node.floorName == thisFloor) {
            name += "N";
            if(compareNeighbours) {
                node.ChangeFloorSprite(CompareNeighbourFloor(node.GetPosition(), selectID, false), true);
            }
        }

        node = manager.GetNode(position.x + 1, position.y);
        if(node != null && node.floorName == thisFloor) {
            name += "E";
            if(compareNeighbours) {
                node.ChangeFloorSprite(CompareNeighbourFloor(node.GetPosition(), selectID, false), true);
            }
        }

        node = manager.GetNode(position.x, position.y - 1);
        if(node != null && node.floorName == thisFloor) {
            name += "S";
            if(compareNeighbours) {
                node.ChangeFloorSprite(CompareNeighbourFloor(node.GetPosition(), selectID, false), true);
            }
        }

        node = manager.GetNode(position.x - 1, position.y);
        if(node != null && node.floorName == thisFloor) {
            name += "W";
            if(compareNeighbours) {
                node.ChangeFloorSprite(CompareNeighbourFloor(node.GetPosition(), selectID, false), true);
            }
        }

        for(int i = 0; i < objeto.spriteObjeto.Length; i++) {
            if(objeto.spriteObjeto[i].name == name) {
                return objeto.spriteObjeto[i];
            }
        }

        return objeto.spriteObjeto[0];
    }

    Sprite ShowSprite (int selectID, int id) {
        //Si no dispone de sprite, devuelve error.
        if(construcciones[selectID].spriteObjeto == null || construcciones[selectID].spriteObjeto.Length == 0 || id >= construcciones[selectID].spriteObjeto.Length) {
            Debug.LogWarning("Construccion::CompareNeighbour error: " + construcciones[selectID].nombre + " no tiene sprites asignados.");
            return null;
        }
        
        return construcciones[selectID].spriteObjeto[id];
    }

    /// <summary>
    /// Oculta la información
    /// </summary>
    void HideInformation() {
        cartel.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class ObjetoTienda {
    public string nombre;
    [TextArea(3, 5)]
    public string descripcion;

    [Space(5)]
    public Sprite[] spriteObjeto;
    public int showSprite;
    public Sprite spriteModelo;

    [Space(5)]
    public int tiempo = 5;
    public bool seguirConstruyendo;
    public CONSTRUCCION categoria;
    public TIPOCONSTRUCCION tipoConstruccion;
    public INVESTIGACION investigacionNec;

    [Header ("Prefab. Si es suelo se cambiará directamente del render.")]
    [Space(5)]
    public GameObject prefab;

    [Space(5)]
    public ObjetoRecursos[] recursosNecesarios;

    [Space(5)]
    public Vector2 entrada = Vector2.zero;                                  //En caso de ocupar más de 1 espacio, cual de ellas es la importante.   
    public Vector2[] posicionesExtras = new Vector2[1] { Vector2.zero };    //Cuanto espacio ocupa la estructura. Por defecto solo 1.
}

[System.Serializable]
public class ObjetoRecursos {
    public RECURSOS recurso;
    public int cantidadNecesaria;
}