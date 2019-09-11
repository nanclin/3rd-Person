using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {

    [Header("REFs")]
    [SerializeField] private Camera Camera = null;
    [SerializeField] private CharacterController CharacterController = null;
    [Header("SETTINGS")]
    [SerializeField] private float WalkSpeed = 5;
    [SerializeField] private float CrouchSpeed = 2;
    [SerializeField] private float SpeedSmoothTime = .2f;
    [SerializeField] private float TurnSmoothTime = .1f;
    [SerializeField] private float PushPower = 1.0f;
    [SerializeField] private Vector3 CrouchScale = new Vector3(1.2f,0.6f,1.2f);
    [SerializeField] private float CrouchSmoothTime = .3f;
    [SerializeField] private AnimationCurve AnimationCurve;
    [Header("DEBUG")]
    [SerializeField] private int HistoryCount = 10;

    private List<Vector3> PositionHistory = new List<Vector3>();
    private List<Vector3> MoveVectorHistory = new List<Vector3>();

    private float CurrentSpeed;
    private float SpeedSmoothVelocity;

    private float CurrentRotation;
    private float TurnSmoothVelocity;

    private Vector3 PrevDir;
    private Vector3 InputDir;

    private bool Crouching;
    private Vector3 CrouchSmoothVeolocity;

    private Vector4 ClosestPoint;

    #if DEBUG
    private Vector3 DEBUG_WalkDir = Vector3.zero;
    private Vector3 DEBUG_LastWalkDir = Vector3.zero;
#endif

	void FixedUpdate()
    {

        #region Movement

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 dirRelToCamera = (RotateY(Camera.transform.eulerAngles.y * Mathf.Deg2Rad) * input).normalized;

        // rotate
        if (input != Vector3.zero) {
            float targetRotation = Mathf.Atan2(dirRelToCamera.x, dirRelToCamera.z) * Mathf.Rad2Deg;
            CurrentRotation = Mathf.SmoothDampAngle(CurrentRotation, targetRotation, ref TurnSmoothVelocity, TurnSmoothTime);
            transform.eulerAngles = Vector3.up * CurrentRotation;
        }


        // move
        float targetSpeed = (Crouching ? CrouchSpeed : WalkSpeed) * input.magnitude;
        CurrentSpeed = Mathf.SmoothDamp(CurrentSpeed, targetSpeed, ref SpeedSmoothVelocity, SpeedSmoothTime);
        CharacterController.SimpleMove(dirRelToCamera * CurrentSpeed * Time.deltaTime * 60);

#if DEBUG
        DEBUG_LastWalkDir = DEBUG_WalkDir.magnitude >.1f ? DEBUG_WalkDir : DEBUG_LastWalkDir;
        DEBUG_WalkDir = dirRelToCamera;
#endif
        // reset pos on fall
        if (transform.position.y < -1)
        {
            transform.position = Vector3.zero;
            CurrentSpeed = 0;
        }

        #endregion

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
        bool crouchButton = Input.GetKey("joystick button 4") || Input.GetKey("joystick button 5") || Input.GetKey("left shift");
        Vector3 targetCrouchScale = transform.localScale;
        if (crouchButton){
            targetCrouchScale = CrouchScale;
            Crouching = true;
        } else if (!crouchButton && hasStandingRoom){
            targetCrouchScale = Vector3.one;
            Crouching = false;
        }
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetCrouchScale, ref CrouchSmoothVeolocity, CrouchSmoothTime);

        // align

#if UNITY_EDITOR
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
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3)
        {
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

	private void OnDrawGizmos()
	{
        for (int i = 0; i < MoveVectorHistory.Count; i++)
        {
            Color color = Color.white;
            color.a = Mathf.Lerp(0.1f, 1, (float)i / (float)MoveVectorHistory.Count);
            Gizmos.color = color;
            Gizmos.DrawSphere(PositionHistory[i], .05f);
            //Gizmos.DrawRay(PositionHistory[i], MoveVectorHistory[i]);
        }
    }

    public static Matrix4x4 RotateY(float rad) {
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = m.m22 = Mathf.Cos(rad);
        m.m02 = Mathf.Sin(rad);
        m.m20 = -m.m02;
        return m;
    }
}
