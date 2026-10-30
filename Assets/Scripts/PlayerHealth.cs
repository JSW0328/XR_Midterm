using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

// 플레이어 체력. 좀비에게 맞으면 줄고, 0이 되면 쓰러졌다가 잠시 뒤 성 안에서 부활한다
// 붙일 곳: 플레이어 (ThirdPersonController)
public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 5;                // 최대 체력
    public float respawnDelay = 3f;      // 쓰러진 뒤 부활까지 걸리는 시간 (초)
    public Transform respawnPoint;       // 부활 위치 (비우면 게임 시작 위치)
    public int goldPenalty = 10;         // 쓰러지면 잃는 골드
    public float invincibleTime = 1.5f;  // 부활 직후 무적 시간 (초)
    public AudioClip hurtSound;          // 맞을 때 소리 (비워 둬도 됨)

    public int CurrentHp { get; private set; }
    public bool IsDead { get; private set; }

    Vector3 startPosition;
    Quaternion startRotation;
    float respawnTimer;
    float invincibleUntil;

    Rigidbody rb;
    ThirdPersonUserControl userControl;
    ThirdPersonCharacter character;
    PlayerWeapons weapons;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        userControl = GetComponent<ThirdPersonUserControl>();
        character = GetComponent<ThirdPersonCharacter>();
        weapons = GetComponent<PlayerWeapons>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        CurrentHp = maxHp;
        RefreshUI();
    }

    void Update()
    {
        if (!IsDead) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;   // 게임 오버면 멈춤

        // 쓰러져 있는 동안은 제자리에 멈춰 있게 한다
        if (character != null) character.Move(Vector3.zero, false, false);

        respawnTimer -= Time.deltaTime;
        if (GameManager.Instance != null)
            GameManager.Instance.SetMessage("쓰러졌어요! " + Mathf.Max(0f, respawnTimer).ToString("0.0") + "초 뒤 성에서 부활");

        if (respawnTimer <= 0f) Respawn();
    }

    // 좀비가 이 함수를 불러서 피해를 준다
    public void TakeDamage(int amount)
    {
        if (IsDead || Time.time < invincibleUntil) return;

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        if (hurtSound != null) AudioSource.PlayClipAtPoint(hurtSound, transform.position);
        RefreshUI();

        if (CurrentHp <= 0) Die();
    }

    void Die()
    {
        IsDead = true;
        respawnTimer = respawnDelay;

        // 조종과 공격을 막는다
        if (userControl != null) userControl.enabled = false;
        if (weapons != null) weapons.enabled = false;

        if (GameManager.Instance != null) GameManager.Instance.LoseGold(goldPenalty);
    }

    void Respawn()
    {
        Vector3 pos = respawnPoint != null ? respawnPoint.position : startPosition;
        Quaternion rot = respawnPoint != null ? respawnPoint.rotation : startRotation;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.position = pos;
        }
        transform.SetPositionAndRotation(pos, rot);

        CurrentHp = maxHp;
        IsDead = false;
        invincibleUntil = Time.time + invincibleTime;

        if (userControl != null) userControl.enabled = true;
        if (weapons != null) weapons.enabled = true;

        if (GameManager.Instance != null) GameManager.Instance.SetMessage("");
        RefreshUI();
    }

    void RefreshUI()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetPlayerHpText("체력: " + CurrentHp + " / " + maxHp);
    }
}
