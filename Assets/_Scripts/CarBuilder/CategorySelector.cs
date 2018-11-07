using System;
using System.Collections;
using System.Collections.Generic;
using CarComponents;
using UnityEngine;
using UnityEngine.UI;

public class CategorySelector : MonoBehaviour {
	public int player;
	public List<Sprite> Options = new List<Sprite>();
	private int currentOption = 0;
	private CarComponent[] options;
	private Transform[] slots = new Transform[5];
	private Animator animator;
	private bool isSetup = false;

	void Start () {
		animator = GetComponent<Animator>();
	
	}
	
	void Update () {
		if(!isSetup) return;
	}

	public void Setup(){
		//List of all slots, 0 and length-1 are invisible
		slots = new Transform[transform.childCount];
		for (int i = 0; i < slots.Length; i++){
			slots[i] = transform.GetChild(i);
		}

		//Get the options from buildcontroller
		options = (CarComponent[])BuildController.GetComponentArray().Clone();

		//Add the options to the slots
		UpdateIcons();

		//Save current option
		currentOption = 0;
		// So the current option can actually be retrieved by:
		// option = options[currentOption + 3]
		isSetup = true;
		animator.SetBool("isSetup", isSetup);
	}

	void UpdateIcons(){
		int optionIndex = options.Length-3;
		for (int i = 0; i < slots.Length; i++){
			Image img = slots[i].GetComponentInChildren<Image>();
			img.sprite = options[optionIndex].icon;

			if(optionIndex+1 >= options.Length){
				optionIndex = 0;
			}else{
				optionIndex++;
			}
		}
	}

	public void TriggerLeft(){
		AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
		if(state.IsName("PartSelectorLeft") || state.IsName("PartSelectorRight")){
			RotateLeft();
		}else{
			animator.SetTrigger("Left");
		}
	}

	public void TriggerRight(){
		AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
		if(state.IsName("PartSelectorRight") || state.IsName("PartSelectorLeft")){
			RotateRight();
		}else{
			animator.SetTrigger("Right");
		}
	}

	void RotateLeft(){
		if(currentOption-1 < 0){
			currentOption = options.Length-1;
		}else{
			currentOption -= 1;
		}
		ShiftLeft(options);
		UpdateIcons();
	}

	void RotateRight(){
		if(currentOption+1 >= options.Length){
			currentOption = 0;
		}else{
			currentOption += 1;
		}
		ShiftRight(options);
		UpdateIcons();
	}

	void ShiftLeft<T>(T[] arr)
    {
        T x = arr[0];
        Array.Copy(arr, 1, arr, 0, arr.Length - 1);
        Array.Clear(arr, arr.Length - 1, 1);
        arr[arr.Length-1] = x;
    }
    void ShiftRight<T>(T[] arr)
    {
        T x = arr[arr.Length-1];
        Array.Copy(arr, 0, arr, 1, arr.Length - 1);
        Array.Clear(arr, 0, 1);
        arr[0] = x;
	}

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
				return true;
			}
		}
	#endregion
}
