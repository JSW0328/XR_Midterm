using UnityEngine;

// 좀비의 체력. 체력이 0이 되면 골드를 주고, 확률에 따라 아이템을 떨구고 사라진다
// 붙일 곳: 좀비 프리팹 (태그도 Enemy 로 설정)
public class Health : MonoBehaviour
{
    // 떨굴 아이템 하나와 그 확률
    [System.Serializable]
    public class Drop
    {
        public GameObject itemPrefab;            // 떨굴 아이템 프리팹
        [Range(0f, 1f)] public float chance;     // 확률 (0.3 = 30%)
    }

    public int maxHp = 3;             // 최대 체력
    public int goldReward = 10;       // 처치하면 바로 받는 골드
    public GameObject deathEffect;    // 죽을 때 나올 파티클 (비워 둬도 됨)
    public AudioClip deathSound;      // 죽을 때 소리 (비워 둬도 됨)

    [Header("아이템 떨구기 (위에서부터 순서대로 확률을 굴려서 하나만 떨굼)")]
    public Drop[] drops;

    int currentHp;
    bool isDead;

    void Awake()
    {
        currentHp = maxHp;
    }

    // 무기, 화살, 폭탄이 이 함수를 불러서 피해를 준다
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHp -= amount;
        if (currentHp <= 0) Die();
    }

    void Die()
    {
        isDead = true;

        if (GameManager.Instance != null) GameManager.Instance.AddGold(goldReward);

        if (deathEffect != null)
        {
            GameObject fx = Instantiate(deathEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(fx, 3f);
        }

        // 그 자리에서 3D 소리 재생 (가까울수록 크게 들림)
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position);

        DropItem();
        Destroy(gameObject);
    }

    void DropItem()
    {
        if (drops == null) return;

        foreach (Drop d in drops)
        {
            if (d == null || d.itemPrefab == null) continue;
            if (Random.value <= d.chance)
            {
                Instantiate(d.itemPrefab, transform.position + Vector3.up * 0.7f, Quaternion.identity);
                return;   // 하나만 떨군다
            }
        }
    }
}
