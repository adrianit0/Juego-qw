using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCamera : MonoBehaviour {

    public Transform otherCamera;

    void Start () {
        transform.position = new Vector3(otherCamera.position.x, 10, otherCamera.position.y);
    }

	void Update () {

        transform.position = new Vector3(otherCamera.position.x, 10, otherCamera.position.y);
        
	}
}
