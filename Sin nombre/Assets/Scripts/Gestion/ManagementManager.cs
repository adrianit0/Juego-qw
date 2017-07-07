using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagementManager : MonoBehaviour {

    public GameObject prefabPanelGrupo;
    public GameObject prefabGrupo; 
    public GameObject prefabRecurso;

    public BotonGestion botonTodo;
    
    public GameObject panelPrincipal;
    public GameObject panelInferior;

    Dictionary<TIPORECURSO, GameObject> panelesTipos;
    List<BotonGestion> botonesGrupo;
    List<BotonGestion> botones;

    Almacen lastAlmacen;

    GameManager manager;

    void Awake() {
        manager = GetComponent<GameManager>();
    }

    void Start() {
        panelPrincipal.SetActive (false);
        CrearBotones();
    }

    /// <summary>
    /// Crea los botones necesarios para hacer funcional el sistema del panel.
    /// </summary>
    void CrearBotones () {
        panelesTipos = new Dictionary<TIPORECURSO, GameObject>();
        botonesGrupo = new List<BotonGestion>();
        botones = new List<BotonGestion>();

        botonTodo.toggle.onValueChanged.AddListener((newBool) => {
            ActualizarTodo(newBool);
        });
        
        foreach (TIPORECURSO _tipo in System.Enum.GetValues(typeof(TIPORECURSO))) {
            GameObject _obj = Instantiate(prefabPanelGrupo);
            _obj.transform.SetParent(panelInferior.transform);
            _obj.transform.localScale = new Vector3(1, 1, 1);

            panelesTipos.Add(_tipo, _obj);

            GameObject _objToggle = Instantiate(prefabGrupo);
            BotonGestion _script = _objToggle.GetComponent<BotonGestion>();

            _objToggle.transform.SetParent(_obj.transform);
            _objToggle.transform.localScale = new Vector3(1, 1, 1);
            
            _script.textoNombre.text = _tipo.ToString();
            _script.textoCantidad.text = "x0";
            _script.tipo = _tipo;

            TIPORECURSO _fixedType = _tipo;
            _script.toggle.onValueChanged.AddListener((newBool) => {
                ActualizarCategoria(_fixedType, newBool);
            });

            _script.toggleEncoger.onValueChanged.AddListener((newBool) => {
                EncogerCategoria(_script, _fixedType, newBool);
            });

            botonesGrupo.Add(_script);
        }

        foreach(ResourcePanel rec in manager.resourceController.panelRecurso) {
            GameObject _obj = Instantiate(prefabRecurso);
            BotonGestion _script = _obj.GetComponent<BotonGestion>();

            _obj.transform.SetParent(panelesTipos[rec.tipo].transform);
            _obj.transform.localScale = new Vector3(1, 1, 1);

            _script.imageSprite.sprite = rec.image;
            _script.textoNombre.text = rec.name;
            _script.textoCantidad.text = "x0";
            _script.recurso = rec.resource;
            _script.tipo = rec.tipo;

            _script.toggle.onValueChanged.AddListener((newBool) => { ActualizarContenido(_script, newBool); });
            _script.botonDeshechar.onClick.AddListener(() => {
                //Incluir contenido.
            });

            botones.Add(_script);
        }
    }

    /// <summary>
    /// Actualiza todo los botones.
    /// </summary>
    void ActualizarTodo (bool newValue) {
        if(lastAlmacen == null) {Debug.LogWarning("ManagementManager::ActualizarContenido error: No hay baul para actualizar el contenido");
            return;
        }

        for (int i = 0; i < botonesGrupo.Count; i++) {
            botonesGrupo[i].toggle.isOn = newValue;
        }
    }

    void ActualizarCategoria (TIPORECURSO tipo, bool newValue) {
        if(lastAlmacen == null) {
            //Debug.LogWarning("ManagementManager::ActualizarContenido error: No hay baul para actualizar el contenido");
            return;
        }

        for (int i = 0; i < botones.Count; i++) {
            if (botones[i].tipo == tipo) {
                botones[i].toggle.isOn = newValue;
            }
        }
    }

    void EncogerCategoria (BotonGestion boton, TIPORECURSO tipo, bool newValue) {
        boton.toggleEncoger.targetGraphic.rectTransform.localScale = new Vector3(1, newValue ? 1 : -1, 1);

        for(int i = 0; i < botones.Count; i++) {
            if(botones[i].tipo == tipo) {
                botones[i].gameObject.SetActive(newValue);
            }
        }
    }

    void ActualizarContenido (BotonGestion boton, bool newValue) {
        if (lastAlmacen == null) {
            //Debug.LogWarning("ManagementManager::ActualizarContenido error: No hay baul para actualizar el contenido");
            return;
        }

        lastAlmacen.inventario.limiteInventario.ChangeValue(boton.recurso, newValue);
    }

    public void AbrirBaul(Almacen baul) {
        lastAlmacen = null;

        panelPrincipal.SetActive(true);

        botonTodo.textoCantidad.text = "x" + baul.inventario.Count.ToString();

        int i = 0;
        foreach(KeyValuePair<TIPORECURSO, GameObject> key in panelesTipos) {
            botonesGrupo[i].textoCantidad.text = "x" + baul.inventario.GetResourceTypeCount(key.Key);
            i++;
        }

        for (i = 0; i < botones.Count; i++) {
            botones[i].textoCantidad.text = "x" + baul.inventario.GetResourceCount(botones[i].recurso);
            botones[i].toggle.isOn = baul.inventario.limiteInventario.lista[i].value;
        }

        lastAlmacen = baul;
    }
}
