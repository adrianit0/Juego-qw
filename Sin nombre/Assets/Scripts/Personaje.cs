using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Método que sea buscar un camino con una excepcion, tal como:
//void GetPosition (PathSettings ajustes, UnityAction excepcion);

/// <summary>
/// Clase que controla al personaje.
/// </summary>
public class Personaje : MonoBehaviour, IEquipo {

    public float velocity = 5;
    public int maxSteps = 0;

    [Header("Valores básicos")]
    [Space (10)]
    public int nivel = 0;

    [Range(-3, 5)]
    public int salud, estress;

    [Header ("Atributos:")]
    [Space (10)]
    [Range(-3, 5)]
    public int constitucion;

    [Range(-3, 5)]
    public int atlestismo, mineria, recoleccion, construccion, ingenio, carisma, culinario;
    
    public Inventario _inventario;

    bool canWalk = false;

    //SPRITES
    [Header("Gráficos del personaje:")]
    [Space (10)]
    public SpriteRenderer body;
    public SpriteRenderer head;
    public SpriteRenderer mask;

    //GAMEMANAGER.
    public GameManager manager;

    //LINE RENDERER.
    public GameObject contentCharacter;
    LineRenderer line;
    List<Vector3> _positions;
    
    //VALORES INTERNOS.
    float distancePos = 0.25f;
    float distanceFinal = 1.1f;

    //ACCION QUE ESTÁ REALIZANDO ACTUALMENTE EL PERSONAJE.
    List<Action> actions = new List<Action>();
    public LineRenderer lineAction;
    Animator anim;
    
    void Awake() {
        line = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();
        if(line == null) {
            line = gameObject.AddComponent<LineRenderer>();
        }
    }
    
	void Start () {
        canWalk = false;
        _inventario = new Inventario(40);
        _inventario.equipo = (IEquipo) this;

        manager.characters.Add(this);

        velocity = 3 + atlestismo;
	}
    
    void Update() {
        //Actualizar el LineRenderer
        UpdateLine();
        
        if (actions.Count>0) {
            Action action = actions[0];
            if(Vector3.Distance(transform.position, action.position) <= distanceFinal) {
                
                if (action == null) {
                    actions.RemoveAt(0);
                    return;
                }

                if(action.actualTime == 0) {
                    lineAction.gameObject.SetActive(true);
                    line.enabled = false;
                    anim.SetBool("Working", true);

                    //Primera vez usa la accion.
                    switch (action.tipo) {
                        case TIPOACCION.Talar:
                        case TIPOACCION.RecogerObjeto:
                        case TIPOACCION.Cosechar:
                            if (recoleccion>0) {
                                action.totalTime *= (1 - (0.15f * recoleccion));
                            }
                            
                            break;
                        case TIPOACCION.Construir:

                            break;
                    }
                }

                action.actualTime += Time.deltaTime;
                PercentAction(action.actualTime / action.totalTime);

                if (action.actualTime > action.totalTime) {
                    //TERMINA LA ACCION
                    RealizarAccion(action);

                } else {
                    //Acciones que realiza mientras realiza la acción, actualmente desactivado.

                }

            } else {
                if(!canWalk) {
                    anim.SetBool("Mov", false);
                    return;
                }

                if (_positions.Count>1) {
                    transform.position += (-_positions[0] + _positions[1]).normalized * velocity * Time.deltaTime;
                    transform.localScale = new Vector3(Mathf.Sign(_positions[0].x - _positions[1].x), 1, 1);
                    contentCharacter.transform.localScale = new Vector3(Mathf.Sign(_positions[0].x - _positions[1].x), 1, 1);
                } else {
                    transform.position += (action.position-transform.position).normalized * velocity * Time.deltaTime;
                    transform.localScale = new Vector3(Mathf.Sign(transform.position.x - action.position.x), 1, 1);
                    contentCharacter.transform.localScale = new Vector3(Mathf.Sign(transform.position.x - action.position.x), 1, 1);
                }

                body.sortingOrder = manager.SetSortingLayer(transform.position.y);
                head.sortingOrder = manager.SetSortingLayer(transform.position.y) + 1;
                mask.sortingOrder = manager.SetSortingLayer(transform.position.y) + 2;

                anim.SetBool("Mov", true);
            }
        } else {
            anim.SetBool("Mov", false);
        }
    }

    void RealizarAccion (Action action) {
        RemoveAction(action);

        switch(action.tipo) {
            case TIPOACCION.Almacenar:
                _inventario.CleanResource(action.warehouseAction.inventario);
                if (_inventario.Count>0) {
                    BuscarAlmacenCercano();
                }
                break;

            case TIPOACCION.SacarAlmacen:
                for(int i = 0; i < action.recursosNecesarios.Count; i++) {
                    int restante = action.warehouseAction.inventario.GetResource(action.recursosNecesarios[i].type, action.recursosNecesarios[i].quantity, _inventario);
                    if (restante>0) {
                        //Buscarlo en otro almacén
                    }
                }
                break;

            case TIPOACCION.VaciarAlmacen:
                action.warehouseAction.OnDestroyBuild();
                break;

            case TIPOACCION.Talar:
            case TIPOACCION.Minar:
            case TIPOACCION.Cosechar:
            case TIPOACCION.RecogerObjeto:
                _inventario.AddResource(action.resourceAction);

                if(!_inventario.IsFull() && action.resourceAction.actualQuantity > 0) {
                    AddAction(manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                } else if(manager.ExistBuild(ESTRUCTURA.Almacen)) {
                    BuscarAlmacenCercano();

                    if(action.resourceAction.actualQuantity > 0) {
                        AddAction(manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                    }
                }
                break;

            case TIPOACCION.ExtraerAgua:
                _inventario.aguaTotal = new Fluido(_inventario.litrosTotales, action.estructure.GetComponent<Agua>().agua);
                break;

            case TIPOACCION.Regar:
                Huerto huerto = action.estructure.GetComponent<Huerto>();
                if (huerto!=null) {
                    huerto.Regar(_inventario.aguaTotal);
                }

                break;

            case TIPOACCION.Plantar:
                huerto = action.estructure.GetComponent<Huerto>();

                if(huerto != null) {
                    huerto.Cultivar(action.recursosNecesarios[0].type);
                }
                break;

            case TIPOACCION.Pescar:
                Agua agua = action.estructure.GetComponent<Agua>();
                if (agua != null) {
                    bool obtenido = agua.Pescar();
                    if (obtenido) {
                        _inventario.AddResource(RECURSOS.Sardina, 1);
                        BuscarAlmacenCercano();
                    }

                    AddAction(manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                }
                break;

            case TIPOACCION.Arar:
                manager.CreateBuild(action.position, action.prefab);
                break;
                
            case TIPOACCION.Construir:
                Estructura _build = manager.CreateBuild(action.position, action.prefab);
                for(int i = 0; i < manager.build.construcciones[action.buildID].posicionesExtras.Length; i++) {
                    manager.AddBuildInMap(action.position + (Vector3) manager.build.construcciones[action.buildID].posicionesExtras[i], _build);
                }
                break;

            case TIPOACCION.Destruir:
                action.estructure.AlDestuirse();

                if (action.estructure != null)
                    Destroy(action.estructure.gameObject);

                //Si hubiera alguien realizando alguna acción lo cancelaría automaticamente al destruirse.
                manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Cancelar);
                break;
        }
        
        lineAction.gameObject.SetActive(false);
        line.enabled = true;

        anim.SetBool("Working", false);
    }

    public void AddAction(Action action, int insertAt = -1) {
        if(action == null)
            return;

        bool seguir =  OnActionReceived(action);

        if(!seguir)
            return;

        action.worker = this;

        manager.actualActions.Add(action);
        if (insertAt<=-1)
            actions.Add (action);
        else {
            actions.Insert(insertAt, action);

            if (insertAt==0) {
                SetPositions (manager.PathFinding(this, action.position));
            }
        }
        
        action.renderIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
    }
    
    public void RemoveAction (Action action) {
        if(action.renderIcon != null)
            Destroy(action.renderIcon.gameObject);
        
        if (actions.Contains (action)) {
            actions.Remove(action);
        }

        if(manager.actualActions.Contains(action))
            manager.actualActions.Remove(action);

        if (actions.Count == 0) {
            lineAction.gameObject.SetActive(false);
            line.enabled = false;

            anim.SetBool("Working", false);
        }

        if (_inventario.Count>0) {
            BuscarAlmacenCercano();
        }

        SiguienteAccion();
    }

    //Un "evento" que se activa cuando va a realizar una acción.
    bool OnActionReceived (Action action) {
        switch (action.tipo) {
            case TIPOACCION.Construir:
                //Va a buscar el material necesario.
                IntVector2 _pos = manager.PathFinding(this, new PathSetting(action.recursosNecesarios)).GetFinalPosition(); ;
                AddAction(manager.CreateAction(_pos.x, _pos.y, HERRAMIENTA.Custom, new CustomAction(TIPOACCION.SacarAlmacen, true, action.recursosNecesarios)), 0);

                break;

            case TIPOACCION.Regar:
                if (_inventario.aguaTotal.litrosTotales==0) {
                    _pos = manager.PathFinding(this, new PathSetting(TIPOAGUA.AguaDulce, 0.5f)).GetFinalPosition(); ;
                    if (_pos != Vector3.zero) {
                        AddAction(manager.CreateAction(_pos.x, _pos.y, HERRAMIENTA.Custom, new CustomAction(TIPOACCION.ExtraerAgua, false, null)), 0);
                    } else {
                        //Si no encuentra el agua cancela la acción.
                        return false;
                    }
                }
                break;
        }
        return true;
    }

    public int GetActionsCount () {
        return actions.Count;
    }

    void SiguienteAccion() {
        if(actions.Count == 0) {
            SetPositions(Vector3.zero);
        } else {
            if(actions[0].pathResult == null) {
                SetPositions(manager.PathFinding(this, actions[0].position));
            } else {
                SetPositions(actions[0].pathResult.path);
            }
        }
    }

    void BuscarAlmacenCercano () {
        IntVector2 pos = manager.PathFinding(this, new PathSetting(PATHTYPE.AlmacenEspacio)).GetFinalPosition();
        if(pos != new IntVector2(0, 0)) {
            AddAction(manager.CreateAction(pos.x, pos.y, HERRAMIENTA.Custom, new CustomAction(TIPOACCION.Almacenar, true, _inventario.inventario)));
        } else {
            manager.CrearSaco(transform.position, maxSteps, _inventario.ToArray());
            _inventario.CleanResource();
        }
    }

    bool BuscarAguaCercana(TIPOAGUA agua, float minNecesario) {
        IntVector2 pos = manager.PathFinding(this, new PathSetting(agua, minNecesario)).GetFinalPosition();
        if(pos != new IntVector2(0, 0)) {
            AddAction(manager.CreateAction(pos.x, pos.y, HERRAMIENTA.Custom, new CustomAction(TIPOACCION.ExtraerAgua, false, null)));
            return true;
        } else {
            //No pasa nada
            return false;
        }
    }

    public void OnCapacityChange(params ResourceInfo[] recursos) {
        //Pues no pasa nada...
    }

    public void SetPositions (params IntVector2[] pos) {
        if (pos == null ||pos.Length==0) {
            pos = new IntVector2[1] { new IntVector2(0, 0) };
        }

        _positions = new List<Vector3>();

        for (int i = 0; i < pos.Length; i++) {
            _positions.Add((Vector3) pos[i]);
        }

        line.enabled = true;

        canWalk = true;

        ReiniciarLine();
    }

    void UpdateLine () {
        if(_positions == null || _positions.Count <= 1) {
            canWalk = actions.Count>0 ? true : false;
            return;
        }
        
        _positions[0] = transform.position;
        line.SetPosition(0, _positions[0]);

        bool cambiar = false;
        if (Vector3.Distance (_positions[0], _positions[1]) <= distancePos) {
            _positions.RemoveAt(1);
            cambiar = true;
        }
        
        if (cambiar) {
            line.numPositions = _positions.Count;
            line.SetPositions(_positions.ToArray());
        }
    }

    void ReiniciarLine () {
        line.numPositions = _positions.Count;
        line.SetPositions(_positions.ToArray());
    }

    public void PercentAction (float porc) {
        porc = Mathf.Clamp(porc, 0, 1);
        lineAction.SetPosition(1, new Vector3(porc-0.5f, 0, 0));
    }
}
