using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 게임 전체 상태(골드, 성 체력, 화면 글자, 게임 오버)를 관리하는 스크립트
// 붙일 곳: 씬에 빈 오브젝트 "GameManager"를 만들고 여기에 붙인다 (씬에 딱 1개)
public class GameManager : MonoBehaviour
{
    // 다른 스크립트에서 GameManager.Instance 로 바로 접근할 수 있게 해 준다
    public static GameManager Instance;

    [Header("시작 값")]
    public int startGold = 0;        // 처음 가진 골드
    public int castleMaxHp = 20;     // 성 최대 체력

    [Header("UI 연결 (Canvas 안의 Text를 끌어다 놓기)")]
    public Text goldText;            // 골드 표시
    public Text castleText;          // 성 체력 표시
    public Text weaponText;          // 현재 무기 표시
    public Text messageText;         // 안내 문구 (소환 안내 등)
    public Text playerHpText;        // 플레이어 체력 표시
    public GameObject gameOverPanel; // 게임 오버 때 보여 줄 패널 (처음엔 꺼 둔다)

    public int Gold { get; private set; }
    public int CastleHp { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;   // 재시작했을 때 멈춘 시간을 다시 흐르게
        Gold = startGold;
        CastleHp = castleMaxHp;
        IsGameOver = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        SetMessage("");
        RefreshUI();
    }

    void Update()
    {
        // 게임 오버 상태에서 R 키를 누르면 처음부터 다시
        // (File > Build Settings 에서 이 씬을 Add Open Scenes 로 추가해 둬야 한다)
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // 골드를 더한다 (좀비를 잡았을 때)
    public void AddGold(int amount)
    {
        Gold += amount;
        RefreshUI();
    }

    // 골드가 충분하면 쓰고 true, 부족하면 false
    public bool TrySpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        RefreshUI();
        return true;
    }

    // 골드를 잃는다 (플레이어가 쓰러졌을 때). 0 아래로는 내려가지 않음
    public void LoseGold(int amount)
    {
        Gold = Mathf.Max(0, Gold - amount);
        RefreshUI();
    }

    public void SetPlayerHpText(string text)
    {
        if (playerHpText != null) playerHpText.text = text;
    }

    // 성 체력을 깎는다 (좀비가 성문에 도착했을 때)
    public void DamageCastle(int amount)
    {
        if (IsGameOver) return;
        CastleHp = Mathf.Max(0, CastleHp - amount);
        RefreshUI();
        if (CastleHp <= 0) GameOver();
    }

    public void SetWeaponText(string text)
    {
        if (weaponText != null) weaponText.text = text;
    }

    public void SetMessage(string text)
    {
        messageTimer = 0f;
        if (messageText != null) messageText.text = text;
    }

    // 몇 초 동안만 보여 주고 사라지는 안내 문구 (아이템 획득 등)
    public void ShowMessage(string text, float seconds)
    {
        if (messageText != null) messageText.text = text;
        messageTimer = seconds;
    }

    float messageTimer;

    void LateUpdate()
    {
        if (messageTimer <= 0f) return;
        messageTimer -= Time.unscaledDeltaTime;
        if (messageTimer <= 0f && messageText != null) messageText.text = "";
    }

    void GameOver()
    {
        IsGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;   // 게임을 멈춘다
    }

    void RefreshUI()
    {
        if (goldText != null) goldText.text = "골드: " + Gold;
        if (castleText != null) castleText.text = "성 체력: " + CastleHp + " / " + castleMaxHp;
    }
}
