using UnityEngine;

// 오브젝트를 제자리에서 천천히 돌리고, 위아래로 둥둥 띄운다
// 붙일 곳: 성의 수정(Keep_Crystal), 나중에 떨어진 아이템(골드, 활, 폭탄)
public class FloatSpin : MonoBehaviour
{
    public float spinSpeed = 40f;     // 1초에 도는 각도 (Y축 기준)
    public float bobHeight = 0.3f;    // 위아래로 움직이는 높이
    public float bobSpeed = 1.5f;     // 위아래로 움직이는 빠르기

    Vector3 startLocalPos;
    float offset;

    void Start()
    {
        startLocalPos = transform.localPosition;
        offset = Random.Range(0f, 10f);   // 여러 개가 똑같이 움직이지 않도록
    }

    void Update()
    {
        // 월드 Y축(위쪽)을 기준으로 회전
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        // 사인 곡선으로 부드럽게 위아래 이동
        float y = Mathf.Sin((Time.time + offset) * bobSpeed) * bobHeight;
        transform.localPosition = startLocalPos + new Vector3(0f, y, 0f);
    }
}
