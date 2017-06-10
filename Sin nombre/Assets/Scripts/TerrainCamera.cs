using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCamera : MonoBehaviour {

    public Transform otherCamera;
    public float velocidad = 0.5f;

    void Start () {
        transform.position = new Vector3(otherCamera.position.x, 10, otherCamera.position.y);
    }

	void Update () {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal!=0 || vertical !=0) {
            otherCamera.transform.position += new Vector3(horizontal, vertical, 0)*velocidad;
            transform.position = new Vector3(otherCamera.position.x, 10, otherCamera.position.y);
        }
	}
}
