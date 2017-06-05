using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public void ActivarGameobject (GameObject obj) {
        obj.SetActive(true);
    }

    public void DesactivarGameObject(GameObject obj) {
        obj.SetActive(false);
    }

    public void ActivarDesactivar(GameObject obj) {
        obj.SetActive(!obj.activeSelf);
    }
}
