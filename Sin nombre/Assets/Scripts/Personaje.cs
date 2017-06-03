using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour {

    public float velocity = 5;
    public int maxSteps = 0;

    bool canWalk = false;

    //GAMEMANAGER.
    public GameManager manager;
    
    //LINE RENDERER.
    LineRenderer line;
    List<Vector3> _positions;
    
    //VALORES INTERNOS.
    float distancePos = 0.25f;
    float distanceFinal = 1.00f;

    //ACCION QUE ESTÁ REALIZANDO ACTUALMENTE EL PERSONAJE.
    public Action action;
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

        if(!canWalk || _positions.Count <= 1) {
            anim.SetBool("Mov", false);
            return;
        }
            


        if (action != null) {
            if(Vector3.Distance(transform.position, _positions[_positions.Count-1]) <= distanceFinal) {
                if(timeBetweenActions == 0) {
                    lineAction.gameObject.SetActive(true);
                    line.enabled = false;
                    anim.SetBool("Working", true);
                }
                timeBetweenActions += Time.deltaTime;
                PercentAction(timeBetweenActions/action.resourceAction.tiempoTotal);

                if (timeBetweenActions > action.resourceAction.tiempoTotal) {
                    manager.AddResource(action.resourceAction);

                    Destroy (action.renderIcon.gameObject);
                    action = null;
                    timeBetweenActions = 0;
                    lineAction.gameObject.SetActive(false);
                    line.enabled = true;

                    anim.SetBool("Working", false);

                    SetPositions(Vector3.zero);
                }

            } else {
                transform.position += (-_positions[0] + _positions[1]).normalized * velocity * Time.deltaTime;
                transform.localScale = new Vector3(Mathf.Sign(_positions[0].x - _positions[1].x), 1, 1);
                anim.SetBool("Mov", true);
            }
        }
    }

    public void SetPositions (params Vector3[] pos) {
        _positions = new List<Vector3>(pos);

        if (pos == null ||pos.Length==0) {
            return;
        }

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
