using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainType : MonoBehaviour {
	public enum TerrainTypePicker { Grass, Mud, Tarmac, BoostSpeed, BoostSlow};

	public TerrainTypePicker terrainTypePicker;

	//TODO: based on wheel needs to be different.
	private void OnTriggerEnter(Collider other) {
		Debug.Log("hit" + name);
		ChangeTerrainType(other.gameObject);
	}

	public void ChangeTerrainType(GameObject other) { //chose gameobject rather than NewCarController to keep the OnTrigger intact.
		NewCarController cCont = other.GetComponent<NewCarController>();
		//TODO if wheels Offroad, do other stats.
		switch (terrainTypePicker) {
			case TerrainTypePicker.Grass:
				cCont.carStats.terrainMultiplier = 0.5f;
				break;
			case TerrainTypePicker.Mud:
				cCont.carStats.terrainMultiplier = 0.2f;
				break;
			case TerrainTypePicker.Tarmac:
				cCont.carStats.terrainMultiplier = 1f;
				break;
			case TerrainTypePicker.BoostSpeed:
				cCont.carStats.terrainMultiplier = 3f;
				break;
			case TerrainTypePicker.BoostSlow:
				cCont.carStats.terrainMultiplier = 0.6f;
				break;
		}
		cCont.TerrainType = terrainTypePicker;
	}
}
