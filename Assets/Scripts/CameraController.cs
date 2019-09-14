using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private Camera Camera;
    [SerializeField] private Transform CharTransform;
    [SerializeField] private Transform CameraTarget = null;
    [Header("Settings")]
    [SerializeField] private Vector2 CameraSpeed = new Vector2(.3f, .1f);
    [SerializeField] private float MouseSensitivity = 10;
    [Header("Rails")]
    [SerializeField] private List<Bezier> Rails;
    [SerializeField] private bool UseRails = false;
    [SerializeField] private bool RailOrTunnel = true;
    [SerializeField] private AnimationCurve RailTransitionCurve;
    [SerializeField] private float RailFadeRadius = 5;
    [SerializeField] private float RailFadeBuffer = 1;
    [SerializeField] private float CameraDistance = 5;
    [SerializeField] private float RailCameraDistance = 5;
    [SerializeField] private float FOV = 30;
    [SerializeField] private float TunnelFOV = 90;
    [SerializeField] private Vector2 MinMaxAngle = new Vector2(30, 60);
    [SerializeField] private Vector2 TunnelMinMaxAngle = new Vector2(-20, 10);
    [SerializeField] private float RailFadeTransitionDuration = 0.5f;

    private Vector3 TargetPos;
    private Vector3 FreeCamRotation = Vector3.zero;
    private float ClosestDistance;
    private BezierPoint ClosestPoint;
    private float RailFadeT = 0;

    private void Start() {
        foreach (var rail in Rails) {
            rail.BakeBezier();
        }
    }

    void Update() {
        TargetPos = CameraTarget.position;

        // free camera position calculation
        Ray freeCamRay = FreeCam();
        Vector3 freeCamPos = freeCamRay.GetPoint(0);
        Quaternion freeCamRot = Quaternion.LookRotation(freeCamRay.direction);

        // rail camera position calculation
        Ray railCamRay = RailOrTunnel ? RailCam() : TunnelCam();
        Vector3 railCamPos = railCamRay.GetPoint(0);
        Quaternion railCamRot = Quaternion.LookRotation(railCamRay.direction);


        // apply values
        float t01 = GetT01TimeSwitchBased();
        Vector3 finalPos = Vector3.Lerp(freeCamPos, railCamPos, UseRails ? RailTransitionCurve.Evaluate(t01) : 0);
        Quaternion finalRot = Quaternion.Lerp(freeCamRot, railCamRot, UseRails ? t01 : 0);
        transform.rotation = finalRot;
        transform.position = finalPos;
    }

    private float GetT01PositionBased() {
        return Mathf.InverseLerp(RailFadeRadius + RailFadeBuffer, RailFadeRadius, ClosestDistance);
    }

    private float GetT01TimeSwitchBased() {
        bool closerToRail = ClosestDistance < RailFadeRadius;
        RailFadeT += Time.deltaTime * (closerToRail ? 1.0f : -1.0f);
        RailFadeT = Mathf.Clamp(RailFadeT, 0.0f, RailFadeTransitionDuration);
        float t01 = RailFadeT / RailFadeTransitionDuration;
        if (t01 <= 0) {
            return 0;
        }
        if (t01 > 1) {
            return 1;
        }
        return t01;
    }

    private Ray FreeCam() {

        // rot
        FreeCamRotation += new Vector3(-Input.GetAxis("Mouse Y") * CameraSpeed.y,
                                      Input.GetAxis("Mouse X") * CameraSpeed.x) * Time.deltaTime * 60 * MouseSensitivity;
        FreeCamRotation += new Vector3(Input.GetAxis("RightStickY") * CameraSpeed.y,
                                      Input.GetAxis("RightStickX") * CameraSpeed.x) * Time.deltaTime * 60;
        Vector2 currentMinMaxAngle = Vector2.Lerp(MinMaxAngle, TunnelMinMaxAngle, RailFadeT / RailFadeTransitionDuration);
        FreeCamRotation.x = Mathf.Clamp(FreeCamRotation.x, currentMinMaxAngle.x, currentMinMaxAngle.y);
        transform.eulerAngles = FreeCamRotation;

        // FOV fade
        Camera.fieldOfView = Mathf.Lerp(FOV, TunnelFOV, RailFadeT / RailFadeTransitionDuration);


        // free camera position calculation
        Vector3 pos = TargetPos - transform.forward * CameraDistance;
        Vector3 dir = transform.forward;
        return new Ray(pos, dir);
    }

    private void CalculateClosestRailDistance() {
        // get closest point
        ClosestDistance = Mathf.Infinity;
        Bezier closestRail;
        foreach (Bezier rail in Rails) {
            BezierPoint cp = rail.GetClosestPoint(TargetPos);
            float d = Vector3.Distance(cp.Position, TargetPos);
            if (d < ClosestDistance) {
                ClosestDistance = d;
                closestRail = rail;
                ClosestPoint = cp;
            }
        }
    }

    private Ray RailCam() {
        CalculateClosestRailDistance();

        // rail alignment
        Vector3 segmentDir = (ClosestPoint.SegmentP1 - ClosestPoint.SegmentP0).normalized;
        int segmentAlligned = 1;// Vector3.Dot(CharTransform.forward, railSegmentDir) > 0 ? 1 : -1;

        Debug.Log(string.Format("ClosestDistance:{0}, inverseLerp:{1}, RailFadeT:{2}", ClosestDistance, Mathf.InverseLerp(5, 2, ClosestDistance), RailFadeT));
        Vector3 pos = ClosestPoint.Position - segmentDir * RailCameraDistance * segmentAlligned;
        Vector3 dir = segmentDir * segmentAlligned;
        return new Ray(pos, dir);
    }

    private Ray TunnelCam() {
        CalculateClosestRailDistance();

        // rail alignment
        Vector3 pos = TargetPos - transform.forward * RailCameraDistance;
        Vector3 dir = transform.forward;
        return new Ray(pos, dir);
    }

    private void OnDrawGizmos() {
        if (Application.isPlaying) {
            Vector3 railSegmentDir = (ClosestPoint.SegmentP1 - ClosestPoint.SegmentP0).normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(TargetPos, railSegmentDir);

            // draw fade radius
            Color color = Color.magenta;
            foreach (var rail in Rails) {
                foreach (Vector3 p in new List<Vector3>(){
                rail.GetPoints[0].position,
                rail.GetPoints[rail.GetPoints.Count-1].position }
                ) {
                    Gizmos.color = color;
                    Gizmos.DrawWireSphere(p, RailFadeRadius);
                    Gizmos.color = color * .75f;
                    Gizmos.DrawWireSphere(p, RailFadeRadius + RailFadeBuffer);
                }
            }
        }
    }
}
