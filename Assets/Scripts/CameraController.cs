using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private Transform CameraTarget = null;
    [SerializeField] private float CameraDistance = 5;
    [SerializeField] private Vector2 CameraSpeed = new Vector2(.3f, .1f);
    [SerializeField] private Vector2 MinMaxAngle = new Vector2(30, 60);
    [SerializeField] private float MouseSensitivity = 10;

    private Vector3 TargetPos;
    private Vector3 CameraRotation = Vector3.zero;

    void FixedUpdate () {
        TargetPos = CameraTarget.position;


        // rot
        CameraRotation += new Vector3(-Input.GetAxis("Mouse Y") * CameraSpeed.y,
                                      Input.GetAxis("Mouse X") * CameraSpeed.x) * Time.deltaTime * 60 * MouseSensitivity;

        CameraRotation += new Vector3(Input.GetAxis("RightStickY") * CameraSpeed.y,
                                      Input.GetAxis("RightStickX") * CameraSpeed.x) * Time.deltaTime * 60;

        CameraRotation.x = Mathf.Clamp(CameraRotation.x, MinMaxAngle.x, MinMaxAngle.y);

        transform.eulerAngles = CameraRotation;
        transform.position = TargetPos - transform.forward * CameraDistance;
	}
}
