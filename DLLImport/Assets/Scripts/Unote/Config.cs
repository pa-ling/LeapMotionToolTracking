using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unote {
	public class Config {
		public class Debug {
			public bool DRAW_STROKEPOINTS  = false;
			public bool DRAW_BRUSHSEGMENTS = false;
			public bool DRAW_IA_GEOMETRY   = false;
		}

		public enum Mode {
			NONE,
			MOUSE,
			SPACE
		};

		public class Stroke {
			public Unote.Stroke.Brushes BRUSH = Unote.Stroke.Brushes.HEXAGON;

			public Mode  PAINT_MODE	      = Unote.Config.Mode.MOUSE;
			public float BRUSH_SIZE       = 0.2f;
			public Color BRUSH_COLOR      = new Color(1.0f, 0.0f, 0.0f);
			public bool  LERP_SP_PATH     = false;
			public float MIN_SP_DISTANCE  = 0.2f;
			public int   REFRESH_INTERVAL = 10;
		}

		public Debug debug;
		public Stroke stroke;
	}
}