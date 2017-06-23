using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resultado del PathFinding.
/// </summary>
public class PathResult {
    IntVector2 finalPosition;

    public IntVector2[] path;

    public PathResult () {
        finalPosition = new IntVector2(0, 0);

        path = null;
    }

    public PathResult(IntVector2 finalPosition, IntVector2[] path) {
        this.finalPosition = finalPosition;

        this.path = path;
    }

    //Devuelve el final del trayecto.
    public IntVector2 GetFinalPosition () {
        return finalPosition;
    }
}