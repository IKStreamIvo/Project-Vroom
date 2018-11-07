using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinny : MonoBehaviour {

	public float spinnySpeed;

	void Update () {
		transform.rotation *= Quaternion.Euler(Vector3.up * spinnySpeed * Time.deltaTime);
	}
}
