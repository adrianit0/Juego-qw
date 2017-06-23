using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum CONSTRUCCION { Estructuras = 0, Agricultura = 1, Investigacion = 2, Electricidad = 3, Agua = 4 }
public enum INVESTIGACION { Ninguno = 0 }

//TODO: Volver a hacer funcional lo de los iconos de los recursos en la construccion

public class Construccion : MonoBehaviour {

    public ObjetoTienda[] construcciones = new ObjetoTienda[1];

    public GameObject[] panelesConstruccion = new GameObject[5];

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

            _obj.transform.GetChild(0).GetComponent<Image>().sprite = construcciones[i].spriteObjeto; 

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

            for (int i = 0; i < construcciones[selectID].posicionesExtras.Length; i++) {
                int x2 = Mathf.RoundToInt(construcciones[selectID].posicionesExtras[i].x), y2 = Mathf.RoundToInt(construcciones[selectID].posicionesExtras[i].y);

                Node nodo = manager.GetNode(x + x2, y + y2);
                if(x+x2< 0 || y + y2 < 0 || x+x2 >= manager.totalSize.x || y + y2 >= manager.totalSize.y || nodo.GetBuildType() != ESTRUCTURA.Ninguno || nodo.GetMovementCost()==0) {
                    interfazConstructor.color = new Color(1, 0, 0, 0.75f);
                    break;
                }
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
    public Sprite spriteObjeto;
    public Sprite spriteModelo;

    [Space (5)]
    public GameObject prefab;

    [Space(5)]
    public int tiempo = 5;
    public CONSTRUCCION categoria;
    public INVESTIGACION investigacionNec;

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