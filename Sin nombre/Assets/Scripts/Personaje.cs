using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour, IEquipo {

    public float velocity = 5;
    public int maxSteps = 0;

    public int capacidadTotal = 30;
    public int capacidadActual {
        set { }
        get { return CountItems(); }
    }
    public List<ResourceInfo> inventario = new List<ResourceInfo>();

    bool canWalk = false;

    //GAMEMANAGER.
    public GameManager manager;

    //LINE RENDERER.
    public GameObject contentCharacter;
    LineRenderer line;
    List<Vector3> _positions;
    
    //VALORES INTERNOS.
    float distancePos = 0.25f;
    float distanceFinal = 1.00f;

    //ACCION QUE ESTÁ REALIZANDO ACTUALMENTE EL PERSONAJE.
    List<Action> actions = new List<Action>();
    float timeBetweenActions = 0f;
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

        manager.characters.Add(this);
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

                if(timeBetweenActions == 0) {
                    lineAction.gameObject.SetActive(true);
                    line.enabled = false;
                    anim.SetBool("Working", true);
                }

                timeBetweenActions += Time.deltaTime;
                PercentAction(timeBetweenActions/action.totalTime);

                if (timeBetweenActions > action.totalTime) {
                    //TERMINA LA ACCION
                    
                    Destroy(action.renderIcon.gameObject);

                    switch (action.tipo) {
                        case TIPOACCION.Almacenar:
                            CleanResource(action.warehouseAction);
                            break;
                        case TIPOACCION.Talar:
                        case TIPOACCION.Minar:
                        case TIPOACCION.Pescar:
                            AddResource(action.resourceAction);

                            if (capacidadActual < capacidadTotal && action.resourceAction.actualQuantity > 0) {
                                AddAction(manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                            } else if (manager.ExistBuild (ESTRUCTURA.Almacen)) {
                                Vector2 pos = manager.GetNearBuild(transform.position, ESTRUCTURA.Almacen);
                                AddAction (manager.CreateAction(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), HERRAMIENTA.Recolectar));

                                if (action.resourceAction.actualQuantity>0) {
                                    AddAction (manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                                }
                            }
                            break;

                        case TIPOACCION.Arar:
                        case TIPOACCION.Construir:
                            manager.CreateBuild(action.position, action.prefab);
                            break;
                    }

                    actions.RemoveAt(0);

                    if(actions.Count == 0) {
                        SetPositions(Vector3.zero);
                    } else {
                        SetPositions(manager.PathFinding(this, actions[0].position));
                    }

                    timeBetweenActions = 0;
                    lineAction.gameObject.SetActive(false);
                    line.enabled = true;

                    anim.SetBool("Working", false);
                }

            } else {
                if(!canWalk) {
                    anim.SetBool("Mov", false);
                    return;
                }

                if (_positions.Count>=1) {
                    transform.position += (-_positions[0] + _positions[1]).normalized * velocity * Time.deltaTime;
                    transform.localScale = new Vector3(Mathf.Sign(_positions[0].x - _positions[1].x), 1, 1);
                    contentCharacter.transform.localScale = new Vector3(Mathf.Sign(_positions[0].x - _positions[1].x), 1, 1);
                } else {
                    transform.position += (action.position-transform.position).normalized * velocity * Time.deltaTime;
                    transform.localScale = new Vector3(Mathf.Sign(transform.position.x - action.position.x), 1, 1);
                    contentCharacter.transform.localScale = new Vector3(Mathf.Sign(transform.position.x - action.position.x), 1, 1);
                }
                
                anim.SetBool("Mov", true);
            }
        } else {
            anim.SetBool("Mov", false);
        }
    }

    public void AddAction (Action action) {
        actions.Add (action);
        
        action.renderIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
    }

    public int GetActionsCount () {
        return actions.Count;
    }
         
    public void AddResource(Recurso recurso) {
        if(recurso == null)
            return;

        ResourceInfo[] recursos = recurso.TomarRecursos(capacidadTotal-capacidadActual);

        if(recursos == null)
            return;
        
        for (int i = 0; i < recursos.Length; i++) {
            AddResource(recursos[i].type, recursos[i].quantity);
        }
    }

    public int CountItems () {
        int count = 0;
        for(int i = 0; i < inventario.Count; i++) {
            count += inventario[i].quantity;
        }
        return count;
    }

    public int AddResource(RECURSOS recurso, int cantidad) {
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity += cantidad;

                return 0;
            }
        }

        inventario.Add(new ResourceInfo(recurso, cantidad));

        return 0;
    }

    public void CleanResource (Almacen almacen) {
        for (int i =0;i < inventario.Count; i++) {
            int sobrante = almacen.AddResource(inventario[i].type, inventario[i].quantity);
            inventario[i].quantity = sobrante;
        }
    }

    public void SetPositions (params Vector3[] pos) {
        if (pos == null ||pos.Length==0) {
            pos = new Vector3[1] { Vector3.zero };
        }

        _positions = new List<Vector3>(pos);

        canWalk = true;

        ReiniciarLine();
    }

    void UpdateLine () {
        if(_positions == null || _positions.Count <= 1) {
            canWalk = false;
            return;
        }
        
        _positions[0] = transform.position;
        line.SetPosition(0, _positions[0]);

        bool cambiar = false;
        if (Vector3.Distance (_positions[0], _positions[1])<= distancePos) {
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
