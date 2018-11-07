using System.Collections;
using System.Collections.Generic;
using CarComponents;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;

public class BuildController : MonoBehaviour {
	public static BuildController inst;
	public bool lockCursor;
	public List<CarComponent> CarComponents = new List<CarComponent>();
	public int BuildTimeAfterReady = 30;
	public BuildPlayer[] players = new BuildPlayer[2];
	private static CarComponent[] _carComponents;
	public static bool[] _playersReady = new bool[2];
	public Text[] roundsTexts;

	public int roundsToDrive;
	
	public int[] startBuildPoints = new int[2];
	public static int[] buildPoints = new int[2];
		
	private void Awake() {
		inst = this;

		if(lockCursor){
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		_carComponents = CarComponents.ToArray();
		_playersReady = new bool[2]; 
		buildPoints = startBuildPoints;
		inst.roundsToDrive = roundsToDrive;
		GetSetBuildPoints(true);
	}

	public void Update() {
		if (Input.GetKey(KeyCode.R)) {
			_playersReady[0] = false;
			_playersReady[1] = false;
			SceneManager.LoadScene(0);
		}
	}

	public static CarComponent GetComponent(int index){
		return _carComponents[index];
	}
	public static new CarComponent GetComponent(string name){
		CarComponent result = null;
		foreach(CarComponent component in _carComponents){
			if(component.name == name){
				result = component;
			}
		}
		return result;
	}

	public static int GetComponentCount(){
		return _carComponents.Length;
	}

	public static CarComponent[] GetComponentArray(){
		return _carComponents;
	}

	public static int GetBuidPoints(int player){
		player -= 1;
		return buildPoints[player];
	}

	public static int ModifyBuildPoints(int amount, int player){
		player -= 1;
		buildPoints[player] += amount;
		GetSetBuildPoints(false);
		return buildPoints[player];
	}

	public static void PlayerReadyTrigger(int player, bool ready) {
		_playersReady[player - 1] = ready;
		if(_playersReady[0] == true && _playersReady[1] == true) {
			GetSetBuildPoints(false);
			SceneManager.LoadScene("Infinitrack");
		}else{
			BuildTimer.StartTimer(inst.BuildTimeAfterReady);
		}
	}

	public static void ForceOtherPlayerReady(){
		foreach (BuildPlayer player in inst.players){
			player.SaveBuildInCategories();
		}
		GetSetBuildPoints(false);
		SceneManager.LoadScene("Infinitrack");
	}

	public static void GetSetBuildPoints(bool get) {
		if (get) {
			if(File.Exists(Application.dataPath + @"\BuildPoints.txt")) {
				buildPoints = JsonConvert.DeserializeObject<int[]>(File.ReadAllText(Application.dataPath + @"\BuildPoints.txt"));
			}else{
				GetSetBuildPoints(false);
			}
		} else {
			foreach(BuildPlayer player in inst.players) {
				string json = JsonConvert.SerializeObject(buildPoints, Formatting.None);
				File.WriteAllText(Application.dataPath + @"\BuildPoints.txt", json);
			}
		}
	}

	private void OnApplicationQuit() {
		GetSetBuildPoints(false);
	}
}
 