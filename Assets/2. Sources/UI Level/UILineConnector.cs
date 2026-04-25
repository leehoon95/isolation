using UnityEngine;
using UnityEngine.UI;

public class UILineConnector : MaskableGraphic
{
	[Header("Line Settings")]
	public Vector2 startPoint;
	public Vector2 endPoint;
	public float thickness = 5f;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();

		// 두 점 사이의 각도와 방향 계산
		Vector2 direction = (endPoint - startPoint).normalized;
		Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);

		// 네 개의 정점(Vertex) 위치 계산
		Vector2 v1 = startPoint - normal;
		Vector2 v2 = startPoint + normal;
		Vector2 v3 = endPoint + normal;
		Vector2 v4 = endPoint - normal;

		// 메쉬 데이터 생성
		AddQuad(vh, v1, v2, v3, v4);
	}

	void AddQuad(VertexHelper vh, Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
	{
		int startIndex = vh.currentVertCount;

		UIVertex vertex = UIVertex.simpleVert;
		vertex.color = color;

		vertex.position = v1; vh.AddVert(vertex);
		vertex.position = v2; vh.AddVert(vertex);
		vertex.position = v3; vh.AddVert(vertex);
		vertex.position = v4; vh.AddVert(vertex);

		vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
		vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
	}


#if UNITY_EDITOR
	// 인스펙터에서 값을 수정할 때 즉시 반영되도록 설정
	protected override void OnValidate()
	{
		base.OnValidate();
		SetAllDirty();
	}
#endif
}