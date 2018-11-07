using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//OBSTACLES ETC.
namespace MapComponent{
	public enum MapType{
		Crate, CarPart, PickUp
	}

	public class MapComponent : ScriptableObject {
		//public new string name;
		public virtual MapType mapType {get; set;}
		public GameObject GFX;
	}
}