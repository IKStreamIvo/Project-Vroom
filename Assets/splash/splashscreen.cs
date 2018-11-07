using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class splashscreen : MonoBehaviour {
	VideoPlayer videoPlayer;
	public Animation fade;
	
	void Start () {
		videoPlayer = GetComponent<VideoPlayer>();
		videoPlayer.Play();
		StartCoroutine(checkFinish());
	}
	
	void Update () {
		
	}

	IEnumerator checkFinish(){
		while (videoPlayer.isPlaying){
			yield return null;
		}
		fade.Play();
		while(fade.isPlaying){
			yield return null;
		}
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}
}
