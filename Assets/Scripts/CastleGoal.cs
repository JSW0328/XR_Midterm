using UnityEngine;

// 성문 앞의 트리거 구역. 좀비가 들어오면 성 체력을 깎고 좀비는 사라진다
// 붙일 곳: 성문 앞에 Cube를 놓고 Mesh Renderer 체크 해제, Box Collider의 Is Trigger 체크
//          좀비의 목표(castleTarget)가 이 상자 안에 들어가도록 크게 만든다
[RequireComponent(typeof(Collider))]
public class CastleGoal : MonoBehaviour
{
    public int damagePerZombie = 1;   // 좀비 한 마리가 도착하면 깎이는 성 체력
    public AudioClip hitSound;        // 성이 공격받는 소리 (비워 둬도 됨)

    // 컴포넌트를 처음 붙일 때 Is Trigger를 자동으로 켜 준다
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // 부딪힌 콜라이더의 주인(리지드바디가 붙은 최상위 오브젝트)을 찾는다
        GameObject root = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!root.CompareTag("Enemy")) return;

        if (GameManager.Instance != null) GameManager.Instance.DamageCastle(damagePerZombie);
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);

        Destroy(root);
    }
}
