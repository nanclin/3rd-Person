using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour {

    [SerializeField] private bool ShowGizmos = true;
    [SerializeField] private Transform RefPoint = null;
    [SerializeField] private List<Transform> Points = null;
    [SerializeField] [Range(3, 20)] private int SampleCount = 5;

    [SerializeField] private List<Vector3> SamplePoints = new List<Vector3>();

    Vector3 ClosestPoint;
    Vector3 sA;
    Vector3 sB;
    int ClosestIndex;

    public List<Vector3> GetClosestPoint() {
        return new List<Vector3>(){
            ClosestPoint,
            sA,
            sB
        };
    }

    public List<Vector3> GetSamplePoints{ get { return SamplePoints; }}

	private void Update() {
        SamplePoints = new List<Vector3>();
        for (int i = 0; i < SampleCount; i++)
        {
            float t = (float)i / (float)(SampleCount-1);
            SamplePoints.Add(PointOnBezier(t));
        }

        Vector3 P = RefPoint.position;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < SamplePoints.Count-1; i++)
        {
            Vector3 A = SamplePoints[i];
            Vector3 B = SamplePoints[i+1];

            Vector3 AB = B - A;
            Vector3 AP = P - A;
            float t01 = Mathf.Clamp01(Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB));
            Vector3 C = A + t01 * AB;
            float d = (P - C).magnitude;
            if (d < closestDistance){
                closestDistance = d;
                ClosestPoint = C;
                ClosestIndex = i;
                sA = A;
                sB = B;
            }
        }
	}

	private void OnDrawGizmos()
	{
        if (Points.Count < 4) return;

        // handles
        for (int i = 0; i < Points.Count - 3; i+=3) {
            Vector3 p0 = Points[i].position;
            Vector3 p1 = Points[i + 1].position;
            Vector3 p2 = Points[i + 2].position;
            Vector3 p3 = Points[i + 3].position;

            if (ShowGizmos) {
                Gizmos.color = Color.black * .75f;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p2, p3);
            }

            Gizmos.color = Color.black;
            Vector3 prev = p0;
            for (float j = 1; j <= 10; j++)
            {
                Vector3 p = BezierSegmentQuad(p0, p1, p2, p3, j / 10);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        if (!ShowGizmos) return;

        // draw end points
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Points[0].position, .2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Points[Points.Count-1].position, .2f);



        // closest point
        Gizmos.color = Color.yellow;
        for (int i = 0; i < SamplePoints.Count; i++)
        {
            Gizmos.DrawSphere(SamplePoints[i], .1f);
            if (i < SamplePoints.Count-1)
                Gizmos.DrawLine(SamplePoints[i], SamplePoints[i + 1]);
        }
        Gizmos.DrawSphere(ClosestPoint, .1f);
    }

    public Vector3 PointOnBezier(float t) {

        Debug.AssertFormat(Points.Count >= 4, "Not enough points, need at least 4! Currently {0} points.", Points.Count);

        // count segments
        int count = Mathf.FloorToInt(Points.Count / 3.0f);

        float s = t * count;
        int segment = Mathf.FloorToInt(s);
        if (segment >= count) segment = count - 1;
        int i = segment * 3;
        float ti = s - segment;

        Vector3 p0 = Points[i].position;
        Vector3 p1 = Points[i + 1].position;
        Vector3 p2 = Points[i + 2].position;
        Vector3 p3 = Points[i + 3].position;

        return BezierSegmentQuad(p0, p1, p2, p3, ti);
    }


    private Vector3 BezierSegmentQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 pA = BezierSegment(p0, p1, p2, t);
        Vector3 pB = BezierSegment(p1, p2, p3, t);
        Vector3 p = Vector3.Lerp(pA, pB, t);
        return p;
    }

    private Vector3 BezierSegment(Vector3 p0, Vector3 p1, Vector3 p2, float t) {

        //Vector3 p0 = Points[i - 1].position;
        //Vector3 p1 = Points[i].position;
        //Vector3 p2 = Points[i + 1].position;
        Vector3 p01 = Vector3.Lerp(p0, p1, t);
        Vector3 p12 = Vector3.Lerp(p1, p2, t);
        Vector3 p0112 = Vector3.Lerp(p01, p12, t);
        //Vector3 p = Vector3.zero;
        return p0112;
    }
}
