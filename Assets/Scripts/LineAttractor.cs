using UnityEngine;

public class LineAttractor : MonoBehaviour {
    public Transform P0;
    public Transform P1;
    public LineAttractorData Data;

#if DEBUG
    void OnDrawGizmos() {
        // line attractor gizmos
        Vector3 p0 = P0.position;
        Vector3 p1 = P1.position;

        // gizmos attractor
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(p0, .2f);
        Gizmos.DrawSphere(p1, .2f);
        Gizmos.DrawLine(p0, p1);
        Utils.GizmosDrawLineRange(p0, p1, Data.Range);
    }
#endif
}
