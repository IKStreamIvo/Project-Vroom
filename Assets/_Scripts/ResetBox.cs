using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBox : MonoBehaviour {

	private void OnTriggerEnter(Collider other) {
		other.GetComponent<CarController>().ResetPosition(true);
	}
}
