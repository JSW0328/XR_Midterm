using UnityEngine;

// 정육면체의 모서리 12개에 얇은 막대를 만들어 윤곽선을 그린다 (게임을 실행하면 생성됨)
// 붙일 곳: Crystal_Cube (정육면체)
public class CubeEdges : MonoBehaviour
{
    public Material edgeMaterial;        // 윤곽선 색 (Mat_CrystalEdge)
    [Range(0.005f, 0.1f)]
    public float thickness = 0.02f;      // 윤곽선 두께 (정육면체 크기 1 기준)

    void Start()
    {
        float h = 0.5f;                  // 정육면체 중심에서 모서리까지 거리
        float len = 1f + thickness;      // 모서리끼리 빈틈 없게 살짝 길게

        // X 방향 모서리 4개
        for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
                MakeEdge(new Vector3(0f, y * h, z * h), new Vector3(len, thickness, thickness));

        // Y 방향 모서리 4개
        for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                MakeEdge(new Vector3(x * h, 0f, z * h), new Vector3(thickness, len, thickness));

        // Z 방향 모서리 4개
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                MakeEdge(new Vector3(x * h, y * h, 0f), new Vector3(thickness, thickness, len));
    }

    void MakeEdge(Vector3 localPos, Vector3 localScale)
    {
        GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edge.name = "Edge";
        Destroy(edge.GetComponent<Collider>());      // 부딪히지 않게

        edge.transform.SetParent(transform, false);  // 정육면체를 따라 같이 돌게
        edge.transform.localPosition = localPos;
        edge.transform.localRotation = Quaternion.identity;
        edge.transform.localScale = localScale;

        if (edgeMaterial != null) edge.GetComponent<Renderer>().sharedMaterial = edgeMaterial;
    }
}
