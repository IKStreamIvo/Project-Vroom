using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarComponents{
	public enum Type{
		Block, Engine, Wheel
	}
	public enum CarStat {
		MaxSpeed, Acceleration, BreakSpeed, SteerSpeed, Offroad
	}

	public enum MathOperator {
		plus, minus
	}

	[CreateAssetMenu(fileName = "New Component", menuName = "CarComponent")]
	public class CarComponent : ScriptableObject {
		public int buildPoints;
		public Type type;
		public GameObject GFX;
		public GameObject BuildGFX;
		public int initialRotation;
		public int rotateDegrees = 90;
		public Sprite icon;
		public Mesh customMeshChecker;
		public List<ComponentModifier> modifiers = new List<ComponentModifier>();
        public Vector3 colliderOffset = Vector3.zero;
    }

	[System.Serializable]
	public class ComponentModifier {
		public CarStat stat;
		public MathOperator math;
		public float value;
	}
}