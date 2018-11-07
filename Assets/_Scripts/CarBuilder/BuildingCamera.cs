using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCamera : MonoBehaviour {
	[Header("Player Settings")]
	public int _player;

	[Header("Rotation")]
    public float rotXSpeed = 2.4f;
    public float rotYSpeed = 2.4f;
    public Vector2 rotLimits = new Vector2(-20f, 80f);
    public float rotLerpSpeed = 2f;
    float x = 0.0f;
    float y = 0.0f;

    [Header("Zooming")]
    public float zoomSpeed = 1f;
    public Vector2 zoomLimits = new Vector2(1, 10);

	void Start () {
		Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
	}
	
	void FixedUpdate () {
		//Rotation
		float rotX = Input.GetAxis("[Build] Cam Rotate X " + _player);
		float rotY = Input.GetAxis("[Build] Cam Rotate Y " + _player);
		x += rotX * rotXSpeed;
		y -= rotY * rotYSpeed;
		y = ClampAngle(y, rotLimits.x, rotLimits.y);
		transform.rotation = Quaternion.Euler(y, x, 0);
	}

	public static float ClampAngle(float angle, float min, float max){
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
