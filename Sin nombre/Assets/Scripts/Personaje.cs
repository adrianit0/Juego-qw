using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour {

    public float velocidad = 1;
    public int maxPasos = 0;
    
    //GAMEMANAGER
    public GameManager manager;
    
    //LINE RENDERER
    LineRenderer line;
    List<Vector3> posiciones;
    
    //VALORES INTERNOS
    float distanciaPos = 0.25f;
    float distanciaFinal = 1.00f;

    float actualizacion = 0.1f;

    //ACCION
    public Accion accion;
    float tiempoActualAccion = 0f;
    public LineRenderer lineaAccion;

    bool puedeAndar = false;
    
    void Awake() {
        line = GetComponent<LineRenderer>();
        if(line == null) {
            line = gameObject.AddComponent<LineRenderer>();
        }
    }
    
	void Start () {
        InvokeRepeating("Actualizacion", actualizacion, actualizacion);
        puedeAndar = false;
	}

    void Update() {
        if(!puedeAndar || posiciones.Count <= 1)
            return;


        if (accion != null) {
            if(Vector3.Distance(transform.position, posiciones[posiciones.Count-1]) <= distanciaFinal) {
                if(tiempoActualAccion == 0) {
                    lineaAccion.gameObject.SetActive(true);
                }
                tiempoActualAccion += Time.deltaTime;
                PorcentajeAccion(tiempoActualAccion/accion.recursoAccion.tiempoTotal);

                if (tiempoActualAccion > accion.recursoAccion.tiempoTotal) {
                    manager.AñadirRecurso(accion.recursoAccion);

                    accion = null;
                    tiempoActualAccion = 0;
                    lineaAccion.gameObject.SetActive(false);
                }

            } else {
                transform.position += (-posiciones[0] + posiciones[1]).normalized * velocidad * Time.deltaTime;
            }
        }
        
    }

    void Actualizacion () {
        
        ActualizarLine();

	}

    public void SetPositions (Vector3[] pos) {
        posiciones = new List<Vector3>(pos);

        if (pos == null ||pos.Length==0) {
            return;
        }

        puedeAndar = true;

        ActualizarLine();
    }

    void ActualizarLine () {
        if(posiciones == null || posiciones.Count <= 1) {
            puedeAndar = false;
            return;
        }

        posiciones[0] = transform.position;

        if (Vector3.Distance (posiciones[0], posiciones[1])<= distanciaPos) {
            posiciones.RemoveAt(1);
        }

        line.numPositions = posiciones.Count;
        line.SetPositions(posiciones.ToArray());
    }

    public void PorcentajeAccion (float porc) {
        porc = Mathf.Clamp(porc, 0, 1);
        lineaAccion.SetPosition(1, new Vector3(porc-0.5f, 0, 0));
    }
}
