using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Personaje : MonoBehaviour {

    public float velocidad = 1;
    public int maxPasos = 0;

    List<Vector3> posiciones;
    LineRenderer line;

    float dif = 0.25f;

    float actualizacion = 0.1f;

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

        transform.position += (- posiciones[0] + posiciones[1]).normalized * velocidad * Time.deltaTime;
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

        if (Vector3.Distance (posiciones[0], posiciones[1])<= dif) {
            posiciones.RemoveAt(1);
        }

        line.numPositions = posiciones.Count;
        line.SetPositions(posiciones.ToArray());
    }
}
