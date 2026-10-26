using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

// 좀비의 판단. 평소에는 중심 탑으로 가다가, 플레이어가 가까이 오면 쫓아가서 공격한다
// 이동 자체는 Standard Assets의 AICharacterControl(NavMesh)이 하고, 이 스크립트는 "누구를 쫓을지"만 정한다
// 붙일 곳: 좀비 (AIThirdPersonController로 만든 것)
[RequireComponent(typeof(AICharacterControl))]
public class ZombieAI : MonoBehaviour
{
    [Header("목표")]
    public Transform castleTarget;       // 평소 목표 (비우면 생성될 때 받은 목표나 CastleTarget을 자동으로 찾음)

    [Header("플레이어 추격")]
    public float chaseRange = 8f;        // 플레이어가 이 거리 안에 오면 쫓아감
    public float giveUpRange = 12f;      // 쫓다가 이 거리보다 멀어지면 포기하고 탑으로 돌아감

    [Header("공격")]
    public float attackRange = 1.6f;     // 이 거리 안이면 공격
    public int attackDamage = 1;         // 한 번에 깎는 플레이어 체력
    public float attackCooldown = 1.2f;  // 공격 간격 (초)
    public AudioClip attackSound;        // 공격 소리 (비워 둬도 됨)

    AICharacterControl ai;
    Transform player;
    PlayerHealth playerHealth;
    bool chasing;
    float nextAttackTime;

    void Start()
    {
        ai = GetComponent<AICharacterControl>();

        // 평소 목표 정하기: Inspector에 넣은 것 > 생성기가 정해 준 것 > 이름이 CastleTarget인 오브젝트
        if (castleTarget == null) castleTarget = ai.target;
        if (castleTarget == null)
        {
            GameObject t = GameObject.Find("CastleTarget");
            if (t != null) castleTarget = t.transform;
        }

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }

        ai.SetTarget(castleTarget);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        bool playerAvailable = player != null && (playerHealth == null || !playerHealth.IsDead);
        float dist = playerAvailable ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        // 쫓을지 말지 결정 (쫓기 시작 거리와 포기 거리를 다르게 해서 왔다갔다 하지 않게)
        if (!chasing && dist <= chaseRange) chasing = true;
        else if (chasing && dist > giveUpRange) chasing = false;
        if (!playerAvailable) chasing = false;

        Transform wanted = chasing ? player : castleTarget;
        if (ai.target != wanted) ai.SetTarget(wanted);

        // 가까우면 공격
        if (chasing && dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
            if (attackSound != null) AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
    }

    // Scene 뷰에서 추격 범위(빨강)와 공격 범위(노랑)를 보여 준다
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
