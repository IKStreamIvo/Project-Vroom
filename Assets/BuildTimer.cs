using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTimer : MonoBehaviour {

	public static TMPro.TMP_Text Text;
	public static BuildTimer instance;

	private void Awake()
	{
		instance = this;
	}

	public static void StartTimer(int secs){
		Text = instance.GetComponent<TMPro.TMP_Text>();
		instance.StartCoroutine(instance.Timer(secs));
	}

	public IEnumerator Timer(int seconds){
		for (int secs = seconds; secs >= 0; secs--){
			string text = TimeSpan.FromSeconds(secs).ToString().Remove(0, 3);
			Text.SetText(text);
			yield return new WaitForSeconds(1);
		}
		BuildController.ForceOtherPlayerReady();
	}
}
