using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapComponent {
	[CreateAssetMenu(fileName = "New Crate", menuName = "MapComponents/Crate")]
	public class Crate : MapComponent {
		public override MapType mapType { get { return MapType.Crate; } }
		public float massToBreak;
		public bool creativeMode;
	}
}