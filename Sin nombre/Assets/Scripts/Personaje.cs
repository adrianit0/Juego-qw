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

    //SPRITES
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

                if(action.actualTime == 0) {
                    lineAction.gameObject.SetActive(true);
                    line.enabled = false;
                    anim.SetBool("Working", true);

                    //Primera vez usa la accion.
                    switch (action.tipo) {
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
        if(action.renderIcon != null)
            Destroy(action.renderIcon.gameObject);

        if(manager.actualActions.Contains(action))
            manager.actualActions.Remove(action);

        switch(action.tipo) {
            case TIPOACCION.Almacenar:
                CleanResource(action.warehouseAction);
                if (capacidadActual>0) {
                    BuscarAlmacenCercano();
                }
                break;
            case TIPOACCION.SacarAlmacen:
                for(int i = 0; i < action.recursosNecesarios.Count; i++) {
                    action.warehouseAction.GetResource(action.recursosNecesarios[i].type, action.recursosNecesarios[i].quantity, this);
                }

                break;
            case TIPOACCION.Talar:
            case TIPOACCION.Minar:
            case TIPOACCION.Pescar:
            case TIPOACCION.RecogerObjeto:
                AddResource(action.resourceAction);

                if(capacidadActual < capacidadTotal && action.resourceAction.actualQuantity > 0) {
                    AddAction(manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                } else if(manager.ExistBuild(ESTRUCTURA.Almacen)) {
                    BuscarAlmacenCercano();

                    if(action.resourceAction.actualQuantity > 0) {
                        AddAction(manager.CreateAction(Mathf.RoundToInt(action.position.x), Mathf.RoundToInt(action.position.y), HERRAMIENTA.Recolectar));
                    }
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
        }

        actions.RemoveAt(0);

        if(actions.Count == 0) {
            SetPositions(Vector3.zero);
        } else {
            if(actions[0].pathResult == null) {
                SetPositions(manager.PathFinding(this, actions[0].position));
            } else {
                SetPositions(actions[0].pathResult.path);
            }
        }

        lineAction.gameObject.SetActive(false);
        line.enabled = true;

        anim.SetBool("Working", false);
    }

    public void AddAction(Action action, int insertAt = -1) {
        OnActionReceived(action);

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

    //Un "evento" que se activa cuando va a realizar una acción.
    void OnActionReceived (Action action) {
        switch (action.tipo) {
            case TIPOACCION.Construir:
                //Va a buscar el material necesario.
                Vector3 _pos = manager.PathFinding(this, new PathSetting(action.recursosNecesarios)).finalPosition;
                AddAction(manager.CreateAction(Mathf.RoundToInt(_pos.x), Mathf.RoundToInt(_pos.y), HERRAMIENTA.Custom, new CustomAction(TIPOACCION.SacarAlmacen, action.recursosNecesarios)), 0);

                break;
        }
    }

    public int GetActionsCount () {
        return actions.Count;
    }

    void BuscarAlmacenCercano () {
        Vector3 pos = manager.PathFinding(this, new PathSetting(TIPOPATH.AlmacenEspacio)).finalPosition;
        if(pos != Vector3.zero) {
            AddAction(manager.CreateAction(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), HERRAMIENTA.Custom, new CustomAction(TIPOACCION.Almacenar, inventario)));
        } else {
            Debug.Log("Tiras los objetos en la posicion más cercana");
            pos = manager.PathFinding(this, new PathSetting(TIPOPATH.huecoLibre)).finalPosition;

            Estructura estructura = manager.CreateBuild(pos, manager.sacoObjetos);
            if(estructura != null) {
                Recurso _recurso = estructura.GetComponent<Recurso>();
                _recurso.CreateResource(inventario.ToArray());
                _recurso.transform.localScale = Vector3.Lerp(new Vector3(0.25f, 0.25f, 1), Vector3.one, ((float) _recurso.actualQuantity) / 100);
                CleanResource();
            }
        }
    }
         
    public void AddResource(Recurso recurso) {
        if(recurso == null)
            return;

        ResourceInfo[] recursos = recurso.GetResource(capacidadTotal-capacidadActual);

        if(recursos == null)
            return;
        
        for (int i = 0; i < recursos.Length; i++) {
            AddResource(recursos[i].type, recursos[i].quantity);
        }
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

    public int CountItem (RECURSOS recurso) {
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                return inventario[i].quantity;
            }
        }
        return 0;
    }

    public int CountItems() {
        int count = 0;
        for(int i = 0; i < inventario.Count; i++) {
            count += inventario[i].quantity;
        }
        return count;
    }
    
    public void RemoveResource(RECURSOS recurso, int cantidad) {
        if(cantidad == 0)
            return;

        bool encontrado = false;
        for(int i = 0; i < inventario.Count; i++) {
            if(inventario[i].type == recurso) {
                inventario[i].quantity -= cantidad;
                encontrado = true;
            }
        }

        if(!encontrado)
            return;

        capacidadActual -= cantidad;
    }

    public int GetResource(RECURSOS recurso, int cantidad) {
        if(cantidad == 0)
            return 0;

        int faltante = 0;

        int disponible = CountItem(recurso);

        if(cantidad > disponible) {
            faltante = cantidad - disponible;
            cantidad = disponible;
        }

        RemoveResource(recurso, cantidad);

        return faltante;
    }

    public void CleanResource (Almacen almacen) {
        for (int i =0;i < inventario.Count; i++) {
            int sobrante = almacen.AddResource(inventario[i].type, inventario[i].quantity);
            inventario[i].quantity = sobrante;
        }
    }

    public void CleanResource() {
        for(int i = 0; i < inventario.Count; i++) {
            inventario[i].quantity = 0;
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
