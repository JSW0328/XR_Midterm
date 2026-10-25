using UnityEngine;

// 날아가는 공격(화살). 좀비(Health)에 맞으면 피해를 주고 사라진다
// 붙일 곳: 화살 프리팹 (Rigidbody 필요, Use Gravity는 끄면 똑바로 날아감)
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public int damage = 2;            // 맞혔을 때 피해
    public float lifeTime = 5f;       // 아무것도 안 맞으면 이 시간 뒤 사라짐
    public GameObject hitEffect;      // 맞았을 때 파티클 (비워 둬도 됨)

    Rigidbody rb;
    bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 빠르게 날아가도 좀비를 통과하지 않도록
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Destroy(gameObject, lifeTime);
    }

    // 쏘는 쪽(플레이어, 아군)이 이 함수로 속도를 정해 준다
    public void Launch(Vector3 velocity)
    {
        rb.velocity = velocity;
        if (velocity.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(velocity);
    }

    // 쏜 사람 몸에 부딪혀 바로 사라지지 않도록 충돌을 무시한다
    public void IgnoreOwner(GameObject owner)
    {
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null || owner == null) return;
        foreach (Collider c in owner.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(myCollider, c);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Health health = collision.collider.GetComponentInParent<Health>();
        if (health != null) health.TakeDamage(damage);

        if (hitEffect != null)
        {
            GameObject fx = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Destroy(gameObject);
    }
}
