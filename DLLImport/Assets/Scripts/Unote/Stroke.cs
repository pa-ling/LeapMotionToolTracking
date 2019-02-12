using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unote {
	using StrokePointSet = System.Collections.Generic.List<Unote.StrokePoint>;
	using StrokeSet      = System.Collections.Generic.List<Unote.Stroke>;

	public class Stroke {
		public enum Brushes {
			SQUARE,
			HEXAGON,
			RECTANGLE
		}

		public StrokePointSet strokePoints;
		public GameObject gameObject;

		public Stroke(StrokePointSet strokePointSet = null) {
			this.strokePoints = strokePointSet;
		}
	}
}
