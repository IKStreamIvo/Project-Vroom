using System;
using System.Collections;
using System.Collections.Generic;
using PickupComponent;
using UnityEngine;

public class CarController : MonoBehaviour {

	public int player;
	public CarStats carStats;
	public bool _drive;
	public float _speed;
	public float _maxSpeedCopy;

	public List<GameObject> wheelsL = new List<GameObject>();
	public List<GameObject> wheelsR = new List<GameObject>();

	public Vector3 spawnPos;

	public Rigidbody rb;
	public new Collider collider;
	public CharacterController charCont;
    public float rotateSmoothSpeed = 10f;
    public float distToGround = 1f;
    public float carGrav = 10f;
    public float smallAntigrav = 50f;

    void Start () {
		rb = GetComponent<Rigidbody>();
		charCont = GetComponent<CharacterController>();
		_maxSpeedCopy = carStats.maxSpeed;
	}
	
	void FixedUpdate () {
		if (!_drive) //prevents car from driving
			return;

		IvoDriving();
		/*transform.position += Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, 0f) * Vector3.forward * _speed * Time.deltaTime;
		
		if (Input.GetButton("[Car] Gas " + player)) {
			if (_speed <= carStats.maxSpeed) {
				_speed += carStats.acceleration * Time.deltaTime;
				RotateWheels(true, true);
			}
		}else if (Input.GetButton("[Car] Break " + player)) {
			if (_speed > 0) {
				_speed -= carStats.breakSpeed * Time.deltaTime;
			} else {
				_speed -= carStats.reverseSpeed * Time.deltaTime;
			}
		} else {
			if(_speed > 0.1) {
				_speed -= carStats.breakSpeed * Time.deltaTime;
			} else {
				_speed = 0;
			}
		}

		//TURNING
		if (Input.GetAxis("[Car] Steer X " + player) > 0.5f) {	
			transform.Rotate(Vector3.up * carStats.steerSpeed * Time.deltaTime);
			RotateWheels(true, false);
		} else if (Input.GetAxis("[Car] Steer X " + player) < -0.5f) {
			transform.Rotate(Vector3.down * carStats.steerSpeed * Time.deltaTime);
			RotateWheels(false, true);
		}*/

		//RESET CAR
		if(Input.GetButtonDown("[Car] Reset " + player)) {
			ResetPosition(false);
		}
	
	}

    private void IvoDriving(){
		Debug.DrawRay(transform.position, -Vector3.up * distToGround, Color.red, 1f);
		int layerMask = 1 << 9;
        layerMask = ~layerMask;
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround, layerMask)){
			rb.AddForce(Vector3.down * Time.fixedDeltaTime * carGrav);
			Debug.Log("off ground");
			return;
		}

		if(Input.GetButton("[Car] Gas " + player)){ //accelerate
			Vector3 force = new Vector3(0f, 0f, carStats.acceleration) * Time.fixedDeltaTime;
			rb.AddRelativeForce(force, ForceMode.Acceleration);
			rb.AddForce(new Vector3(0f, smallAntigrav, 0f) * Time.fixedDeltaTime);
		}else if(Input.GetButton("[Car] Break " + player)){ //reversing
			Vector3 force = new Vector3(0f, 0f, -carStats.acceleration / 1.5f * Time.fixedDeltaTime);
			rb.AddRelativeForce(force, ForceMode.Acceleration);
		}else{ //deaccelerate
			rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0f, rb.velocity.y, 0f), rotateSmoothSpeed * Time.fixedDeltaTime);
		}

		//smooth x and z rotation against flipping
		/*rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, new Vector3(0f, rb.angularVelocity.y, 0f), rotateSmoothSpeed * Time.fixedDeltaTime);
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f), rotateSmoothSpeed * Time.fixedDeltaTime);
		*/
		//Debug.Log(Input.GetAxis("[Car] Steer X " + player));
		if (Input.GetAxis("[Car] Steer X " + player) > 0.5f) {	
			//Debug.Log("roate");
			transform.Rotate(Vector3.up * carStats.steerSpeed * Time.fixedDeltaTime);
			//rb.AddTorque(0f, carStats.steerSpeed * Time.fixedDeltaTime, 0f, ForceMode.Acceleration);
			RotateWheels(true, false);
		} else if (Input.GetAxis("[Car] Steer X " + player) < -0.5f) {
			transform.Rotate(Vector3.down * carStats.steerSpeed * Time.fixedDeltaTime);
			//rb.AddTorque(0f, -carStats.steerSpeed * Time.fixedDeltaTime, 0f, ForceMode.Acceleration);
			RotateWheels(false, true);
		}

		
    }

    IEnumerator ResetRotation() {
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		yield return new WaitForSeconds(2);
		rb.constraints = RigidbodyConstraints.None;
	}

	public void ResetPosition(bool totalReset) {
		StartCoroutine(ResetRotation());
		if (totalReset) {
			transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(Vector3.zero));
		} else {
			transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, 0f));
		}
	}
	
	private void RotateWheels(bool left, bool right) {
		bool forward = (_speed > 0) ? true : false;

		if (right) {
			foreach (GameObject wheel in wheelsR) {
				if(forward)
					wheel.transform.Rotate(Quaternion.Euler(0f, transform.localRotation.eulerAngles.z, 0f) * Vector3.back * _speed * Time.deltaTime * 3);
				else
					wheel.transform.Rotate(Quaternion.Euler(0f, transform.localRotation.eulerAngles.z, 0f) * Vector3.forward * _speed * Time.deltaTime * 3);
			}
		}

		if (left) {
			foreach (GameObject wheel in wheelsL) {
				if (forward)
					wheel.transform.Rotate(Quaternion.Euler(0f, transform.localRotation.eulerAngles.z, 0f) * Vector3.forward * _speed * Time.deltaTime * 3);
				else
					wheel.transform.Rotate(Quaternion.Euler(0f, transform.localRotation.eulerAngles.z, 0f) * Vector3.back * _speed * Time.deltaTime * 3);
			}
		}
	}

	public void PickUp(PickUpType pickUpType, GameObject thingIHit) {
		if (pickUpType == PickUpType.SpawnPoint) {
			spawnPos = collider.transform.position + Vector3.up * 4;
			return;
		}
		Debug.Log(thingIHit.name);
		//considering that there's anything else but spawnpoints.
		switch (pickUpType) {
			case PickUpType.BuildPoint:
				BuildController.ModifyBuildPoints(thingIHit.GetComponent<Pickup>().buildPointsToEarn, player);
				break;
		}

		PickUpDestroy(pickUpType);
	}

	public bool PickUpDestroy(PickUpType pickUpType) {
		if (pickUpType == PickUpType.SpawnPoint) {
			return false;
		}

		return true;
	}
}
