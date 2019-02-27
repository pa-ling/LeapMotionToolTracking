using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VENTUS
{
	namespace Interaction
	{
		namespace Sketching
		{
			public class SketchingController : NetworkBehaviour
			{
				[Header("Stroke Prefab")]
				public GameObject strokePrefab;
				[Header("Stroke Style")]
				public Color strokeColor = Color.white;
				public float strokeWidth = 0.05f;
				[Header("Stroke Configuration")]
				public float samplingRate = 0.01f;

				[HideInInspector]
				public List<Stroke> strokes;

				private Stroke currentStroke;

				private Vector3 virtualBrushPosition;
				private Vector3 lastVirtualBrushPosition;

				#region StartUp
				private void Start()
				{
					strokes = new List<Stroke>();
					currentStroke = null;
					virtualBrushPosition = Vector3.zero;
					lastVirtualBrushPosition = Vector3.zero;
				}
				#endregion

				#region OnDe-/Serialization
				public override bool OnSerialize(NetworkWriter writer, bool initialState)
				{
					if (initialState)
					{
						foreach (Stroke stroke in strokes)
						{
							writer.Write(stroke.GetStrokeColor());

							Vector3[] positions = stroke.GetStrokePointPositions();

							writer.WritePackedUInt32((uint)positions.Length);

							foreach (Vector3 position in positions)
								writer.Write(position);
						}

						return true;
					}
					return true;
				}

				public override void OnDeserialize(NetworkReader reader, bool initialState)
				{
					if (initialState)
					{
						while (reader.Position < reader.Length)
						{
							Color color = reader.ReadColor();

							uint positionCount = reader.ReadPackedUInt32();

							Vector3[] positions = new Vector3[positionCount];

							for (uint i = 0; i < positionCount; ++i)
								positions[i] = reader.ReadVector3();

							Stroke stroke = GenerateNewStroke();
							stroke.SetStrokeColor(color);
							stroke.SetStrokePointPositions(positions);
						}

						return;
					}
				}
				#endregion

				#region SetVirtualBrushPosition
				public void SetVirtualBrushPosition(Vector3 newPosition)
				{
					CmdSetVirtualBrushPosition(newPosition);
				}

				[Command]
				public void CmdSetVirtualBrushPosition(Vector3 newPosition)
				{
					RpcSetVirtualBrushPosition(newPosition);
				}

				[ClientRpc]
				public void RpcSetVirtualBrushPosition(Vector3 newPosition)
				{
					lastVirtualBrushPosition = virtualBrushPosition;
					virtualBrushPosition = newPosition;
				}
				#endregion

				#region Draw
				public void Draw()
				{
					CmdDraw();
				}

				[Command]
				public void CmdDraw()
				{
					RpcDraw();
				}

				[ClientRpc]
				public void RpcDraw()
				{
					if (currentStroke == null)
						currentStroke = GenerateNewStroke();

					if (Vector3.Distance(virtualBrushPosition, lastVirtualBrushPosition) >= samplingRate)
						currentStroke.AddStrokePoint(virtualBrushPosition);
				}
				#endregion

				#region StopDraw
				public void StopDraw()
				{
					CmdStopDraw();
				}

				[Command]
				public void CmdStopDraw()
				{
					RpcStopDraw();
				}

				[ClientRpc]
				public void RpcStopDraw()
				{
					currentStroke = null;
				}
				#endregion

				#region GenerateStroke
				private Stroke GenerateNewStroke()
				{
					GameObject newStroke = Instantiate(strokePrefab);

					Stroke strokeComponent = newStroke.GetComponent<Stroke>();
					strokeComponent.SetStrokeColor(strokeColor);
					strokeComponent.SetStrokeWidth(strokeWidth);

					strokes.Add(strokeComponent);
					//Session.strokes.Add(strokeComponent);

					return strokeComponent;
				}

				public Stroke GenerateNewStroke(Vector3[] strokePositions, Color color)
				{
					GameObject newStroke = Instantiate(strokePrefab);

					Stroke strokeComponent = newStroke.GetComponent<Stroke>();
					strokeComponent.SetStrokePointPositions(strokePositions);
					strokeComponent.SetStrokeColor(color);
					strokeComponent.SetStrokeWidth(strokeWidth);


					strokes.Add(strokeComponent);
					//Session.strokes.Add(strokeComponent);

					return strokeComponent;
				}
				#endregion
			}
		}
	}
}
