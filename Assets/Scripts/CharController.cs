using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {

    [Header("REFs")]
    [SerializeField] private Transform LevelStart;
    [SerializeField] private Camera Camera = null;
    [SerializeField] private CharacterController CharacterController = null;
    [Header("SETTINGS")]
    [SerializeField] private float WalkSpeed = 5;
    [SerializeField] private float CrouchSpeed = 2;
    [SerializeField] private float SpeedSmoothTime = .2f;
    [SerializeField] private float TurnSmoothTime = .1f;
    [SerializeField] private float PushPower = 1.0f;
    [SerializeField] private Vector3 CrouchScale = new Vector3(1.2f, 0.6f, 1.2f);
    [SerializeField] private float CrouchSmoothTime = .3f;
    [Header("DEBUG")]
    [SerializeField] private int HistoryCount = 10;

    [Header("ATTRACTOR")]
    [SerializeField] private bool UseAttractor = true;
    [SerializeField] private Transform P0 = null;
    [SerializeField] private Transform P1 = null;
    [SerializeField] private AnimationCurve AttractorCurve;
    [SerializeField] [Range(0, 5)] private float AttractorRange = 2;
    [SerializeField] [Range(0, 1)] private float AttractorPower = .5f;

    private List<Vector3> PositionHistory = new List<Vector3>();
    private List<Vector3> MoveVectorHistory = new List<Vector3>();

    private float CurrentSpeed;
    private float SpeedSmoothVelocity;

    private float CurrentRotation;
    private float TurnSmoothVelocity;

    private Vector3 Input;
    private Vector3 InputDirRelToCamera;

    private bool Crouching;
    private Vector3 CrouchSmoothVeolocity;


#if DEBUG
    private Vector3 DEBUG_WalkDir = Vector3.zero;
    private Vector3 DEBUG_LastWalkDir = Vector3.zero;
#endif

    void FixedUpdate() {
        // input
        Input = new Vector3(UnityEngine.Input.GetAxis("Horizontal"), 0, UnityEngine.Input.GetAxis("Vertical"));
        InputDirRelToCamera = (Utils.RotateY(Camera.transform.eulerAngles.y * Mathf.Deg2Rad) * Input).normalized;

        // move dir
        Vector3 moveDir = InputDirRelToCamera;
        if (UseAttractor)
            moveDir = AttractorDir(InputDirRelToCamera, P0.position, P1.position, AttractorRange, AttractorCurve);

        // rotate
        if (Input != Vector3.zero) {
            float targetRotation = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            CurrentRotation = Mathf.SmoothDampAngle(CurrentRotation, targetRotation, ref TurnSmoothVelocity, TurnSmoothTime);
            transform.eulerAngles = Vector3.up * CurrentRotation;
        }


        // move
        float targetSpeed = (Crouching ? CrouchSpeed : WalkSpeed) * Input.magnitude;
        CurrentSpeed = Mathf.SmoothDamp(CurrentSpeed, targetSpeed, ref SpeedSmoothVelocity, SpeedSmoothTime);
        CharacterController.SimpleMove(moveDir * CurrentSpeed * Time.deltaTime * 60);

#if DEBUG
        DEBUG_LastWalkDir = DEBUG_WalkDir.magnitude > .1f ? DEBUG_WalkDir : DEBUG_LastWalkDir;
        DEBUG_WalkDir = moveDir;
#endif
        // reset pos on fall
        if (transform.position.y < -2) {
            transform.position = LevelStart.position;
            CurrentSpeed = 0;
        }

        int layerMask = ~(1 << LayerMask.NameToLayer("Character"));
        RaycastHit hit;
        bool hasStandingRoom = true;
        float maxDist = 2;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, Vector3.up, out hit, maxDist, layerMask)) {
            hasStandingRoom = false;
        }

#if DEBUG && UNITY_EDITOR
        float rayDist = hasStandingRoom ? maxDist : hit.distance;
        Color rayColor = hasStandingRoom ? Color.yellow : Color.red;
        Debug.DrawRay(transform.position, Vector3.up * rayDist, rayColor);
#endif

        // crouch
        bool crouchButton = UnityEngine.Input.GetKey("joystick button 4") || UnityEngine.Input.GetKey("joystick button 5") || UnityEngine.Input.GetKey("left shift");
        Vector3 targetCrouchScale = transform.localScale;
        if (crouchButton) {
            targetCrouchScale = CrouchScale;
            Crouching = true;
        }
        else if (!crouchButton && hasStandingRoom) {
            targetCrouchScale = Vector3.one;
            Crouching = false;
        }
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetCrouchScale, ref CrouchSmoothVeolocity, CrouchSmoothTime);

#if DEBUG && UNITY_EDITOR
        // store debug data
        PositionHistory.Add(transform.position);
        if (PositionHistory.Count > HistoryCount)
            PositionHistory.RemoveAt(0);

        MoveVectorHistory.Add(transform.forward);
        if (MoveVectorHistory.Count > HistoryCount)
            MoveVectorHistory.RemoveAt(0);
#endif

    }

    // this script pushes all rigidbodies that the character touches
    void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic) {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3) {
            return;
        }

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * PushPower;
    }

    private Vector3 AttractorDir(Vector3 dir, Vector3 p0, Vector3 p1, float distance, AnimationCurve curve = null) {
        Vector3 cp = Utils.ClosestPointOnLine(transform.position, p0, p1);
        Vector3 lineDir = (p1 - p0).normalized;
        int lineAlignment = Vector3.Dot(lineDir, dir) > 0 ? 1 : -1;
        if (dir == Vector3.zero)
            lineAlignment = 0;
        Vector3 attractorDir = lineDir * lineAlignment;
        float d01 = Mathf.Clamp01(Mathf.InverseLerp(distance, 0, (transform.position - cp).magnitude)); // closer to the line, higher the value
        if (curve != null)
            d01 = curve.Evaluate(d01);
        Vector3 lerpDir = Vector3.Lerp(dir, attractorDir, d01 * AttractorPower);
        return lerpDir;
    }

#if DEBUG
    private void OnDrawGizmos() {

        // positions history
        for (int i = 0; i < MoveVectorHistory.Count; i++) {
            Color color = Color.white;
            color.a = Mathf.Lerp(0.1f, 1, (float)i / (float)MoveVectorHistory.Count);
            Gizmos.color = color;
            Gizmos.DrawSphere(PositionHistory[i], .05f);
            //Gizmos.DrawRay(PositionHistory[i], MoveVectorHistory[i]);
        }

        // line attractor gizmos
        if (UseAttractor) {
            Vector3 cp = Utils.ClosestPointOnLine(transform.position, P0.position, P1.position);

            // gizmos attractor
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(P0.position, .2f);
            Gizmos.DrawSphere(P1.position, .2f);
            Gizmos.DrawLine(P0.position, P1.position);
            Gizmos.DrawSphere(cp, .2f);
            Gizmos.DrawLine(transform.position, cp);
            Utils.GizmosDrawLineRange(P0.position, P1.position, AttractorRange);

            // temp values
            Vector3 inputDir = InputDirRelToCamera;
            Vector3 lineDir = (P1.position - P0.position).normalized;
            Vector3 attractedDir = (lineDir + inputDir).normalized;
            Vector3 lerpDir = AttractorDir(inputDir, P0.position, P1.position, AttractorRange, AttractorCurve);

            // gizmos dirs
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, inputDir * 2);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, InputDirRelToCamera * 2);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, lineDir * 2);
            Gizmos.color = Color.green * .75f;
            Gizmos.DrawRay(transform.position, attractedDir);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, lerpDir);
        }
    }
#endif
}
