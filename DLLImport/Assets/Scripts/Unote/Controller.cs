using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Extensions;

namespace Unote {
	using StrokePointSet = System.Collections.Generic.List<Unote.StrokePoint>;
	using StrokeSet      = System.Collections.Generic.List<Unote.Stroke>;

	public class Controller : MonoBehaviour {
		public class State {
			public bool LOCK 		   = false;
			public bool btn_0          = false;
			public bool btn_1          = false;
			public bool isDrawing      = false;
			public bool addStrokePoint = false;
		}

		private class Cache {
			// infinte drawing plane
			public Plane   intersectionPlane;
			public Vector3 planeInNormal;
			public Vector3 planeInPoint;

			// intersection ray between origin and plane
			public Ray intersectionRay;

			// position + direction of users pointer on plane
			public Vector3 userBrushPointerPosition;
			public Vector3 userBrushPointerDirection;

			// saved position of last stroke point
			public Vector3 lastStrokePointPosition;

			// stroke point set -> clear after stroke generation
			public StrokePointSet strokePoints;

			// strokes set cache
			public StrokeSet strokes;
		}

		private class Store {
			public int strokeIdIndex;

			public StrokeSet strokes;
		}
			
		private bool READ_CONFIG = false;

		public GameObject strokePrefab;

		// runtime configuration
		private Unote.Config config;
		public Unote.Controller.State state;
		private Unote.Controller.Cache cache;
		private Unote.Controller.Store store;

		// editor controls
		public Unote.Config.Mode paintMode;
		public Unote.Stroke.Brushes brush;
		public float brushSize;
		public Color brushColor;
		public bool lerpStrokePath;
		public float minSPDistance;

		// ##### DEBUG #####
		public bool TEST_STROKE = false;

		// internal timer
		private Timer timer;

		// brush geometry storage
		private Vector3[] brushGeometry;

		////////////////////////////////////////////////////////////////////////////////////////////////////

		// updates and returns the drawing position
		public Vector3 UpdateBrushPosition(Vector3 screenPointPosition) {
			// cast ray from camera through a screen point
			this.cache.intersectionRay = Camera.main.ScreenPointToRay(screenPointPosition);

			Vector3 intersectionPoint = new Vector3(0.0f, 0.0f, 0.0f);

			// get intersection from ray on plane
			float distance;
			if (this.cache.intersectionPlane.Raycast(this.cache.intersectionRay, out distance)) {
				// set intersection distance as position point
				intersectionPoint = this.cache.intersectionRay.GetPoint(distance);
			}

			//this.lastBrushPosition = this.brushPointerPosition;
			this.cache.userBrushPointerPosition = intersectionPoint;

			return intersectionPoint;
		}

		// updates the drawing movement direction and returns euler angles
		private Vector3 UpdateDrawingDirection() {
			transform.LookAt(this.cache.lastStrokePointPosition);
			this.cache.userBrushPointerDirection = transform.eulerAngles;
			return this.cache.userBrushPointerDirection;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		private void Draw() {
			if (!this.state.LOCK && !this.state.isDrawing) {
				this.state.isDrawing = true;
				this.timer.Start();
				this.SetBrush(this.brush);
			}

			if (this.state.isDrawing) {
				this.timer.Start();
			}
            //transform.root.GetChild(1).GetChild(1).GetComponent<ParticleSystem>().Play();
        }

		private void StopDrawing() {
			if (this.state.isDrawing) {
				this.state.isDrawing = false;
				this.timer.Stop();
			}
            //transform.root.GetChild(1).GetChild(1).GetComponent<ParticleSystem>().Clear();
        }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		// - called by ElapsedEventHandler of internal timer
		// - checks min distance to previous stroke point
		// - activates generation of next stroke point if dist > minDist
		private void AddNewStrokePoint(object source, ElapsedEventArgs e) {
			float distanceToLastStrokePoint = Vector3.Distance(this.cache.lastStrokePointPosition, this.cache.userBrushPointerPosition);

			if (distanceToLastStrokePoint > this.minSPDistance) {
				this.cache.lastStrokePointPosition = this.cache.userBrushPointerPosition;
				this.state.addStrokePoint = true;
			}
		}
			
		private void ClearStrokePointCache() {
			for (int i = 0; i < this.cache.strokePoints.Count; i++) {
				Unote.StrokePoint strokePoint = this.cache.strokePoints[i];
				Destroy(strokePoint.gameObject);
			}
			this.cache.strokePoints.Clear();
		}

		private void ClearStrokeCache() {
			for (int i = 0; i < this.cache.strokes.Count; i++) {
				Unote.Stroke stroke = this.cache.strokes[i];
				Destroy(stroke.gameObject);
				this.cache.strokes.RemoveAt(i);
			}
		}

		// stroke generation chain
		private void GenerateStroke(bool isClosed, bool strokeLoad = false) {
			if (this.cache.strokePoints.Count > 1) {
				// remove all strokes from cache
				this.ClearStrokeCache();
			
				if (isClosed) {
					this.store.strokeIdIndex++;
					Unote.Stroke newStroke = new Unote.Stroke(this.ProcessStrokePoints(isClosed, strokeLoad));
					newStroke.gameObject   = this.CreateStrokeGameObject(newStroke);
					this.store.strokes.Add(newStroke);
					this.ClearStrokePointCache();
				} else {
					Unote.Stroke newStroke = new Unote.Stroke(this.ProcessStrokePoints(isClosed, strokeLoad));
					newStroke.gameObject   = this.CreateStrokeGameObject(newStroke);
					this.cache.strokes.Add(newStroke);

					// Particle System (?) - Die Fliege
					//this.cache.strokePoints.Clear();
				}
			}
		}

		// create stroke
		private StrokePointSet ProcessStrokePoints(bool isClosed, bool strokeLoad = false) {
			if (this.cache.strokePoints != null) {
				int lastStrokePoint    = this.cache.strokePoints.Count - 1;
				int lastProcessedPoint = strokeLoad ? 0 : this.cache.strokePoints.Count - 2;

				// set rotations relative to next stroke point (direction from i to i+1)
				for (int i = lastProcessedPoint; i <= lastStrokePoint; i++) {
					GameObject emptyGameObject = new GameObject();
					Quaternion rotation = new Quaternion();
					Vector3 relativePos = new Vector3(0.0f, 0.0f, 0.0f);
					Transform thisSPTransform = this.cache.strokePoints[i+0].gameObject.transform;
					Transform prevSPTransform = emptyGameObject.transform;

					if (i > 0) {
						prevSPTransform = this.cache.strokePoints[i-1].gameObject.transform;
						relativePos = thisSPTransform.position - prevSPTransform.position;
					} else {
						relativePos = this.cache.strokePoints[i+1].gameObject.transform.position - thisSPTransform.position;
					}

					relativePos = relativePos == Vector3.zero ? new Vector3(0.01f, 0.01f, 0.01f) : relativePos;
					rotation = Quaternion.LookRotation(relativePos.normalized, prevSPTransform.up);
					//rotation = Quaternion.Euler(this.cache.userBrushPointerDirection);
					thisSPTransform.rotation = rotation;

					Destroy(emptyGameObject);
				}
					
				// set interpolated rotations between two adjacent stroke points (rotation between i-1 and i+1)
			    for (int i = lastProcessedPoint; i <= lastStrokePoint; i++) {
					Quaternion rotation = this.cache.strokePoints[i].gameObject.transform.rotation;

					if (!this.lerpStrokePath || (i == 0 || i == lastStrokePoint)) {
						rotation = this.cache.strokePoints[i].gameObject.transform.rotation;
					} else if (i > 0 && i < lastStrokePoint) {
						rotation = Quaternion.Lerp(this.cache.strokePoints[i-1].gameObject.transform.rotation, this.cache.strokePoints[i+1].gameObject.transform.rotation, 0.5f);
					}

					if (strokeLoad || (!isClosed && (i == 0 || i == lastStrokePoint))) {
						this.cache.strokePoints[i].gameObject = this.CreateSPBGameObject(this.cache.strokePoints[i], rotation, i);
					}
				}
			}

			StrokePointSet strokePointCopy = this.cache.strokePoints.GetRange(0, this.cache.strokePoints.Count);

			return strokePointCopy;
		}

		private GameObject CreateSPBGameObject(Unote.StrokePoint strokePoint, Quaternion rotation, int id = 0) {
			Vector3[] spb_vertices = new Vector3[brushGeometry.Length];
			Vector3[] spb_normals = new Vector3[brushGeometry.Length];
			//Vector2[] uv = new Vector2[7];

			int[] spb_indices = new int[(this.brushGeometry.Length - 2) * 3];

			// set vertices, normals
			for (int i = 0; i < this.brushGeometry.Length; i++) {
				spb_vertices[i] = this.brushGeometry[i];
				spb_normals[i]  = Vector3.up;
			}

			// set indices
			for (int i = 0; i < (this.brushGeometry.Length - 2); i++) {
				int idx = i * 3;
				spb_indices[0]     = 0;
				spb_indices[idx+1] = i+1;
				spb_indices[idx+2] = i+2;
			}

			//Create an instance of the Unity Mesh class:
			Mesh spb_mesh = new Mesh();

			//add our vertex and triangle values to the new mesh:
			spb_mesh.vertices = spb_vertices;
			spb_mesh.normals = spb_normals;
			//mesh.uv = uv;
			spb_mesh.triangles = spb_indices;

			//have the mesh recalculate its bounding box (required for proper rendering):
			spb_mesh.RecalculateBounds();

			strokePoint.gameObject.name = "StokePoint_" + this.store.strokeIdIndex + "-" + id;
			strokePoint.gameObject.AddComponent<MeshFilter>();
			strokePoint.gameObject.AddComponent<MeshRenderer>();

			if (!this.config.debug.DRAW_BRUSHSEGMENTS) {
				strokePoint.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}

			//Look for a MeshFilter component attached to this GameObject:
			MeshFilter filter = strokePoint.gameObject.GetComponent<MeshFilter>();

			//If the MeshFilter exists, attach the new mesh to it.
			//Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
			if (filter != null) {
				filter.sharedMesh = spb_mesh;
			}

			//strokePoint.gameObject.transform.position = strokePoint.gameObject.transform.position;
			strokePoint.gameObject.transform.rotation = rotation;

			return strokePoint.gameObject;
		}

		private GameObject CreateStrokeGameObject(Unote.Stroke stroke) {
			StrokePointSet strokePoints = stroke.strokePoints;

			if (strokePoints != null) {
				int lastStrokePoint         = strokePoints.Count - 1;
				int spb_vertexCount         = strokePoints[0].gameObject.GetComponent<MeshFilter>().mesh.vertexCount; // count of vertices per brush
				int sph_endPointIndiceCount = (spb_vertexCount - 2) * 3; // (n - 2) * 3 = number of triangles * 3 = number of indices per brush

				Mesh strokePathHull_mesh          = new Mesh();
				Vector3[] strokePathHull_vertices = new Vector3[4 * spb_vertexCount * strokePoints.Count];
				int[] strokePathHull_indices      = new int[(6 * spb_vertexCount * (strokePoints.Count-1)) + (sph_endPointIndiceCount * 2)];

				// for every stroke point
				for (int sp_i = 0; sp_i < lastStrokePoint; sp_i++) {
					GameObject spb0 = strokePoints[sp_i].gameObject;
					GameObject spb1 = strokePoints[sp_i+1].gameObject;

					Mesh spb0_mesh = spb0.GetComponent<MeshFilter>().mesh;
					Mesh spb1_mesh = spb1.GetComponent<MeshFilter>().mesh;

					Vector3[] spHullSegment_vertices = new Vector3[4 * spb_vertexCount];
					int[] spHullSegment_indices      = new int[(6 * spb_vertexCount)];

					int spb_vertexOffset = sp_i * spHullSegment_vertices.Length;

					/*spHullSegment_vertices[0] = spb0.transform.TransformPoint(spb0_mesh.vertices[0]);
					spHullSegment_vertices[1] = spb1.transform.TransformPoint(spb1_mesh.vertices[0]);
					spHullSegment_vertices[2] = spb1.transform.TransformPoint(spb1_mesh.vertices[1]);
					spHullSegment_vertices[3] = spb0.transform.TransformPoint(spb0_mesh.vertices[1]);
					
					spHullSegment_vertices[4] = spb0.transform.TransformPoint(spb0_mesh.vertices[1]);
					spHullSegment_vertices[5] = spb1.transform.TransformPoint(spb1_mesh.vertices[1]);
					spHullSegment_vertices[6] = spb1.transform.TransformPoint(spb1_mesh.vertices[2]);
					spHullSegment_vertices[7] = spb0.transform.TransformPoint(spb0_mesh.vertices[2]);

					spHullSegment_vertices[8] = spb0.transform.TransformPoint(spb0_mesh.vertices[2]);
					spHullSegment_vertices[9] = spb1.transform.TransformPoint(spb1_mesh.vertices[2]);
					spHullSegment_vertices[10] = spb1.transform.TransformPoint(spb1_mesh.vertices[3]);
					spHullSegment_vertices[11] = spb0.transform.TransformPoint(spb0_mesh.vertices[3]);

					spHullSegment_vertices[12] = spb0.transform.TransformPoint(spb0_mesh.vertices[3]);
					spHullSegment_vertices[13] = spb1.transform.TransformPoint(spb1_mesh.vertices[3]);
					spHullSegment_vertices[14] = spb1.transform.TransformPoint(spb1_mesh.vertices[4]);
					spHullSegment_vertices[15] = spb0.transform.TransformPoint(spb0_mesh.vertices[4]);

					spHullSegment_vertices[16] = spb0.transform.TransformPoint(spb0_mesh.vertices[4]);
					spHullSegment_vertices[17] = spb1.transform.TransformPoint(spb1_mesh.vertices[4]);
					spHullSegment_vertices[18] = spb1.transform.TransformPoint(spb1_mesh.vertices[5]);
					spHullSegment_vertices[19] = spb0.transform.TransformPoint(spb0_mesh.vertices[5]);

					spHullSegment_vertices[20] = spb0.transform.TransformPoint(spb0_mesh.vertices[5]);
					spHullSegment_vertices[21] = spb1.transform.TransformPoint(spb1_mesh.vertices[5]);
					spHullSegment_vertices[22] = spb1.transform.TransformPoint(spb1_mesh.vertices[0]);
					spHullSegment_vertices[23] = spb0.transform.TransformPoint(spb0_mesh.vertices[0]);

					spHullSegment_indices[0] = 0;
					spHullSegment_indices[1] = 1;
					spHullSegment_indices[2] = 2;
					spHullSegment_indices[3] = 0;
					spHullSegment_indices[4] = 2;
					spHullSegment_indices[5] = 3;

					spHullSegment_indices[6] = 4;
					spHullSegment_indices[7] = 5;
					spHullSegment_indices[8] = 6;
					spHullSegment_indices[9] = 4;
					spHullSegment_indices[10] = 6;
					spHullSegment_indices[11] = 7;

					spHullSegment_indices[12] = 8;
					spHullSegment_indices[13] = 9;
					spHullSegment_indices[14] = 10;
					spHullSegment_indices[15] = 8;
					spHullSegment_indices[16] = 10;
					spHullSegment_indices[17] = 11;

					spHullSegment_indices[18] = 12;
					spHullSegment_indices[19] = 13;
					spHullSegment_indices[20] = 14;
					spHullSegment_indices[21] = 12;
					spHullSegment_indices[22] = 14;
					spHullSegment_indices[23] = 15;

					spHullSegment_indices[24] = 16;
					spHullSegment_indices[25] = 17;
					spHullSegment_indices[26] = 18;
					spHullSegment_indices[27] = 16;
					spHullSegment_indices[28] = 18;
					spHullSegment_indices[29] = 19;

					spHullSegment_indices[30] = 20;
					spHullSegment_indices[31] = 21;
					spHullSegment_indices[32] = 22;
					spHullSegment_indices[33] = 20;
					spHullSegment_indices[34] = 22;
					spHullSegment_indices[35] = 23;*/

					// calculate number of connected triangles that share one vertex
					//int brushIndicesCount    = (spb_vertexCount - 2) * 3; // (n - 2) * 3
					int[] startPoint_indices = new int[sph_endPointIndiceCount];
					int[] endPoint_indices   = new int[sph_endPointIndiceCount];

					// for every brush side on a stroke point -> connect brushes with quad face
					for (int spb_v = 0; spb_v < spb_vertexCount; spb_v++) {
						int v_idx = spb_v * 4;
						int i_idx = spb_v * 6;

						// copy stroke point brush vertices into stroke path hull vertex buffer
						spHullSegment_vertices[v_idx]   = spb0.transform.TransformPoint(spb0_mesh.vertices[spb_v]);
						spHullSegment_vertices[v_idx+1] = spb1.transform.TransformPoint(spb1_mesh.vertices[spb_v]);
						spHullSegment_vertices[v_idx+2] = spb1.transform.TransformPoint(spb1_mesh.vertices[(spb_v+1) % spb_vertexCount]); // set last vertex = 0
						spHullSegment_vertices[v_idx+3] = spb0.transform.TransformPoint(spb0_mesh.vertices[(spb_v+1) % spb_vertexCount]);

						// close start point brush -> set start point face indices
						if (sp_i == 0 && spb_v == 0) {
							for (int i = 0; i < (this.brushGeometry.Length - 2); i++) {
								int idx = i * 3;
								startPoint_indices[idx]   = v_idx + 0;
								startPoint_indices[idx+1] = v_idx + idx + (3 + i);
								startPoint_indices[idx+2] = v_idx + idx + (7 + i);
							}
						}

						// close end point brush -> set end point face indices
						if (sp_i == lastStrokePoint-1 && spb_v == 0) {
							for (int i = 0; i < (this.brushGeometry.Length - 2); i++) {
								int idx = i * 3;
								endPoint_indices[idx] = (v_idx) + 1 + spb_vertexOffset;				  // <
								endPoint_indices[idx+1] = (v_idx) + idx + (6 + i) + spb_vertexOffset; // counter-clockwise order to swap face
								endPoint_indices[idx+2] = (v_idx) + idx + (2 + i) + spb_vertexOffset; // <
							}
						}

						// set stroke path hull indices
						spHullSegment_indices[i_idx]   = v_idx   + spb_vertexOffset;
						spHullSegment_indices[i_idx+1] = v_idx+1 + spb_vertexOffset;
						spHullSegment_indices[i_idx+2] = v_idx+2 + spb_vertexOffset;
						spHullSegment_indices[i_idx+3] = v_idx   + spb_vertexOffset;
						spHullSegment_indices[i_idx+4] = v_idx+2 + spb_vertexOffset;
						spHullSegment_indices[i_idx+5] = v_idx+3 + spb_vertexOffset;
					}

					// copy hull segment vertices into stroke hull vertex buffer
					spHullSegment_vertices.CopyTo(
						strokePathHull_vertices,
						sp_i * spHullSegment_vertices.Length
					);

					// copy start point indices into stroke hull index buffer
					if (sp_i == 0) {
						startPoint_indices.CopyTo(
							strokePathHull_indices,
							0
						);
					}

					// copy hull segment indices into stroke hull index buffer
					spHullSegment_indices.CopyTo(
						strokePathHull_indices,
						(sp_i * spHullSegment_indices.Length) + sph_endPointIndiceCount
					);

					if (sp_i == lastStrokePoint-1) {
						//int test = ((sp_i+1) * spHullSegment_indices.Length) + sph_endPointIndiceCount;
						endPoint_indices.CopyTo(
							strokePathHull_indices,
							((sp_i+1) * spHullSegment_indices.Length) + sph_endPointIndiceCount
						);
					}
				}

				//Vector3[] normals = new Vector3[4];

				// manually calculate surface normals
				/*Vector3 a = spHullSegment_vertices[0];
				Vector3 b = spHullSegment_vertices[1];
				Vector3 c = spHullSegment_vertices[2];
				Vector3 d = spHullSegment_vertices[3];

				normals[0] = Vector3.Cross(b - a, c - a).normalized;
				normals[1] = Vector3.Cross(c - b, a - b).normalized;
				normals[2] = Vector3.Cross(a - c, b - c).normalized;
				normals[3] = Vector3.Cross(a - d, c - d).normalized;*/

				strokePathHull_mesh.vertices = strokePathHull_vertices;
				//brushSegmentHullMesh.normals = normals;
				//mesh.uv = uv;
				strokePathHull_mesh.triangles = strokePathHull_indices;

				strokePathHull_mesh.RecalculateBounds();
				strokePathHull_mesh.RecalculateNormals();

				//Material surfaceMaterial = new Material(Shader.Find("Self-Illumin/Diffuse"));
				/*Material surfaceMaterial = (Material)Resources.Load("SolidColor", typeof(Material));

				GameObject strokePathHull_obj = new GameObject("Stroke_" + this.store.strokeIdIndex);
				strokePathHull_obj.AddComponent<MeshFilter>();
				strokePathHull_obj.AddComponent<MeshRenderer>();*/


				GameObject strokePathHull_obj = Instantiate(this.strokePrefab);

				strokePathHull_obj.name = "Stroke_" + this.store.strokeIdIndex;

				MeshFilter filter = strokePathHull_obj.GetComponent<MeshFilter>();
				Renderer render   = strokePathHull_obj.GetComponent<MeshRenderer>();

				//render.material = surfaceMaterial;
				render.material.color = this.brushColor;


				if (filter != null) {
					filter.sharedMesh = strokePathHull_mesh;
				}

				foreach (StrokePoint strokePoint in stroke.strokePoints) {
					strokePoint.gameObject.transform.parent = strokePathHull_obj.transform;
				}

				return strokePathHull_obj;
			}

			return null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////

		// set brush geometry: square
		private void LoadBrushGeometrySquare(float brushSize = 1.0f) {
			// 3 o--------o 0
			//   |        |
			//   |   0.0  |
			//   |        |
			// 2 o--------o 1 

			float diameter = brushSize;
			float radius   = diameter / 2;

			this.brushGeometry    = new Vector3[4];
			this.brushGeometry[0] = new Vector3(radius, radius, 0.0f);
			this.brushGeometry[1] = new Vector3(radius, -radius, 0.0f);
			this.brushGeometry[2] = new Vector3(-radius, -radius, 0.0f);
			this.brushGeometry[3] = new Vector3(-radius, radius, 0.0f);
		}
		
		// set brush geometry: hexagon
		private void LoadBrushGeometryHexagon(float brushSize = 1.0f) {
			//   5 o-----o 0
			//    /       \
			// 4 o   0.0   o 1
			//    \       /
			//   3 o-----o 2

			float diameter = brushSize;
			float radius   = diameter / 2;

			this.brushGeometry    = new Vector3[6];
			this.brushGeometry[0] = new Vector3(radius/2, radius, 0.0f);
			this.brushGeometry[1] = new Vector3(radius, 0.0f, 0.0f);
			this.brushGeometry[2] = new Vector3(radius/2, -radius, 0.0f);
			this.brushGeometry[3] = new Vector3(-radius/2, -radius, 0.0f);
			this.brushGeometry[4] = new Vector3(-radius, 0.0f, 0.0f);
			this.brushGeometry[5] = new Vector3(-radius/2, radius, 0.0f);
		}

		// set brush geometry: rectangle
		private void LoadBrushGeometryRectangle(float brushSize = 1.0f) {
			// 3 o--------o 0
			//   |   0.0  |
			// 2 o--------o 1 

			float diameter = brushSize;
			float radius   = diameter / 2;

			this.brushGeometry    = new Vector3[4];
			this.brushGeometry[0] = new Vector3(radius, radius/4, 0.0f);
			this.brushGeometry[1] = new Vector3(radius, -radius/4, 0.0f);
			this.brushGeometry[2] = new Vector3(-radius, -radius/4, 0.0f);
			this.brushGeometry[3] = new Vector3(-radius, radius/4, 0.0f);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////

		private void SetBrush(Unote.Stroke.Brushes brush) {
			switch (brush) {
				case Unote.Stroke.Brushes.SQUARE: {
					this.LoadBrushGeometrySquare(this.brushSize);
					break;
				}
				case Unote.Stroke.Brushes.HEXAGON: {
					this.LoadBrushGeometryHexagon(this.brushSize);
					break;
				}
				case Unote.Stroke.Brushes.RECTANGLE: {
					this.LoadBrushGeometryRectangle(this.brushSize);
					break;
				}
				default: {
					break;
				}
			}
		}

		// create unlimited plane with normalvector 0, 0, 1 and position 0, 0, 0
		private void SetIntersectionPlane(Vector3 inNormal, Vector3 inPoint) {
			this.cache.planeInNormal     = inNormal;
			this.cache.planeInPoint      = inPoint;
			this.cache.intersectionPlane = new Plane(this.cache.planeInNormal, this.cache.planeInPoint);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////

		// debug output
		private void OnDebugDraw() {
			DebugExt.DrawPlane(this.cache.intersectionPlane.normal, this.cache.planeInPoint, Color.blue);
			Debug.DrawRay(this.cache.intersectionRay.origin, this.cache.intersectionRay.direction * 20, Color.yellow);
		}
			
		// constructor
		private bool IS_LOADED;
		private void Start() {
			this.IS_LOADED = false;

			this.config       		 = new Unote.Config();
			this.config.debug	 	 = new Unote.Config.Debug();
			this.config.stroke 		 = new Unote.Config.Stroke();
			this.state        		 = new Unote.Controller.State();
			this.cache        		 = new Unote.Controller.Cache();
			this.store        		 = new Unote.Controller.Store();

			this.timer               = new System.Timers.Timer();
			this.timer.Interval      = this.config.stroke.REFRESH_INTERVAL;
			this.timer.Elapsed      += new ElapsedEventHandler(this.AddNewStrokePoint);

			this.cache.strokePoints  = new StrokePointSet();
			this.cache.strokes       = new StrokeSet();
			this.store.strokes       = new StrokeSet();

			if (this.READ_CONFIG) {
				this.paintMode      = this.config.stroke.PAINT_MODE;
				this.brushSize      = this.config.stroke.BRUSH_SIZE;
				this.brushColor     = this.config.stroke.BRUSH_COLOR;
				this.lerpStrokePath = this.config.stroke.LERP_SP_PATH;
				this.minSPDistance  = this.config.stroke.MIN_SP_DISTANCE;
			}
				
			this.LoadBrushGeometrySquare(this.config.stroke.BRUSH_SIZE);
			//this.LoadBrushGeometryHexagon(this.config.stroke.BRUSH_SIZE);

			this.SetIntersectionPlane(
				new Vector3(0.0f, 0.0f, 1.0f),
				new Vector3(0.0f, 0.0f, 4.0f)
			);

			if (this.TEST_STROKE) {
				this.TEST__generateStrokePoints();
			}

			this.IS_LOADED = true;
		}


		/*[HideInInspector]
		public bool LOCK = false;
		[HideInInspector]
		public bool button_0 = false;*/

		private void Update() {
			if (this.IS_LOADED) {
				if (this.config.debug.DRAW_IA_GEOMETRY) {
					this.OnDebugDraw();
				}

				if (this.paintMode == Unote.Config.Mode.MOUSE && InputExt.MouseIsMoved()) {
					transform.position = this.UpdateBrushPosition(Input.mousePosition);
					this.UpdateDrawingDirection();
				}

				if (this.paintMode == Unote.Config.Mode.SPACE) {
					this.cache.userBrushPointerPosition = transform.position;
					this.UpdateDrawingDirection();
				}

				if (Input.GetKeyDown(KeyCode.PageDown)){ //Presenter Key
					this.state.btn_0 = !this.state.btn_0;
				}

				if (Input.GetMouseButton(0)     ||
					//Input.GetKey(KeyCode.Space) ||
					this.state.btn_0) {
					this.Draw();
				}

				if (!Input.GetMouseButton(0)     &&
					//!Input.GetKey(KeyCode.Space) &&
					!this.state.btn_0) {
					this.StopDrawing();
					if (this.cache.strokePoints.Count > 1) {
						this.GenerateStroke(true);
					} else {
						this.ClearStrokePointCache();
					}
				}

				if (this.state.addStrokePoint) {
					GameObject strokeGameObject = new GameObject();
					strokeGameObject.name = "StrokePoint[" +
						this.cache.userBrushPointerPosition.x.ToString("0.000") + ", " +
						this.cache.userBrushPointerPosition.y.ToString("0.000") + ", " +
						this.cache.userBrushPointerPosition.z.ToString("0.000") + "]";
					this.cache.strokePoints.Add(new Unote.StrokePoint(strokeGameObject, this.cache.userBrushPointerPosition));
					if (this.cache.strokePoints.Count > 1) {
						this.GenerateStroke(false);
					}
					this.state.addStrokePoint = false;
				}

				if (this.TEST_STROKE) {
					TEST__generateStrokePoints();
				}

				if (this.paintMode != this.config.stroke.PAINT_MODE) {
					this.config.stroke.PAINT_MODE = this.paintMode;
				}

                transform.GetComponentInParent<Renderer> ().material.color = this.brushColor;
                //transform.root.GetChild(1).GetChild(1).GetComponent<ParticleSystem>().startColor = this.brushColor; //TODO: SetColor

                /*if (transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color != this.brushColor) {
					transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = this.brushColor;
				}*/
            }
        }

		private void OnDrawGizmos() {
			Gizmos.color = Color.green;

			if (this.config != null && this.cache.strokePoints != null) {
				if (this.config.debug.DRAW_STROKEPOINTS) {
					foreach (Unote.StrokePoint strokePoint in this.cache.strokePoints) {
						Gizmos.DrawSphere(strokePoint.gameObject.transform.position, 0.1f);
					}

					foreach (Unote.Stroke stroke in this.cache.strokes) {
						foreach (Unote.StrokePoint strokePoint in stroke.strokePoints) {
							Gizmos.DrawSphere(strokePoint.gameObject.transform.position, 0.1f);
						}
					}
				}
			}
		}

		private void TEST__generateStrokePoints() {
			if (this.TEST_STROKE) {
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(0.0f, 0.0f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(1.0f, 0.5f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(2.0f, 1.0f, 1.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(3.0f, 1.4f, 2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(4.0f, 1.3f, 3.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(5.0f, 1.0f, 2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(6.0f, 0.5f, 1.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(7.0f, -0.5f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(8.0f, -1.5f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(9.0f, -1.8f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(10.0f, -1.8f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(11.0f, -1.0f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(12.0f, 0.0f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(12.0f, 1.0f, -2.0f))); //
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(11.5f, 2.0f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(10.5f, 3.0f, -2.0f)));
				this.cache.strokePoints.Add(new Unote.StrokePoint(new GameObject(), new Vector3(9.5f, 4.0f, -2.0f)));
				this.GenerateStroke(true, true);
			}
			this.TEST_STROKE = !this.TEST_STROKE;
		}
	}
}