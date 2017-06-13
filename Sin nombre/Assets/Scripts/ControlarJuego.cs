using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Es el Update del GameManager.
public class ControlarJuego : MonoBehaviour {

    public SpriteRenderer seleccion;
    public bool desactivarBotonDerecho = false;
    bool pulsandoBotonDerecho = false;

    Vector2 posInicial, posFinal;
    Estructura primeraEstructura;

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

        if(Input.GetMouseButtonUp(1) && !desactivarBotonDerecho && !pulsandoBotonDerecho && !EventSystem.current.IsPointerOverGameObject()) {
            int _x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x);
            int _y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            if(_x < 0 || _y < 0 || _x >= manager.totalSize.x || _y >= manager.totalSize.y)
                return;

            manager.map[_x, _y].bloqueado = !manager.map[_x, _y].bloqueado;
            manager.map[_x, _y].coll.isTrigger = !manager.map[_x, _y].bloqueado;

            if (manager.map[_x, _y].bloqueado) {
                manager.CreateBuild(new Vector3(_x, _y), manager.agua);
            } else {
                if (manager.map[_x, _y].estructura != null && manager.map[_x, _y].estructura.tipo == ESTRUCTURA.Agua)
                    manager.RemoveBuildInMap(new Vector3(_x, _y));
            }
            //mapa[_x, _y].render.sprite = (!mapa[_x, _y].bloqueado) ? spriteTierra : spriteAgua;

            manager.UpdateMap();
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            SeleccionarObjetivo(ScreenToWorldSpaceFixed(Input.mousePosition));
        }

        if (Input.GetMouseButton (0) && pulsandoBotonDerecho) {

            ActualizarObjetivo(ScreenToWorldSpaceFixed(Input.mousePosition));
            
            if(Input.GetMouseButtonUp(1)) {
                CancelarObjetivo();
            }
        }

        if (Input.GetMouseButtonUp(0) && pulsandoBotonDerecho) {
            FijarObjetivo();
        }
    }

    void SeleccionarObjetivo (Vector2 pos) {
        pulsandoBotonDerecho = true;
 
        posInicial = pos;
        posFinal = pos;

        if (manager.herramientaSeleccionada == HERRAMIENTA.Seleccionar) {
            primeraEstructura = manager.map[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)].estructura;
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

    void FijarObjetivo () {
        if (manager.herramientaSeleccionada != HERRAMIENTA.Seleccionar) {
            //CREA UNA ACCION
            for(int y = (int) Mathf.Min(posInicial.y, posFinal.y); y < Mathf.Max(posInicial.y, posFinal.y) + 1; y++) {
                for(int x = (int) Mathf.Min(posInicial.x, posFinal.x); x < Mathf.Max(posInicial.x, posFinal.x) + 1; x++) {
                    manager.AddAction(x, y, manager.herramientaSeleccionada);
                }
            }
        } else {
            //MUESTRA LA INFORMACIÓN
            List<Estructura> estructuras = new List<Estructura>();
            List<GameObject> selecciones = new List<GameObject>();
            for(int y = (int) Mathf.Min(posInicial.y, posFinal.y); y < Mathf.Max(posInicial.y, posFinal.y) + 1; y++) {
                for(int x = (int) Mathf.Min(posInicial.x, posFinal.x); x < Mathf.Max(posInicial.x, posFinal.x) + 1; x++) {
                    if (primeraEstructura == null && manager.map[x, y].estructura != null) {
                        primeraEstructura = manager.map[x, y].estructura;
                    }

                    if (manager.map[x, y].estructura != null && manager.map[x, y].estructura.tipo == primeraEstructura.tipo) {
                        estructuras.Add(manager.map[x, y].estructura);

                        GameObject _obj = Instantiate (manager.info.seleccionPrefab);
                        _obj.transform.position = new Vector3(x, y);
                        selecciones.Add(_obj);
                    }
                }
            }

            manager.info.SeleccionarUnidades(primeraEstructura, estructuras.ToArray(), selecciones.ToArray());

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