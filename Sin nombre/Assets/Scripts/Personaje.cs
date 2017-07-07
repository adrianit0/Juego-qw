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

    float tiempoInicialTrabajo = 0;
    
    //Inventario, donde lleva los recursos
    public Inventario inventario { get; private set; }

    //Cantidad de agua que el personaje tiene y puede llevar.
    public Fluido aguaTotal { get; private set; }
    
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
    List<GameAction> actions;
    public LineRenderer lineAction;
    Animator anim;
    
    void Awake() {
        line = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();
        if(line == null) {
            line = gameObject.AddComponent<LineRenderer>();
        }

        actions = new List<GameAction>();
    }
    
	void Start () {
        canWalk = false;
        inventario = new Inventario(40, manager);
        aguaTotal = new Fluido(8);

        inventario.SetInterface((IEquipo) this);

        manager.AddCharacter(this);

        velocity = 3 + atlestismo;
	}
    
    void Update() {
        //Actualizar el LineRenderer
        UpdateLine();
        
        if (actions.Count>0) {
            GameAction action = actions[0];
            if(Vector3.Distance(transform.position, (Vector3) action.node.GetPosition ()) <= distanceFinal) {
                
                if (action == null) {
                    actions.RemoveAt(0);
                    return;
                }

                //TODO: Arreglar
                if(tiempoInicialTrabajo == 0) {
                    tiempoInicialTrabajo = action.totalTime;
                    lineAction.gameObject.SetActive(true);
                    line.enabled = false;
                    anim.SetBool("Working", true);

                    action.RealizeAction(ACTIONEVENT.BeforeStart);
                }

                action.totalTime -= Time.deltaTime;
                PercentAction(1- action.totalTime / tiempoInicialTrabajo);

                if (action.totalTime<0) {
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
                    transform.position += ((Vector3) action.node.GetPosition()-transform.position).normalized * velocity * Time.deltaTime;
                    transform.localScale = new Vector3(Mathf.Sign(transform.position.x - action.node.GetPosition().x), 1, 1);
                    contentCharacter.transform.localScale = new Vector3(Mathf.Sign(transform.position.x - action.node.GetPosition().x), 1, 1);
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

    void RealizarAccion (GameAction action) {
        action.RealizeAction(ACTIONEVENT.OnCompleted);

        RemoveAction(action);
        tiempoInicialTrabajo = 0;

        lineAction.gameObject.SetActive(false);
        line.enabled = true;

        anim.SetBool("Working", false);

        //Si se ha quedado sin acciones que busque una entre la lista de acciones.
        if (actions.Count==0) {
            manager.actions.actionsQueue.AssignActionCharacter(this);
        }
    }

    public void AddAction(GameAction action, int insertAt = -1) {
        if(action == null)
            return;

        action.RealizeAction(ACTIONEVENT.OnAwake);

        if (action.worker == null)
            action.AssignCharacter(this);

        if (insertAt<=-1)
            actions.Add (action);
        else {
            actions.Insert(insertAt, action);

            if (insertAt==0) {
                tiempoInicialTrabajo = 0;
                SetPositions (manager.path.PathFind(this, new PathSetting( action.node.GetPosition ())).path);
            }
        }
        
        //TODO: Rehacer esto
        //action.renderIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
    }
    
    public void RemoveAction (GameAction action, bool removeFromQueue = true) {
        if (actions.Contains (action)) {
            actions.Remove(action);
        }

        if (removeFromQueue)
            manager.actions.RemoveAction(action);

        if (actions.Count == 0) {
            lineAction.gameObject.SetActive(false);
            line.enabled = false;
            tiempoInicialTrabajo = 0;

            anim.SetBool("Working", false);
        }

        if (inventario.Count>0 && actions.Count==0) {
            BuscarAlmacenCercano();
        }

        SiguienteAccion();
    }

    public bool IsWorking () {
        return actions != null && actions.Count > 0;
    }

    public int GetActionsCount () {
        return actions.Count;
    }

    void SiguienteAccion() {
        if(actions.Count == 0) {
            SetPositions(Vector3.zero);
        } else {
            //TODO: Arreglar el metodo del camino automatico.
            //if(actions[0].pathResult == null) {
                SetPositions(manager.path.PathFind(this, actions[0].node.GetPosition()));
            /*} else {
                SetPositions(actions[0].pathResult.path);
            }*/
        }
    }

    public void BuscarAlmacenCercano () {
        IntVector2 pos = manager.path.PathFind(this, new PathSetting(PATHTYPE.AlmacenEspacio)).GetFinalPosition();
        if(pos != new IntVector2(0, 0)) {
            AddAction(manager.actions.CreateAction(pos, HERRAMIENTA.Custom, TIPOACCION.Almacenar, this, true, -1, inventario.ToArray()));
        } else {
            manager.CrearSaco(transform.position, maxSteps, inventario.ToArray());
            inventario.CleanResource();
        }
    }

    bool BuscarAguaCercana(TIPOAGUA agua, float minNecesario) {
        IntVector2 pos = manager.path.PathFind(this, new PathSetting(agua, minNecesario)).GetFinalPosition();
        if(pos != new IntVector2(0, 0)) {
            AddAction(manager.actions.CreateAction(pos, HERRAMIENTA.Custom, TIPOACCION.ExtraerAgua, this, false, -1, null));
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
