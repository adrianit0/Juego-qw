using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Es el Update del GameManager.
public class ControlarJuego : MonoBehaviour {

    public SpriteRenderer seleccion;
    public Camera secondCamera;
    public bool desactivarBotonDerecho = false;
    bool pulsandoBotonDerecho = false;

    IntVector2 posInicial, posFinal;
    Estructura primeraEstructura;

    Vector3 lastFramePosition;

    GameManager manager;
    Construccion build;

    void Awake() {
        manager = GetComponent<GameManager>();
        build = GetComponent<Construccion>();
    }

    void Start() {
        seleccion.gameObject.SetActive(false);
    }

    void Update() {
        if(build.construyendo) {
            //Si va a construir hace uso de su propio Update().
            build.BuildUpdate();
            return;
        }
        
        //AsignarAgua();
        UpdateMoverCamaraBoton();
        UpdateMoverCamaraMouse();
        UpdateZoom();
        InteractuarMapa();
    }

    /*void AsignarAgua () {
        if(Input.GetMouseButtonUp(1) && !desactivarBotonDerecho && !pulsandoBotonDerecho && !EventSystem.current.IsPointerOverGameObject()) {
            int _x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            if(_x < 0 || _y < 0 || _x >= manager.totalSize.x || _y >= manager.totalSize.y)
                return;

            manager.map[_x, _y].bloqueado = !manager.map[_x, _y].bloqueado;
            manager.map[_x, _y].coll.isTrigger = !manager.map[_x, _y].bloqueado;

            if(manager.map[_x, _y].bloqueado) {
                manager.CreateBuild(new Vector3(_x, _y), manager.agua);
            } else {
                if(manager.map[_x, _y].estructura != null && manager.map[_x, _y].estructura.tipo == ESTRUCTURA.Agua)
                    manager.RemoveBuildInMap(new Vector3(_x, _y));
            }
            //mapa[_x, _y].render.sprite = (!mapa[_x, _y].bloqueado) ? spriteTierra : spriteAgua;

            manager.UpdateMap();
        }
    }*/

    void UpdateMoverCamaraBoton () {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if(horizontal != 0 || vertical != 0) {
            Camera.main.transform.position += new Vector3(horizontal, vertical, 0) * 0.25f;
            secondCamera.transform.position += new Vector3(horizontal, 0, vertical) * 0.25f;
        }
    }

    void UpdateMoverCamaraMouse () {
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(1)) {
            Camera.main.transform.Translate (lastFramePosition - currentPos);
        }

        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); ;
    }

    void UpdateZoom() {
        if (!EventSystem.current.IsPointerOverGameObject())
            Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize - (Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel")), 3, 10);
    } 

    void InteractuarMapa () {
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            SeleccionarObjetivo(ScreenToWorldSpaceFixed(Input.mousePosition));
        }

        if(Input.GetMouseButton(0) && pulsandoBotonDerecho) {
            ActualizarObjetivo(ScreenToWorldSpaceFixed(Input.mousePosition));

            if(Input.GetMouseButtonUp(1)) {
                CancelarObjetivo();
            }
        }

        if(Input.GetMouseButtonUp(0) && pulsandoBotonDerecho) {
            FijarObjetivo();
        }
    }

    void SeleccionarObjetivo (IntVector2 pos) {
        pulsandoBotonDerecho = true;
 
        posInicial = pos;
        posFinal = pos;

        if (manager.herramientaSeleccionada == HERRAMIENTA.Seleccionar) {
            primeraEstructura = manager.GetNode(pos.x, pos.y).GetBuild();
        }
    }

    void ActualizarObjetivo (Vector2 pos) {
        if (pos != posFinal) {
            posFinal = pos;

            seleccion.gameObject.SetActive(true);

            Vector2 _posIni = new Vector2(
                posInicial.x >= posFinal.x ? posInicial.x + 0.5f : posInicial.x - 0.5f, 
                posInicial.y > posFinal.y ? posInicial.y + 0.5f : posInicial.y - 0.5f
                );

            Vector2 _posFinal = new Vector2(
                posFinal.x > posInicial.x ? posFinal.x + 0.5f : posFinal.x - 0.5f, 
                posFinal.y >= posInicial.y ? posFinal.y + 0.5f : posFinal.y - 0.5f
                );

            seleccion.transform.position = (_posIni + _posFinal) / 2;
            seleccion.transform.localScale = _posIni - _posFinal;
        }
    }

    /// <summary>
    /// Fija el objetivo
    /// Actualmente desactivado todo lo que no sea la herramienta "SELECCIONAR"
    /// </summary>
    void FijarObjetivo () {
        int maxY = Mathf.Max(posInicial.y, posFinal.y) + 1;
        int maxX = Mathf.Max(posInicial.x, posFinal.x) + 1;
        if (manager.herramientaSeleccionada != HERRAMIENTA.Seleccionar) {
            //CREA UNA ACCION
            for(int y = (int) Mathf.Min(posInicial.y, posFinal.y); y < maxY; y++) {
                for(int x = (int) Mathf.Min(posInicial.x, posFinal.x); x < maxX; x++) {
                    //TODO: Arreglar
                    manager.actions.CreateAction(new IntVector2 (x, y), manager.herramientaSeleccionada, TIPOACCION.Almacenar, null, false);
                }
            }
        } else { 
            //MUESTRA LA INFORMACIÓN
            List<Estructura> estructuras = new List<Estructura>();
            List<GameObject> selecciones = new List<GameObject>();
            
            for(int y = Mathf.Min(posInicial.y, posFinal.y); y < maxY; y++) {
                for(int x = Mathf.Min(posInicial.x, posFinal.x); x < maxX; x++) {
                    Estructura build = manager.GetNode(x, y).GetBuild();

                    if (primeraEstructura == null && build != null) {
                        primeraEstructura = build;
                    }

                    if (build != null && build.GetBuildType() == primeraEstructura.GetBuildType()) {
                        estructuras.Add(build);

                        GameObject _obj = manager.info.GetSeleccion();
                        _obj.transform.position = new Vector3(x, y);
                        selecciones.Add(_obj);
                    }
                }
            }

            //Si ha encontrado alguna estructura mostrará su información
            //Si no mostrará la información del suelo.
            if(selecciones.Count > 0) {
                manager.info.SeleccionarUnidades(primeraEstructura, estructuras.ToArray(), selecciones.ToArray());
            } else {
                for(int y = Mathf.Min(posInicial.y, posFinal.y); y < maxY; y++) {
                    for(int x = Mathf.Min(posInicial.x, posFinal.x); x < maxX; x++) {
                        GameObject _obj = manager.info.GetSeleccion();
                        _obj.transform.position = new Vector3(x, y);
                        selecciones.Add(_obj);
                    }
                }

                manager.info.SeleccionarTerreno(selecciones.ToArray());
            }
        }

        CancelarObjetivo();
    }

    void CancelarObjetivo () {
        pulsandoBotonDerecho = false;
        seleccion.gameObject.SetActive(false);
    }

    Vector2 ScreenToWorldSpaceFixed (Vector2 screenPosition) {
        return new Vector2(Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(screenPosition).x), Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(screenPosition).y));
    }
}