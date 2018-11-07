using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PickupComponent {
	public enum PickUpType {
		BuildPoint, SpawnPoint
	}

	//Holds stats for every general pickup component.
	public class PickupComponent : MonoBehaviour {
		public virtual PickUpType pickUp { get; set; }
	}
}