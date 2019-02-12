using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unote {
	public class StrokePoint {
		public GameObject gameObject;

		public StrokePoint(GameObject gameObject, Vector3 position, Quaternion rotation = default(Quaternion)) {
			this.gameObject = gameObject;
			this.gameObject.transform.position = position;
			this.gameObject.transform.rotation = rotation;
		}
	}
}