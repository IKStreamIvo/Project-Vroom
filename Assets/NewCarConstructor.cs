using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CarComponents;
using Newtonsoft.Json;
using UnityEngine;

public class NewCarConstructor : MonoBehaviour {
	public bool overideStats;
	public CarStats editorStats = new CarStats();
	public Vector3 spawnPosP1;
	public Vector3 spawnPosP2;
	public GameObject carP1;
	public GameObject carP2;
	public CarCameraController camP1;
	public CarCameraController camP2;

	public List<PickupComponent.Pickup> spawnPoints;

	void Start(){
		carP1 = Construct(1);
		carP2 = Construct(2);

		camP1.target = carP1.transform;
		camP2.target = carP2.transform;
	}
	
	private GameObject Construct(int player){
		if(!File.Exists(Application.dataPath + @"\PlacedBlocksCatergories" + player + ".txt")) {
			Debug.Log("No Save-File could be found! Are you sure you saved something?"); //TODO show on screen.
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

		GameObject car = new GameObject("Car P" + player);
		//1. Static meshes (hull)
		GameObject hull = new GameObject("Hull");
		hull.transform.SetParent(car.transform);
		//2. Wheel Visuals (renderers)
		GameObject wheelRenderers = new GameObject("Wheels");
		wheelRenderers.transform.SetParent(car.transform);

		CarStats carStats = new CarStats();	

		//Fill the containers
		foreach(BlockSave blockSave in list){
			GameObject prefab = ReturnBlockToPlace(blockSave.name);
			CarComponent component = (CarComponent)BuildController.GetComponent(blockSave.name);
			GameObject block;
			switch(component.type){
				case CarComponents.Type.Wheel:
						block = Instantiate(prefab, StringToVector3(blockSave.position), StringToQuaternion(blockSave.rotation), wheelRenderers.transform);
					break;
				default:
						block = Instantiate(prefab, StringToVector3(blockSave.position), StringToQuaternion(blockSave.rotation), hull.transform);
					break;
			}
			carStats = ModifyCarStats(component, carStats);
		}
		///Setup rigidbody + collider
		//Combine all meshes into one
		MeshFilter[] meshFilters = hull.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		for (int i = 0; i < meshFilters.Length; i++){
			combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			if(meshFilters[i].GetComponent<MeshCollider>())
            	meshFilters[i].GetComponent<MeshCollider>().enabled = false;
		}
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
		MeshCollider combinedCollider = car.gameObject.AddComponent<MeshCollider>();
		combinedCollider.sharedMesh = combinedMesh;
		combinedCollider.convex = true;
		Rigidbody carRb = car.AddComponent<Rigidbody>();
		carRb.mass = 1500;

		//3. Wheel Colliders (colliders)
		List<WheelCollider> wheelColls = new List<WheelCollider>();
		List<Transform> wheelCollsTs = new List<Transform>();
		List<Transform> wheelTrans = new List<Transform>();

		GameObject wheelColliders = Instantiate(wheelRenderers, wheelRenderers.transform.position, wheelRenderers.transform.rotation, car.transform);
		wheelColliders.name = "WheelCollliders";
		
		//we need the lists to start with the two front wheels
		Transform front1col = null; 
		Transform front2col = null;
		for (int i = 0; i < wheelColliders.transform.childCount; i++){
			Transform child = wheelColliders.transform.GetChild(i);
			if(front1col == null){
				front1col = child;
			}else{
				if(child.position.z > front1col.position.z){
					front1col = child;
				}
			}
		}
		for (int i = 0; i < wheelColliders.transform.childCount; i++){
			Transform child = wheelColliders.transform.GetChild(i);
			if(child != front1col){
				if(front2col == null){
					front2col = child;
				}else{
					if(child.position.z > front2col.position.z){
						front2col = child;
					}
				}
			}
		}
		wheelCollsTs.Add(front1col);
		wheelCollsTs.Add(front2col);

		for (int child = 0; child < wheelColliders.transform.childCount; child++){
			Transform wheel = wheelColliders.transform.GetChild(child);

			//Remove all renderers
			List<MeshRenderer> renderers = new List<MeshRenderer>();
			renderers.AddRange(wheel.GetComponents<MeshRenderer>());
			renderers.AddRange(wheel.GetComponentsInChildren<MeshRenderer>());
			foreach(MeshRenderer renderer in renderers){
				Destroy(renderer);
			}
			List<MeshFilter> filters = new List<MeshFilter>();
			filters.AddRange(wheel.GetComponents<MeshFilter>());
			filters.AddRange(wheel.GetComponentsInChildren<MeshFilter>());
			foreach(MeshFilter filter in filters){
				Destroy(filter);
			}

			//Remove all colliders
			List<Collider> colls = new List<Collider>();
			colls.AddRange(wheel.GetComponents<Collider>());
			colls.AddRange(wheel.GetComponentsInChildren<Collider>());

			//Add Wheel Colliders
			WheelCollider wheelColl = wheel.gameObject.AddComponent<WheelCollider>();
			wheelColl.mass = 10f;
			wheelColl.radius = 1f;
			wheelColl.wheelDampingRate = 1f;
			wheelColl.suspensionDistance = .08f;
			
			if(!wheelCollsTs.Contains(wheel))
				wheelCollsTs.Add(wheel);

			
			foreach(Collider coll in colls){
				Destroy(coll);
			}
		}
		foreach(Transform wheelColTs in wheelCollsTs){
			wheelColls.Add(wheelColTs.GetComponent<WheelCollider>());
		}
		
		//Clear components from the wheel renderers
		Transform front1ts = null;
		Transform front2ts = null;
		for (int i = 0; i < wheelRenderers.transform.childCount; i++){
			Transform child = wheelRenderers.transform.GetChild(i);
			if(front1ts == null){
				front1ts = child;
			}else{
				if(child.position.z > front1ts.position.z){
					front1ts = child;
				}
			}
		}
		for (int i = 0; i < wheelRenderers.transform.childCount; i++){
			Transform child = wheelRenderers.transform.GetChild(i);
			if(child != front1ts){
				if(front2ts == null){
					front2ts = child;
				}else{
					if(child.position.z > front2ts.position.z){
						front2ts = child;
					}
				}
			}
		}
		wheelTrans.Add(front1ts);
		wheelTrans.Add(front2ts);

		GameObject[] wheelRender = new GameObject[wheelRenderers.transform.childCount];
		for (int i = 0; i < wheelRenderers.transform.childCount; i++){
			wheelRender[i] = wheelRenderers.transform.GetChild(i).gameObject;
		}
		foreach(GameObject wheel in wheelRender){
			GameObject wheelParent = new GameObject("Wheelparent");
			wheelParent.transform.SetParent(wheel.transform.parent);
			wheelParent.transform.position = wheel.transform.position;
			wheelParent.transform.rotation = wheel.transform.rotation;
			wheel.transform.SetParent(wheelParent.transform);
			if(!wheelTrans.Contains(wheel.transform))
				wheelTrans.Add(wheel.transform);

			//Remove all colliders
			List<Collider> colls = new List<Collider>();
			colls.AddRange(wheelParent.GetComponents<Collider>());
			colls.AddRange(wheelParent.GetComponentsInChildren<Collider>());
			foreach(Collider coll in colls){
				Destroy(coll);
			}
		}

		NewCarController controller = car.AddComponent<NewCarController>();
		controller.wheelColliders = wheelColls.ToArray();
		controller.wheelTransforms = wheelTrans.ToArray();
		controller.player = player;
		controller.particleSystems = car.GetComponentsInChildren<ParticleSystem>();
		controller.rb = carRb;
		controller.carStats = carStats;

		if(player == 1){
			car.transform.position = spawnPosP1;
			controller.spawnPos = spawnPosP1;
		}else{
			car.transform.position = spawnPosP2;
			controller.spawnPos = spawnPosP2;
		}
		
		SetLayerRecursively(car, 9);
		carRb.drag = .5f;
		carStats.terrainMultiplier = 1f;
		//carRb.angularDrag = .5f;
		return car;
	}

	//TODO we dont need to add terrainmultiplier here right? :thinking:
	private CarStats ModifyCarStats(CarComponent component, CarStats stats){
		CarStats carStats = new CarStats(stats.maxSpeed,stats.acceleration,stats.breakSpeed, stats.steerSpeed, stats.terrainMultiplier);
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
				case CarStat.Offroad:
					carStats.offroad += modifier.value;
					break;
			}
		}
		return carStats;
	}

	#region Calculations
		void SetLayerRecursively(GameObject obj, int newLayer){
			if(obj == null){
				return;
			}
		
			obj.layer = newLayer;
		
			foreach (Transform child in obj.transform){
				if (child == null){
					continue;
				}
				SetLayerRecursively(child.gameObject, newLayer);
			}
		}

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
}
