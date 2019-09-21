using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {

    public static Vector3 ClosestPointOnLine(Vector3 p, Vector3 a, Vector3 b) {
        Vector3 ab = b - a;
        Vector3 ap = p - a;
        float t01 = Mathf.Clamp01(Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab));
        Vector3 c = a + ab * t01;
        return c;
    }

    public static void GizmosDrawLineRange(Vector3 a, Vector3 b, float range) {
        Vector3 ab = a - b;
        Vector3 rn = RightNormalXZ(ab).normalized;
        Gizmos.DrawLine(a + rn * range, b + rn * range);
        Gizmos.DrawLine(a - rn * range, b - rn * range);
        Gizmos.DrawWireSphere(a, range);
        Gizmos.DrawWireSphere(b, range);
    }

    public static Vector3 RightNormalXZ(Vector3 v) {
        return new Vector3(-v.z, 0, v.x);
    }

    public static Matrix4x4 RotateY(float rad) {
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = m.m22 = Mathf.Cos(rad);
        m.m02 = Mathf.Sin(rad);
        m.m20 = -m.m02;
        return m;
    }
}
