using UnityEngine;

namespace VENTUS
{
	namespace Interaction
	{
		namespace Sketching
		{
			public class Stroke : MonoBehaviour
			{
				private LineRenderer lineRenderer;

				private void Awake()
				{
					lineRenderer = GetComponent<LineRenderer>();
				}

				public void AddStrokePoint(Vector3 position)
				{
					lineRenderer.positionCount += 1;
					lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
				}

				public Vector3[] GetStrokePointPositions()
				{
					Vector3[] positions = new Vector3[lineRenderer.positionCount];

					if (lineRenderer.GetPositions(positions) > 0)
						return positions;
					else
						return null;
				}

				public void SetStrokePointPositions(Vector3[] positions)
				{
					lineRenderer.positionCount = positions.Length;
					lineRenderer.SetPositions(positions);
				}

				public Color GetStrokeColor()
				{
					return lineRenderer.material.color;
				}

				public void SetStrokeColor(Color color)
				{
					lineRenderer.material.color = color;
				}

				public void SetStrokeWidth(float width)
				{
					lineRenderer.widthMultiplier = width;
				}
			}
		}
	}
}
