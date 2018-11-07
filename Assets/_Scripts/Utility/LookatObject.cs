using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookatObject : MonoBehaviour {

	public Transform target;

	public void Look(){
		if(target != null){
			transform.LookAt(target, Vector3.up);
		}
	}
}
