using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BarraLine : MonoBehaviour {


    public string sortingString;
    public int sortingOrder;

    [Space (10)]
    public bool usarRenderValues = false;
    public int cantAumentado = 0;

    LineRenderer line;
    SpriteRenderer render;
    
    void Awake () {
        line = GetComponent<LineRenderer>();
        render = GetComponent<SpriteRenderer>();
    }
#if UNITY_EDITOR

    void Update () {
        Actualizar();
    }

#else

    void Start () {
        Actualizar();
    }

#endif

    void Actualizar () {
        if(line == null)
            return;


        if (usarRenderValues) {
            if(render == null)
                return;

            line.sortingLayerName = render.sortingLayerName;
            line.sortingOrder = render.sortingOrder+ cantAumentado;
        } else {
            line.sortingLayerName = sortingString;
            line.sortingOrder = sortingOrder;
        }
    }
}
