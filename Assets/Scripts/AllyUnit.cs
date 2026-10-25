using UnityEngine;

// 소환된 아군(궁수 탑). 범위 안에서 가장 가까운 좀비를 찾아 자동으로 화살을 쏜다
// 붙일 곳: 아군 프리팹 (예: Cylinder 몸통 + Sphere 머리로 만든 탑)
public class AllyUnit : MonoBehaviour
{
    public float range = 12f;            // 공격 범위
    public float attackInterval = 1f;    // 몇 초마다 쏠지
    public Projectile projectilePrefab;  // 쏠 화살 프리팹 (Use Gravity 꺼 둔 것)
    public float projectileSpeed = 20f;
    public Transform firePoint;          // 화살이 나가는 위치 (비우면 몸 위쪽에서 자동)
    public Transform turnPart;           // 적을 향해 돌아갈 부분, 예: 머리 (비워 둬도 됨)
    public AudioClip shootSound;         // 쏠 때 소리 (3D로 재생됨)

    float timer;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer -= Time.deltaTime;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        // 좀비의 가슴 높이를 조준
        Vector3 aimPoint = target.position + Vector3.up * 1f;

        // 머리를 좀비 쪽으로 돌린다 (위아래는 그대로)
        if (turnPart != null)
        {
            Vector3 flat = aimPoint - turnPart.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f) turnPart.rotation = Quaternion.LookRotation(flat);
        }

        if (timer > 0f || projectilePrefab == null) return;
        timer = attackInterval;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 2f;
        Vector3 dir = (aimPoint - origin).normalized;

        Projectile p = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
        p.IgnoreOwner(gameObject);
        p.Launch(dir * projectileSpeed);

        if (shootSound != null) AudioSource.PlayClipAtPoint(shootSound, origin);
    }

    // 범위 안에서 가장 가까운 Enemy 태그 오브젝트를 찾는다
    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float best = range * range;

        foreach (GameObject e in enemies)
        {
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d <= best)
            {
                best = d;
                nearest = e.transform;
            }
        }
        return nearest;
    }

    // Scene 뷰에서 공격 범위를 노란 원으로 보여 준다 (게임 화면에는 안 보임)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
