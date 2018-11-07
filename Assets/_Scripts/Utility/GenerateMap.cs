using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateMap : MonoBehaviour {

	public bool startGeneration;
	public bool purgeTerrain;
	public bool mapGenerated;

	public int howManyBlocks;
	public int howManyRows;
	public int blockSize;

	public GameObject cubePrefab;
	public List<GameObject> terrainBlocks;

	public Vector3 startPosition;
	public float tempXValue;
	public float tempZValue;

	public Material colourHolder;

	// Use this for initialization
	void Start () {
		howManyRows = howManyBlocks - 1;
		tempXValue = startPosition.x;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (purgeTerrain) {
			foreach(GameObject terrainBlockObject in terrainBlocks) {
				DestroyImmediate(terrainBlockObject);
			}

			terrainBlocks.Clear();

			mapGenerated = false;
			purgeTerrain = false;
			startPosition.x = tempXValue;
			startPosition.z = tempZValue;
		}

		if (startGeneration) {
			if(terrainBlocks.Count > 0) {
				foreach (GameObject terrainBlockObject in terrainBlocks) {
					DestroyImmediate(terrainBlockObject);
				}

				terrainBlocks.Clear();
				
				startPosition.x = tempXValue;
				startPosition.z = tempZValue;
			}

			howManyRows = howManyBlocks - 1;
			tempXValue = startPosition.x;
			tempZValue = startPosition.z;
			for (int x = 1; x <= howManyBlocks; x++) {
				Material tempMaterial = new Material(colourHolder);
				tempMaterial.name = "texture" + x + ":" + howManyRows;

				GameObject tempCube = Instantiate(cubePrefab, startPosition, new Quaternion(0, 0, 0, 0), this.transform);
				tempCube.transform.localScale = new Vector3(blockSize, 0.5f, blockSize);
				tempCube.name = (x + "," + howManyRows);

				startPosition = new Vector3(startPosition.x + blockSize, startPosition.y, startPosition.z);

				if(x == howManyBlocks && howManyRows > 0) {
					startPosition = new Vector3(startPosition.x = tempXValue, startPosition.y, startPosition.z + blockSize);
					howManyRows--;
					x = 0;
				}
				terrainBlocks.Add(tempCube);
			}
			mapGenerated = true;
			startGeneration = false;
		}
	}
}
