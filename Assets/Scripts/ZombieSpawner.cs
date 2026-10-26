using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

// 일정 시간마다 좀비를 만들어서 성문으로 걸어가게 한다. 시간이 갈수록 더 자주 나온다
// 붙일 곳: 씬에 빈 오브젝트 "ZombieSpawner"를 만들고 여기에 붙인다
public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;   // 좀비 프리팹 (AIThirdPersonController로 만든 것)
    public Transform[] spawnPoints;   // 좀비가 나타날 위치들 (빈 오브젝트 여러 개)
    public Transform castleTarget;    // 좀비가 걸어갈 목표 (성문 앞 빈 오브젝트)

    [Header("생성 간격")]
    public float startDelay = 3f;          // 게임 시작 후 첫 좀비까지 기다리는 시간
    public float startInterval = 5f;       // 처음 생성 간격 (초)
    public float minInterval = 1.5f;       // 가장 빠른 생성 간격
    public float intervalDecrease = 0.1f;  // 한 마리 나올 때마다 간격이 이만큼 줄어든다
    public int maxAlive = 15;              // 동시에 있을 수 있는 최대 좀비 수

    [Header("확인용")]
    public bool showLog = true;            // Console에 생성 기록을 보여 줄지 (다 되면 꺼도 됨)

    float timer;
    float currentInterval;
    int spawnCount;

    void Start()
    {
        currentInterval = startInterval;
        timer = startDelay;

        // 설정이 빠졌으면 바로 알려 준다
        if (zombiePrefab == null) Debug.LogWarning("[ZombieSpawner] Zombie Prefab이 비어 있어요.");
        if (spawnPoints == null || spawnPoints.Length == 0) Debug.LogWarning("[ZombieSpawner] Spawn Points가 비어 있어요.");
        if (castleTarget == null) Debug.LogWarning("[ZombieSpawner] Castle Target이 비어 있어요.");
        if (showLog) Debug.Log("[ZombieSpawner] 시작! " + startDelay + "초 뒤 첫 좀비가 나와요.");
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        // 다음 생성까지 시간 다시 맞추고, 간격은 조금씩 줄인다
        timer = currentInterval;
        currentInterval = Mathf.Max(minInterval, currentInterval - intervalDecrease);

        // 살아 있는 좀비 수 (체력 스크립트가 붙은 것만 센다)
        int alive = FindObjectsOfType<Health>().Length;
        if (alive >= maxAlive)
        {
            if (showLog) Debug.Log("[ZombieSpawner] 좀비가 " + alive + "마리라 이번엔 건너뛰어요.");
            return;
        }

        Spawn();
    }

    void Spawn()
    {
        if (zombiePrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (point == null)
        {
            Debug.LogWarning("[ZombieSpawner] Spawn Points 중에 빈 칸이 있어요.");
            return;
        }

        // 생성 위치를 NavMesh(걸을 수 있는 바닥) 위로 맞춘다
        Vector3 pos = point.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 5f, NavMesh.AllAreas)) pos = hit.position;
        else Debug.LogWarning("[ZombieSpawner] " + point.name + " 근처에 NavMesh(파란 바닥)가 없어요.");

        GameObject zombie = Instantiate(zombiePrefab, pos, point.rotation);
        spawnCount++;
        zombie.name = "Zombie_" + spawnCount;

        // Standard Assets의 AI 조종 스크립트에 목표(성문)를 알려 준다
        AICharacterControl ai = zombie.GetComponent<AICharacterControl>();
        if (ai != null) ai.SetTarget(castleTarget);

        if (showLog) Debug.Log("[ZombieSpawner] " + zombie.name + " 생성 (" + point.name + ")");
    }
}
