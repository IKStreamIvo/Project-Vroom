using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using CarComponents;
using TMPro;
using System;

public class BuildPlayer : MonoBehaviour {
	#region Editor Vars
		public int _player;
		public GameObject BuildCursor;
		public new Transform camera;
		public MeshCollider meshChecker;
		public CategorySelector UI;
		public TMP_Text BPText;
		public TMP_Text PartCostText;
		public GameObject ReadyText;
		[Header("Stat Display")]
		public TMP_Text speedText;
		public TMP_Text accelerationText;
		public TMP_Text breakSpeedText;
		public TMP_Text steeringText;
		[Header("Preview Stat Display")]
    	public int previewStatDisplayLength = 5;
		public TMP_Text PspeedText;
		public TMP_Text PaccelerationText;
		public TMP_Text PbreakSpeedText;
		public TMP_Text PsteeringText;
	#endregion
	#region Cursor Data
		private Vector3 _cursorPos;
		private GameObject _cursorBlock;
	#endregion
	#region Building Vars
		public GameObject Car;
		private Dictionary<Vector3, GameObject> _placedBlocks;
		private Dictionary<GameObject, Vector3> _blocksPlaced;
		public Dimensions moveLimits = new Dimensions(-10, 9, 9, -10, 10, 0);
		private int _selectedBlock = -1;
		public bool _playerReady;
		public bool ignoreBlockChecks = false;
		public Vector3 buildOffset = new Vector3();
		public static bool _buildable;
		public CarStats carStats;
	#endregion

	#region Unity Functions
		private void Start(){
			Car = new GameObject("Car");
			Car.tag = "player" + _player;
			Car.transform.position = transform.position;
			_cursorPos = Vector3.zero;
			_placedBlocks = new Dictionary<Vector3, GameObject>();
			_blocksPlaced = new Dictionary<GameObject, Vector3>();
			_playerReady = false;
			carStats = new CarStats();
		}

		private void Update(){
			if (BuildController.GetComponentCount() == 0) {
				Debug.LogError("[BuildController] No Building Blocks!");
				return;
			}

			//If player is ready, stop them from interacting with anything.
			if (_playerReady) {
				return;
			}

			if(_selectedBlock == -1){
				SelectBlock(0);
				UI.Setup();
				BPText.SetText(BuildController.GetBuidPoints(_player).ToString());
				if(File.Exists(Application.dataPath + @"\PlacedBlocksCatergories" + _player + ".txt")){
					LoadBuild();
				}
			}

			//Moving Cursor
			if(GetAxisDown("[Build] Move X " + _player)){
				MoveCursor(new Vector3(GetAxis("[Build] Move X " + _player, true, true), 0f, 0f));
			}
			if(GetAxisDown("[Build] Move Y " + _player)){
				MoveCursor(new Vector3(0f, 0f, GetAxis("[Build] Move Y " + _player, true, true)));
			}
			if(GetAxisDown("[Build] Move Vertical " + _player)) {
				MoveCursor(new Vector3(0f, GetAxis("[Build] Move Vertical " + _player, true, true), 0f));
			}
			
			//Cycle through blocks list
			if(GetAxisDown("[Build] Cycle Blocks Left " + _player)){
				SelectBlock(_selectedBlock - 1);
				UI.TriggerRight();
			}
			if(GetAxisDown("[Build] Cycle Blocks Right " + _player)){
				SelectBlock(_selectedBlock + 1);
				UI.TriggerLeft();
			}

			//Placing
			if(Input.GetButtonDown("[Build] Place Block " + _player)) {
				if(currentCollisions.Count == 0) PlaceBlock();
			}
			if(Input.GetButton("[Build] Remove Block " + _player)) {
				RemoveBlock();
			}

			//Save and Load
			if (Input.GetButtonDown("[Build] Ready " + _player)) {
				if(_placedBlocks.Count == 0) {
					Debug.LogError("You can't drive a car without any components!");
					return;
				}

				SaveBuildInCategories();
				
				if (_playerReady) {
					_playerReady = false;
					_cursorPos = Vector3.zero;
					SelectBlock(-1);
				} else {
					_playerReady = true;
					Destroy(_cursorBlock);
				}
				BuildController.PlayerReadyTrigger(_player, _playerReady);
				ReadyText.SetActive(true);
			}
			
			//Rotating
			if(GetAxisDown("[Build] Rotate Block " + _player)){
				_cursorBlock.transform.Rotate(Vector3.up * BuildController.GetComponent(_selectedBlock).rotateDegrees * GetAxis("[Build] Rotate Block " + _player, true, true), Space.Self);
				meshChecker.transform.rotation = _cursorBlock.transform.rotation;
			}
		}
	#endregion

	#region Collision Functions
		//Keeping track of collisions
		public List<Collider> currentCollisions = new List<Collider>();

		private void OnTriggerEnter(Collider other) {
			HologramColor(new Color(1f, 0f, 0f, .5f));
			//if it already exists.. dont do it!
			foreach(Collider coll in currentCollisions){
				if(other.gameObject == coll.gameObject){
					return;
				}
			}
			currentCollisions.Add(other);
		}
		private void OnTriggerExit(Collider other) {
			List<Collider> removeMe = new List<Collider>();
			foreach(Collider coll in currentCollisions){
				if(other.gameObject == coll.gameObject)
					removeMe.Add(coll);
			}
			foreach(Collider coll in removeMe) {
				currentCollisions.Remove(coll);
			}
			if(currentCollisions.Count == 0 && !_placedBlocks.ContainsKey(_cursorPos))
				HologramColor(new Color(1f, 1f, 1f, .5f));
		}

		private void HologramColor(Color color){
			List<MeshRenderer> renderers = new List<MeshRenderer>();
			if(_cursorBlock.GetComponent<MeshRenderer>())
				renderers.Add(_cursorBlock.GetComponent<MeshRenderer>());
			if(_cursorBlock.GetComponentInChildren<MeshRenderer>())
				renderers.AddRange(_cursorBlock.GetComponentsInChildren<MeshRenderer>());
			foreach(MeshRenderer renderer in renderers){
				renderer.sortingOrder = 50;
				Material material = renderer.material;
				
				//Change color
				material.color = color;

				//Change render mode to transparent instead of opaque (src: https://answers.unity.com/questions/1004666/change-material-rendering-mode-in-runtime.html)
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;

				renderer.material = material;	
			}
		}
	#endregion

	#region Building Functions
		private void MoveCursor(Vector3 translation){
			float rotY = Mathf.Round(camera.eulerAngles.y / 90) * 90;
			translation = Quaternion.Euler(0f, rotY, 0f) * translation;
			Vector3 nextPos = _cursorPos + new Vector3(Mathf.Round(translation.x), Mathf.Round(translation.y), Mathf.Round(translation.z));
			if(	nextPos.x >= moveLimits.left && nextPos.x <= moveLimits.right && 
				nextPos.z >= moveLimits.back && nextPos.z <= moveLimits.forward &&
				nextPos.y >= moveLimits.down && nextPos.y <= moveLimits.up){

				_cursorPos = nextPos;
				BuildCursor.transform.position = _cursorPos + buildOffset;
			}
			if(_placedBlocks.ContainsKey(_cursorPos)){
				HologramColor(new Color(1f, 0f, 0f, .5f));
			}else if(currentCollisions.Count == 0){
				HologramColor(new Color(1f, 1f, 1f, .5f));
			}
		}

		private void SelectBlock(int index){
			if(index >= BuildController.GetComponentCount()) {
				_selectedBlock = 0;
			}else if(index < 0) {
				_selectedBlock = BuildController.GetComponentCount() - 1;
			}else{
				_selectedBlock = index;
			}
			
			if(_cursorBlock != null){
				Destroy(_cursorBlock);
			}

			CarComponent component = BuildController.GetComponent(_selectedBlock);
			if(component.BuildGFX == null){
				_cursorBlock = Instantiate(component.GFX, _cursorPos + buildOffset, component.GFX.transform.rotation, BuildCursor.transform);
			}else{
				_cursorBlock = Instantiate(component.BuildGFX, _cursorPos + buildOffset, component.BuildGFX.transform.rotation, BuildCursor.transform);
			}
			//Make material a hologram
			HologramColor(new Color(1f, 1f, 1f, .5f));

			//If it has a collider, remove it and put it on the checker
			if(component.customMeshChecker != null){
				meshChecker.sharedMesh = component.customMeshChecker;
				meshChecker.isTrigger = true;
				if(_cursorBlock.GetComponent<MeshCollider>()){
					Destroy(_cursorBlock.GetComponent<MeshCollider>());
				}
				if(_cursorBlock.GetComponentInChildren<MeshCollider>()){
					foreach(MeshCollider collider in _cursorBlock.GetComponentsInChildren<MeshCollider>()){
						Destroy(collider);
					}
				}
			}else{
				if(_cursorBlock.GetComponent<MeshCollider>()){
					MeshCollider org = _cursorBlock.GetComponent<MeshCollider>();
					meshChecker.sharedMesh = org.sharedMesh;
					meshChecker.isTrigger = true;
					Destroy(org);
				}
				if(_cursorBlock.GetComponentInChildren<MeshCollider>()){
					foreach(MeshCollider collider in _cursorBlock.GetComponentsInChildren<MeshCollider>()){
						Destroy(collider);
					}
				}
			}
			meshChecker.transform.localPosition = component.colliderOffset;

			_cursorBlock.transform.Rotate(Vector3.up * component.initialRotation, Space.Self);
			meshChecker.transform.rotation = _cursorBlock.transform.rotation;

			PartCostText.SetText(component.buildPoints.ToString());

			PreviewStatDisplay();
		}

		private void PlaceBlock(){
			if(_placedBlocks.ContainsKey(_cursorPos)) return;
			CarComponent component = BuildController.GetComponent(_selectedBlock);

			if(BuildController.GetBuidPoints(_player) < component.buildPoints) return;
			BPText.SetText(BuildController.ModifyBuildPoints(-component.buildPoints, _player).ToString());

			GameObject placedBlock;
			if(component.BuildGFX != null){
				placedBlock = Instantiate(component.GFX, _cursorPos + buildOffset, _cursorBlock.transform.rotation, Car.transform);
			}else{
				placedBlock = Instantiate(component.GFX, _cursorPos + buildOffset, _cursorBlock.transform.rotation, Car.transform);
			}
			placedBlock.GetComponent<MeshRenderer>().sortingOrder = -50;
			placedBlock.AddComponent<BlockProperties>().data = component;
			//if(component.customMeshChecker != null){
				if(placedBlock.GetComponentInParent<MeshCollider>()){
					foreach(MeshCollider coll in placedBlock.GetComponentsInChildren<MeshCollider>()){
						Destroy(coll);
					}
				}
				GameObject meshChild = new GameObject("Mesh");
				meshChild.tag = "childmesh";
				MeshCollider cColl = meshChild.AddComponent<MeshCollider>();
				cColl.convex = true;
				if(component.customMeshChecker != null){
					cColl.sharedMesh = component.customMeshChecker;

				}else{
					Mesh sm = placedBlock.GetComponent<MeshCollider>().sharedMesh;
					cColl.sharedMesh = sm;
				}
				meshChild.transform.SetParent(placedBlock.transform);
				meshChild.transform.localPosition = component.colliderOffset + (cColl.sharedMesh.bounds.extents/10f) * .8f;
				meshChild.transform.localRotation = Quaternion.identity;
				meshChild.transform.localScale = new Vector3(.8f, .8f, .8f);

				if(placedBlock.GetComponent<MeshCollider>()){
					Destroy(placedBlock.GetComponent<MeshCollider>());
				}
				/*if(placedBlock.GetComponentInParent<MeshCollider>()){
					foreach(MeshCollider coll in placedBlock.GetComponentsInChildren<MeshCollider>()){
						Destroy(coll);
					}
				}*/
			//}
			
			_placedBlocks.Add(_cursorPos, placedBlock);
			_blocksPlaced.Add(placedBlock, _cursorPos);

			//CarStats
			carStats = UpdateCarStats(component, carStats, true);
			UpdateStatDisplay();
			PreviewStatDisplay();

			HologramColor(new Color(1f, 0f, 0f, .5f));
		}

		private void RemoveBlock(){
			if(currentCollisions.Count != 0){
				//find collision entry
				List<Collider> removeMe = new List<Collider>();
				removeMe.AddRange(currentCollisions);
				foreach(Collider coll in removeMe){
					GameObject go = coll.gameObject;
					if(coll.CompareTag("childmesh")) go = go.transform.parent.gameObject;
					currentCollisions.Remove(coll);
					if(_placedBlocks.ContainsValue(go)){
						_placedBlocks.Remove(_blocksPlaced[go]);
						_blocksPlaced.Remove(go);
					}
					CarComponent component = go.GetComponent<BlockProperties>().data;
					BPText.SetText(BuildController.ModifyBuildPoints(component.buildPoints, _player).ToString());
					//CarStats
					carStats = UpdateCarStats(component, carStats, false);
					UpdateStatDisplay();
					PreviewStatDisplay();
					Destroy(go);
				}
			}
			if(_placedBlocks.ContainsKey(_cursorPos)){
				GameObject removeMe = _placedBlocks[_cursorPos];
				_blocksPlaced.Remove(removeMe);
				_placedBlocks.Remove(_cursorPos);
				CarComponent component = removeMe.GetComponent<BlockProperties>().data;
				BPText.SetText(BuildController.ModifyBuildPoints(component.buildPoints, _player).ToString());
				//CarStats
				carStats = UpdateCarStats(component, carStats, false);
				UpdateStatDisplay();
				PreviewStatDisplay();
				Destroy(removeMe);
			}
			if(currentCollisions.Count == 0){
				HologramColor(new Color(1f,1f,1f,.5f));
			}
		}

		public void PreviewStatDisplay(){
			CarComponent component = BuildController.GetComponent(_selectedBlock);
			CarStats previewStats = UpdateCarStats(component, carStats, true);

			float accelerationDif = previewStats.acceleration - carStats.acceleration;
			string acceleration = accelerationDif.ToString();
			if(accelerationDif > 0f) acceleration = acceleration.Insert(0, "+");
			if (acceleration.Length > previewStatDisplayLength) acceleration = acceleration.Substring(0, previewStatDisplayLength);	
			PaccelerationText.SetText(acceleration);

			float speedDif = previewStats.maxSpeed - carStats.maxSpeed;
			string speed = speedDif.ToString();
			if(speedDif > 0f) speed = speed.Insert(0, "+");
			if (speed.Length > previewStatDisplayLength) speed = speed.Substring(0, previewStatDisplayLength);	
			PspeedText.SetText(speed);

			float breakSpeedDif = previewStats.breakSpeed - carStats.breakSpeed;
			string breakSpeed = breakSpeedDif.ToString();
			if(breakSpeedDif > 0f) breakSpeed = breakSpeed.Insert(0, "+");
			if (breakSpeed.Length > previewStatDisplayLength) breakSpeed = breakSpeed.Substring(0, previewStatDisplayLength);	
			PbreakSpeedText.SetText(breakSpeed);
			
			float steeringDif = previewStats.steerSpeed - carStats.steerSpeed;
			string steering = steeringDif.ToString();
			if(steeringDif > 0f) steering = steering.Insert(0, "+");
			if (steering.Length > previewStatDisplayLength) steering = steering.Substring(0, previewStatDisplayLength);	
			PsteeringText.SetText(steering);
		}
		public void UpdateStatDisplay(){
			accelerationText.SetText(carStats.acceleration.ToString());
			speedText.SetText(carStats.maxSpeed.ToString());
			breakSpeedText.SetText(carStats.breakSpeed.ToString());
			steeringText.SetText(carStats.steerSpeed.ToString());
		}
	//TODO terrainMultiplier is not really necessary here i think, but good thing to check over.
		public CarStats UpdateCarStats(CarComponent component, CarStats stats, bool positive){
			CarStats newStats = new CarStats(stats.maxSpeed,stats.acceleration,stats.breakSpeed, stats.steerSpeed, stats.terrainMultiplier);
			foreach(ComponentModifier modifier in component.modifiers){
				switch(modifier.stat){
					case CarStat.MaxSpeed: 
						switch(modifier.math){
							case MathOperator.plus:
								newStats.maxSpeed += modifier.value * (positive ? 1 : -1);
								break;
							case MathOperator.minus:
								newStats.maxSpeed -= modifier.value * (positive ? 1 : -1);
								break;
						}
						break;
					case CarStat.Acceleration: 
						switch(modifier.math){
							case MathOperator.plus:
								newStats.acceleration += modifier.value * (positive ? 1 : -1);
								break;
							case MathOperator.minus:
								newStats.acceleration -= modifier.value * (positive ? 1 : -1);
								break;
						}
						break;
					case CarStat.BreakSpeed: 
						switch(modifier.math){
							case MathOperator.plus:
								newStats.breakSpeed += modifier.value * (positive ? 1 : -1);
								break;
							case MathOperator.minus:
								newStats.breakSpeed -= modifier.value * (positive ? 1 : -1);
								break;
						}
						break;
					case CarStat.SteerSpeed: 
						switch(modifier.math){
							case MathOperator.plus:
								newStats.steerSpeed += modifier.value * (positive ? 1 : -1);
								break;
							case MathOperator.minus:
								newStats.steerSpeed -= modifier.value * (positive ? 1 : -1);
								break;
						}
						break;
				}
			}
			return newStats;
		}
	#endregion

	#region Save/Load
		//first is position, second is blockType, rotation.
		private Dictionary<string, string[]> saveStringDict = new Dictionary<string, string[]>();
		private Dictionary<string, string[]> saveCategoryDict = new Dictionary<string, string[]>();

		public void SaveBuildInCategories(){
			if(_placedBlocks.Count == 0) {
				Debug.Log("No blocks have been placed down. No saving has occured.");
				return;
			}

			//Setup the dictionary for every possible type
			Dictionary<CarComponents.Type, List<BlockSave>> saveme = new Dictionary<CarComponents.Type, List<BlockSave>>();
			foreach (CarComponents.Type type in (CarComponents.Type[])System.Enum.GetValues(typeof(CarComponents.Type))){
				saveme.Add(type, new List<BlockSave>());
			}

			//foreach block placed, check the type and put it in the correct slot
			foreach(GameObject block in _placedBlocks.Values){
				CarComponent component = block.GetComponent<BlockProperties>().data;
				BlockSave save = new BlockSave(component.name, block.transform.position - buildOffset, block.transform.rotation);
				
				//add to saveme dict
				saveme[component.type].Add(save);
			}

			string json = JsonConvert.SerializeObject(saveme, Formatting.Indented);
			File.WriteAllText(Application.dataPath + @"\PlacedBlocksCatergories" + _player + ".txt", json);
		}
		private void LoadBuild(){
			if(!File.Exists(Application.dataPath + @"\PlacedBlocksCatergories" + _player + ".txt")) {
				Debug.Log("No Save-File could be found! Are you sure you saved something?");
				return;
			}

			//If there are already blocks placed down, this will clear the placed ones.
			if (_placedBlocks.Count > 0) {
				for (int b = 0; b < _placedBlocks.Count; b++) {
					Destroy(_placedBlocks.ElementAt(b).Value);
				}

				_placedBlocks.Clear();
				_blocksPlaced.Clear();
			}

			//Get JSON
			string json = File.ReadAllText(Application.dataPath + @"\PlacedBlocksCatergories" + _player + ".txt");
			Dictionary<string, BlockSave[]> stringDict = JsonConvert.DeserializeObject<Dictionary<string, BlockSave[]>>(json);
			Dictionary<CarComponents.Type, BlockSave[]> enumDict = new Dictionary<CarComponents.Type, BlockSave[]>();
			List<BlockSave> list = new List<BlockSave>();
			
			foreach(string enumName in stringDict.Keys){
				CarComponents.Type type = (CarComponents.Type)Enum.Parse(typeof(CarComponents.Type), enumName, true);
				enumDict.Add(type, stringDict[enumName]);
				list.AddRange(stringDict[enumName]);
			}

			foreach(BlockSave blockSave in list){
				CarComponent component = BuildController.GetComponent(blockSave.name);
				GameObject prefab = ReturnBlockToPlace(blockSave.name);
				GameObject placedBlock = Instantiate(prefab, Vector3.zero, StringToQuaternion(blockSave.rotation), Car.transform);
				placedBlock.transform.localPosition = StringToVector3(blockSave.position);
				placedBlock.GetComponent<MeshRenderer>().sortingOrder = -50;
			placedBlock.AddComponent<BlockProperties>().data = component;
			//if(component.customMeshChecker != null){
				if(placedBlock.GetComponentInParent<MeshCollider>()){
					foreach(MeshCollider coll in placedBlock.GetComponentsInChildren<MeshCollider>()){
						Destroy(coll);
					}
				}
				GameObject meshChild = new GameObject("Mesh");
				meshChild.tag = "childmesh";
				MeshCollider cColl = meshChild.AddComponent<MeshCollider>();
				cColl.convex = true;
				if(component.customMeshChecker != null){
					cColl.sharedMesh = component.customMeshChecker;

				}else{
					Mesh sm = placedBlock.GetComponent<MeshCollider>().sharedMesh;
					cColl.sharedMesh = sm;
				}
				meshChild.transform.SetParent(placedBlock.transform);
				meshChild.transform.localPosition = component.colliderOffset + (cColl.sharedMesh.bounds.extents/10f) * .8f;
				meshChild.transform.localRotation = Quaternion.identity;
				meshChild.transform.localScale = new Vector3(.8f, .8f, .8f);

				if(placedBlock.GetComponent<MeshCollider>()){
					Destroy(placedBlock.GetComponent<MeshCollider>());
				}
				_placedBlocks.Add(StringToVector3(blockSave.position), placedBlock);
				_blocksPlaced.Add(placedBlock, StringToVector3(blockSave.position));

				//CarStats
				carStats = UpdateCarStats(component, carStats, true);
				UpdateStatDisplay();
				PreviewStatDisplay();
			}
		}


		/// <summary>
		/// Returns the Block GameObject with the name blockName
		/// </summary>
		/// <param name="blockName"></param>
		/// <returns>GameObject of blockName in Blocks</returns>
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

	#region Input Manager
		private Dictionary<string, bool> _pressedInputs = new Dictionary<string, bool>();
		private float GetAxis(string axis, bool raw = false, bool round = false){
			if(raw && round) 		return 	Mathf.Round(Input.GetAxisRaw(axis));
			else if(raw && !round) 	return 	Input.GetAxisRaw(axis);
			else if(!raw && round) 	return 	Mathf.Round(Input.GetAxis(axis));
			else 					return 	Input.GetAxis(axis);
		}
		private bool GetAxisDown(string axis){
			bool newState = GetAxis(axis, true, true) != 0f;

			bool prevState;
			if(_pressedInputs.TryGetValue(axis, out prevState)){ //does it exist yet?
				if(prevState != newState){ //its different
					_pressedInputs[axis] = newState;
					return newState;
				}else{ //its the same
					return false;
				}
			}else{ //add entry
				_pressedInputs.Add(axis, newState);
				//we didnt register it yet, so it was not yet pressed
				//return the raw state
				return newState;
			}
		}
	#endregion

	#region DEBUG
		List<Vector3> cubeposses = new List<Vector3>();

    private void OnDrawGizmos() {
			Gizmos.color = new Color(1f, 0f, 0f, .5f);
			foreach(Vector3 pos in cubeposses){
				Gizmos.DrawCube(pos, new Vector3(1f, 1f, 1f));
			}
		}
	#endregion
}

public class BlockSave{
	public string name;
	public string position;
	public string rotation;

	public BlockSave(string name, Vector3 position, Quaternion rotation){
		this.name = name;
		this.position = position.ToString();
		this.rotation = rotation.ToString();
	}

	[JsonConstructor]
	public BlockSave(string name, string position, string rotation){
		this.name = name;
		this.position = position;
		this.rotation = rotation;
	}
}

[System.Serializable]
public class Dimensions{
	public int left;
	public int right;
	public int forward;
	public int back;
	public int up;
	public int down;

	public Dimensions(int left, int right, int forward, int back, int up, int down){
		this.left = left;
		this.right = right;
		this.forward = forward;
		this.back = back;
		this.up = up;
		this.down = down;
	}
}
