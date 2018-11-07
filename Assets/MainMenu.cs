using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	
	private void Start(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update () {
		if(Input.GetButtonDown("[Car] Gas 1")){
			StartG();
		}
		if(Input.GetButtonDown("[Car] Break 1")){
			Quit();
		}
	}

	public void StartG(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
	public void Quit(){
		Application.Quit();
	}
}
