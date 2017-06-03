using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BarraLine : MonoBehaviour {


    public string sortingString;
    public int sortingOrder;

    LineRenderer line;
    
    void Awake () {
        line = GetComponent<LineRenderer>();
        
    }
#if UNITY_EDITOR

    void Update () {
        line.sortingLayerName = sortingString;
        line.sortingOrder = sortingOrder;
    }

#else

    void Start () {
        line.sortingLayerName = sortingString;
        line.sortingOrder = sortingOrder;
    }

#endif
}
