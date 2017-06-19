using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector2 {

    public int x;
    public int y;

    public IntVector2(int x = 0, int y = 0) {
        this.x = x;
        this.y = y;
    }

    public static IntVector2 operator + (IntVector2 c1, IntVector2 c2) {
        return new IntVector2(c1.x + c2.x, c1.x + c2.x);
    }

    public static IntVector2 operator - (IntVector2 c1, IntVector2 c2) {
        return new IntVector2(c1.x - c2.x, c1.x - c2.x);
    }

    public static IntVector2 operator * (IntVector2 c1, IntVector2 c2) {
        return new IntVector2(c1.x * c2.x, c1.x * c2.x);
    }

    public static IntVector2 operator / (IntVector2 c1, IntVector2 c2) {
        return new IntVector2(c1.x / c2.x, c1.x / c2.x);
    }

    public static bool operator == (IntVector2 c1, IntVector2 c2) {
        return c1.x == c2.x && c1.y == c2.y;
    }

    public static bool operator != (IntVector2 c1, IntVector2 c2) {
        return c1.x != c2.x || c1.y != c2.y;
    }

    /*public override bool Equals(Object o) {
        return x == v2.x && y == v2.y;
    }*/

    public override int GetHashCode() {
        return x.GetHashCode() + y.GetHashCode();
    }

    static public implicit operator IntVector2(Vector2 vector) {
        return new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
    }

    static public implicit operator IntVector2(Vector3 vector) {
        return new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
    }

    static public explicit operator Vector2(IntVector2 intVector) {
        return new Vector2(intVector.x, intVector.y);
    }

    static public explicit operator Vector3(IntVector2 intVector) {
        return new Vector3(intVector.x, intVector.y, 0);
    }
}