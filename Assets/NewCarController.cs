using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PickupComponent;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewCarController : MonoBehaviour {
	public int player = 1;

	private float horInput;
	private float verInput;
	private float steeringAngle;

	public Rigidbody rb;
	public ParticleSystem[] particleSystems;
	public WheelCollider[] wheelColliders;
	public Transform[] wheelTransforms;
	public CarStats carStats;
	public float maxSteerAngle = 45f; //how fast we can turn
	public float steerForce = 10f;
	public float motorForce = 4000f;
    public float driveForce = 2000f;
    public float stopForce = 2500f;
    public float distToGround = 3f;

	public int round;
	public int checkPoint;
	public bool finished;
	public Text roundsText;

	public TerrainType.TerrainTypePicker TerrainType;

	public Vector3 spawnPos;
    public float rotateFightingSpeed = 1f;

    private void Start() {
		roundsText = BuildController.inst.roundsTexts[player - 1];
		roundsText.text = "Rounds: " + round + " / " + (BuildController.inst.roundsToDrive + 1);
	}

	private void FixedUpdate() {
		GetInput();
		Steer();

		int layerMask = 1 << 9;
        layerMask = ~layerMask;
		RaycastHit hit;
		Debug.DrawLine(transform.position, transform.position + (-Vector3.up * distToGround));
		if (!Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround, layerMask)){
			//rb.AddForce(Vector3.down * Time.fixedDeltaTime * carGrav);
			return;
		} else if(Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround, layerMask)) {
			TerrainType tempTerrain = hit.transform.GetComponent<TerrainType>();
			if(tempTerrain != null) {
				if(TerrainType != tempTerrain.terrainTypePicker) {
					tempTerrain.ChangeTerrainType(gameObject);	
				}
			}
		}
		if (!finished) {
			if (verInput != 0f) Accelerate();
			else Deaccelerate();
			UpdateWheelPoses();
		}
		if(particleSystems.Length > 0){
			if(verInput > .1f){
				if(particleSystems[0].isStopped){
					for (int i = 0; i < particleSystems.Length; i++){
						particleSystems[i].Play();
					}
				}
			}else{
				if(particleSystems[0].isPlaying){
					for (int i = 0; i < particleSystems.Length; i++){
						particleSystems[i].Stop();
					}
				}
			}
		}
	}

    public void GetInput(){
		horInput = Input.GetAxis("[Car] Steer X " + player);
		if(Input.GetButton("[Car] Gas " + player)){
			verInput = 1f;
		}else if(Input.GetButton("[Car] Break " + player)){
			verInput = -1;
		}else{
			verInput = 0f;
		}

		if (Input.GetButtonDown("[Car] Reset " + player)) {
			ResetPosition(false);
		}
	}

	public void Steer(){
		steeringAngle = carStats.steerSpeed * horInput;
		for (int i = 0; i < 2; i++){
			Transform wtransform = wheelTransforms[i];
			WheelCollider wcollider = wheelColliders[i];
			wcollider.steerAngle = steeringAngle;
			Vector3 currRot = wtransform.localRotation.eulerAngles;
			wtransform.localRotation = Quaternion.Euler(currRot.x, wcollider.steerAngle, currRot.z);
		}

		if(steeringAngle == 0f){
			Vector3 target = rb.angularVelocity;
			target.y = 0f;
			rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, target, rotateFightingSpeed * Time.deltaTime);
		}
	}

	public void Accelerate(){
		float speed;
		if(carStats.terrainMultiplier != 1f){//any offroad
			speed = verInput * (motorForce * (carStats.terrainMultiplier * carStats.offroad));
		}else{//normal
			speed = verInput * motorForce;
		}
		//Debug.Log((carStats.terrainMultiplier * carStats.offroad));
		rb.AddRelativeForce(Quaternion.Euler(0f, steeringAngle, 0f) * new Vector3(0f, 0f, Mathf.Sign(verInput) * carStats.acceleration * carStats.terrainMultiplier * Time.fixedDeltaTime), ForceMode.Acceleration);
		foreach (WheelCollider wheel in wheelColliders){
			wheel.motorTorque = speed;
		}
	}

    private void Deaccelerate(){
        foreach (WheelCollider wheel in wheelColliders){
			wheel.motorTorque = 0f;
		}
		Vector3 norm = -rb.velocity.normalized;
		norm.y = 0f;
		rb.AddForce(norm * carStats.breakSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
		Vector3 target = rb.angularVelocity;
		target.y = 0f;
		rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, target, rotateFightingSpeed * Time.deltaTime);
	}

	public void UpdateWheelPoses(){
		for (int i = 0; i < wheelTransforms.Length; i++){
			Transform wtransform = wheelTransforms[i];
			WheelCollider wcollider = wheelColliders[i];
			UpdateWheelPose(wcollider, wtransform);
		}
	}

	public void UpdateWheelPose(WheelCollider wcollider, Transform wtransform){
		Vector3 current = wtransform.rotation.eulerAngles;
		float sign = current.y < 180f ? 1f : -1f;
		wtransform.Rotate(0f, 0f, wcollider.rpm / 180f * 360f * Time.fixedDeltaTime * sign);
	}

	public void PickUp(PickUpType pickUpType, GameObject thingIHit, Pickup pickup) {
		Debug.Log("pickuptpye" + pickUpType);
		if (pickUpType == PickUpType.SpawnPoint) {
			spawnPos = thingIHit.transform.position + Vector3.up * 4;
			
			if (pickup.isFinish && checkPoint == 9) {
				if (round < BuildController.inst.roundsToDrive) {
					round++;
					roundsText.text = "Rounds: " + round + " / " + (BuildController.inst.roundsToDrive + 1);
				} else {
					SceneManager.LoadScene("CarBuilder", LoadSceneMode.Single);
					round = 0;
					checkPoint = 0;
				}
				checkPoint = -1;
			}

			if(pickup.checkpoint == checkPoint + 1) {
				checkPoint = pickup.checkpoint;
			}
		}
		//considering that there's anything else but spawnpoints.
		if(pickUpType == PickUpType.BuildPoint){
			BuildController.ModifyBuildPoints(pickup.buildPointsToEarn, player);
		}
			
		

		PickUpDestroy(pickUpType);
	}

	public bool PickUpDestroy(PickUpType pickUpType) {
		if (pickUpType == PickUpType.SpawnPoint) {
			return false;
		}

		return true;
	}

	public void ResetPosition(bool totalReset) {
		if (totalReset) { //off map
			transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(Vector3.zero));
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			foreach (WheelCollider wheel in wheelColliders){
				wheel.motorTorque = 0f;
			}
		} else { //button
			transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, 0f));
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			foreach (WheelCollider wheel in wheelColliders){
				wheel.motorTorque = 0f;
			}
		}
	}
}
