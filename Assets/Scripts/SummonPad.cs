using UnityEngine;

// 포탑 설치 발판. 플레이어가 올라가서 E 키를 누르면 골드를 쓰고 자동 공격 포탑을 설치한다
// 붙일 곳: 발판 오브젝트 (Cylinder를 납작하게 + Collider의 Is Trigger 체크)
//          발판 콜라이더는 위로 조금 두껍게 만들어야 플레이어가 들어온 걸 감지한다
[RequireComponent(typeof(Collider))]
public class SummonPad : MonoBehaviour
{
    public GameObject allyPrefab;       // 설치할 포탑 프리팹
    public Transform spawnPoint;        // 포탑이 나타날 위치 (비우면 발판 위)
    public int cost = 60;               // 설치 비용 (골드)
    public KeyCode summonKey = KeyCode.E;
    public int maxAllies = 1;           // 이 발판에 설치할 수 있는 최대 수
    public AudioClip summonSound;       // 설치 소리 (비워 둬도 됨)
    public GameObject summonEffect;     // 설치 파티클 (비워 둬도 됨)

    int count;
    bool playerInside;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        if (!playerInside) return;
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;
        if (!Input.GetKeyDown(summonKey)) return;

        if (count >= maxAllies)
        {
            GameManager.Instance.ShowMessage("이미 포탑이 설치된 자리예요", 2f);
            return;
        }
        if (!GameManager.Instance.TrySpendGold(cost))
        {
            GameManager.Instance.ShowMessage("골드가 부족해요 (필요: " + cost + ")", 2f);
            return;
        }

        Summon();
    }

    void Summon()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        Instantiate(allyPrefab, pos, rot);
        count++;

        if (summonEffect != null)
        {
            GameObject fx = Instantiate(summonEffect, pos, Quaternion.identity);
            Destroy(fx, 3f);
        }
        if (summonSound != null) AudioSource.PlayClipAtPoint(summonSound, pos);

        GameManager.Instance.ShowMessage("포탑 설치 완료!", 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = true;
        if (GameManager.Instance != null)
        {
            if (count >= maxAllies) GameManager.Instance.SetMessage("이미 포탑이 설치된 자리예요");
            else GameManager.Instance.SetMessage("[" + summonKey + "] 포탑 설치 (" + cost + " 골드)");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = false;
        if (GameManager.Instance != null) GameManager.Instance.SetMessage("");
    }

    bool IsPlayer(Collider other)
    {
        GameObject root = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        return root.CompareTag("Player");
    }
}
