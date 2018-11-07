using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LookatObject))]
public class LookatObjectEditor : Editor {
	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		LookatObject script = (LookatObject)target;
		if(GUILayout.Button("Look at Target")){
			script.Look();
		}
	}
}
