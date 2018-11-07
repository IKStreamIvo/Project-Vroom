using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PickupComponent {
	public class Pickup : MonoBehaviour {

		public PickUpType pickUpType;
		public int buildPointsToEarn;
		private int[] pointsToEarn = {2, 5, 10};

		//Checkpoint Things
		public int checkpoint;
		public bool isFinish;

		private void Awake() {
			//name = "BuildPoint_" + buildPointsToEarn;
		}

		private void OnTriggerEnter(Collider collider) {
			NewCarController cc = collider.GetComponent<NewCarController>();
			
			cc.PickUp(pickUpType, gameObject, this);

			if (cc.PickUpDestroy(pickUpType)) {
				Destroy(this.gameObject);
			}
		}
	}
}