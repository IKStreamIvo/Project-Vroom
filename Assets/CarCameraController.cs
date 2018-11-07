using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCameraController : MonoBehaviour {
	public Transform target;
    public float followSpeed = 5f;
    public float rotateSpeed = 5f;

    void FixedUpdate () {
		if(target == null) return;
		
		transform.position = Vector3.Lerp(transform.position, target.position, followSpeed * Time.fixedDeltaTime);
		Vector3 targetRot = target.rotation.eulerAngles;
		targetRot.x = 0f;
		targetRot.z = 0f;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRot), rotateSpeed * Time.fixedDeltaTime);
	}
}
