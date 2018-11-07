using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CarComponents;
using Newtonsoft.Json;
using UnityEngine;

public class CarConstructor : MonoBehaviour {
	public bool overideStats;
	[SerializeField]
	public CarStats editorStats = new CarStats();
	public GameObject carP1;
	public GameObject carP2;
	public List<GameObject> wheelsL;
	public List<GameObject> wheelsR;


	private void Awake() {
		carP1 = Construct(1);
		GetWheels(1);

		carP2 = Construct(2);
		GetWheels(2);
	}

	public GameObject Construct(int player){

		if(!File.Exists(Application.dataPath + @"\PlacedBlocksCatergories" + player + ".txt")) {
			Debug.Log("No Save-File could be found! Are you sure you saved something?");
			return null;
		}

		//Get JSON
		string json = File.ReadAllText(Application.dataPath + @"\PlacedBlocksCatergories" + player + ".txt");
		Dictionary<string, BlockSave[]> stringDict = JsonConvert.DeserializeObject<Dictionary<string, BlockSave[]>>(json);
		Dictionary<CarComponents.Type, BlockSave[]> enumDict = new Dictionary<CarComponents.Type, BlockSave[]>();
		List<BlockSave> list = new List<BlockSave>();
		
		foreach(string enumName in stringDict.Keys){
			CarComponents.Type type = (CarComponents.Type)Enum.Parse(typeof(CarComponents.Type), enumName, true);
			enumDict.Add(type, stringDict[enumName]);
			list.AddRange(stringDict[enumName]);
		}
		
		//Create car
		GameObject car = new GameObject("Car (Player" + player + ")");
		car.transform.position = new Vector3(0f, 5f, 0f);
		Transform hull = new GameObject("Hull").transform;
		hull.SetParent(car.transform);
		hull.position = Vector3.zero;

		//CarStats!
		CarStats carStats = new CarStats();
		foreach(BlockSave blockSave in list){
			GameObject prefab = ReturnBlockToPlace(blockSave.name);
			GameObject block = Instantiate(prefab, StringToVector3(blockSave.position), StringToQuaternion(blockSave.rotation), hull);

			CarComponent component = (CarComponent)BuildController.GetComponent(blockSave.name);
			foreach(ComponentModifier modifier in component.modifiers){
				switch(modifier.stat){
					case CarStat.MaxSpeed: 
						switch(modifier.math){
							case MathOperator.plus:
								carStats.maxSpeed += modifier.value;
								break;
							case MathOperator.minus:
								carStats.maxSpeed -= modifier.value;
								break;
						}
						break;
					case CarStat.Acceleration: 
						switch(modifier.math){
							case MathOperator.plus:
								carStats.acceleration += modifier.value;
								break;
							case MathOperator.minus:
								carStats.acceleration -= modifier.value;
								break;
						}
						break;
					case CarStat.BreakSpeed: 
						switch(modifier.math){
							case MathOperator.plus:
								carStats.breakSpeed += modifier.value;
								break;
							case MathOperator.minus:
								carStats.breakSpeed -= modifier.value;
								break;
						}
						break;
					case CarStat.SteerSpeed: 
						switch(modifier.math){
							case MathOperator.plus:
								carStats.steerSpeed += modifier.value;
								break;
							case MathOperator.minus:
								carStats.steerSpeed -= modifier.value;
								break;
						}
						break;
				}
			}

			if(BuildController.GetComponent(blockSave.name).type == CarComponents.Type.Wheel) {
				if(StringToQuaternion(blockSave.rotation).eulerAngles.y == 90) {
					wheelsL.Add(block);
				}else if(StringToQuaternion(blockSave.rotation).eulerAngles.y == 270) {
					wheelsR.Add(block);
				}
			}
		}
		if(overideStats) carStats = editorStats;

		//Combine all meshes into one
		MeshFilter[] meshFilters = car.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		for (int i = 0; i < meshFilters.Length; i++){
			combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			if(meshFilters[i].GetComponent<MeshCollider>())
            	meshFilters[i].GetComponent<MeshCollider>().enabled = false;
		}
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
		/*BoxCollider boxCollider = hull.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = combinedMesh.bounds.extents * 2f;
		boxCollider.center = combinedMesh.bounds.center;*/
		MeshCollider combinedCollider = hull.gameObject.AddComponent<MeshCollider>();
		combinedCollider.sharedMesh = combinedMesh;
		combinedCollider.convex = true;

		//Setting up Car Controls
		Rigidbody hullRB = hull.gameObject.AddComponent<Rigidbody>();
		hullRB.interpolation = RigidbodyInterpolation.Extrapolate;
		hullRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		hullRB.angularDrag = 30f;
		hullRB.mass = .5f;
		hullRB.gameObject.layer = 9;
		CarController cCont = hull.gameObject.AddComponent<CarController>();

		cCont.player = player;
		cCont.carStats = carStats;
		cCont.collider = combinedCollider;
		
		cCont._drive = true;

		GameObject cam = GameObject.FindGameObjectWithTag("player" + player);
		Vector3 pos = cam.transform.position;
		cam.transform.SetParent(hull);
		cam.transform.position = new Vector3(0f, pos.y, pos.z);

		car.transform.position += new Vector3(player * 25f, 0f, 0f);

		// endpoint of Construct.
		return car;
	}

	#region Calculations
		GameObject ReturnBlockToPlace(string blockName) {
			foreach (CarComponent component in BuildController.GetComponentArray()) {
				if (component.name == blockName) {
					return component.GFX;
				}
			}
			return null;
		}

		//COPIED FROM https://answers.unity.com/questions/1134997/string-to-vector3.html
		public static Vector3 StringToVector3(string sVector) {
			// Remove the parentheses
			if (sVector.StartsWith("(") && sVector.EndsWith(")")) {
				sVector = sVector.Substring(1, sVector.Length - 2);
			}

			// split the items
			string[] sArray = sVector.Split(',');

			// store as a Vector3
			Vector3 result = new Vector3(
				float.Parse(sArray[0]),
				float.Parse(sArray[1]),
				float.Parse(sArray[2]));

			return result;
		}

		public static Quaternion StringToQuaternion(string sQuat) {
		if (sQuat.StartsWith("(") && sQuat.EndsWith(")")) {
			sQuat = sQuat.Substring(1, sQuat.Length - 2);
		}

		// split the items
		string[] sArray = sQuat.Split(',');

		// store as a Vector3
		Quaternion result = new Quaternion(
			float.Parse(sArray[0]),
			float.Parse(sArray[1]),
			float.Parse(sArray[2]),
			float.Parse(sArray[3]));

		return result;
	}
	#endregion

	void GetWheels(int player) {
		CarController cCont = GameObject.Find("Car (Player" + player + ")").transform.GetComponentInChildren<CarController>();

		cCont.wheelsL.AddRange(wheelsL);
		cCont.wheelsR.AddRange(wheelsR);
		
		wheelsL.Clear();
		wheelsR.Clear();
	}
}

[System.Serializable]
public class CarStats {
	public float maxSpeed;
	public float acceleration;
	public float breakSpeed;
	public float steerSpeed;
	public float offroad;
	public float terrainMultiplier;

	public CarStats(float maxSpeed, float acceleration, float breakSpeed, float steerSpeed, float terrainMultiplier, float offroad = 1f){
		this.maxSpeed = maxSpeed;
		this.acceleration = acceleration;
		this.breakSpeed = breakSpeed;
		this.steerSpeed = steerSpeed;
		this.offroad = offroad;
		this.terrainMultiplier = terrainMultiplier;
	}

	public CarStats(){

	}
}