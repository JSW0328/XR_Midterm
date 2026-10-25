using UnityEngine;

// 좀비의 체력. 체력이 0이 되면 골드를 주고 사라진다
// 붙일 곳: 좀비 프리팹 (태그도 Enemy 로 설정)
public class Health : MonoBehaviour
{
    public int maxHp = 3;             // 최대 체력
    public int goldReward = 10;       // 처치하면 받는 골드
    public GameObject deathEffect;    // 죽을 때 나올 파티클 (비워 둬도 됨)
    public AudioClip deathSound;      // 죽을 때 소리 (비워 둬도 됨)

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

        Destroy(gameObject);
    }
}
