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

    [Header("승리 조건")]
    public float surviveSeconds = 180f;  // 이 시간(초) 동안 성을 지키면 승리 (기본 3분)
    public string missionText = "3분 동안 성을 지켜라!";  // 시작할 때 보여 줄 미션 문구

    [Header("UI 연결 (Canvas 안의 Text를 끌어다 놓기)")]
    public Text goldText;            // 골드 표시
    public Text castleText;          // 성 체력 표시
    public Text weaponText;          // 현재 무기 표시
    public Text messageText;         // 안내 문구 (소환 안내 등)
    public Text playerHpText;        // 플레이어 체력 표시
    public GameObject gameOverPanel; // 게임 오버 때 보여 줄 패널 (처음엔 꺼 둔다)
    public Text timerText;           // 남은 시간 표시
    public GameObject victoryPanel;  // 승리 때 보여 줄 패널 (처음엔 꺼 둔다)

    public int Gold { get; private set; }
    public int CastleHp { get; private set; }
    public bool IsGameOver { get; private set; }   // 게임이 끝났는지 (패배 또는 승리)
    public bool IsVictory { get; private set; }    // 승리로 끝났는지

    float timeLeft;   // 남은 시간 (초)

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;   // 재시작했을 때 멈춘 시간을 다시 흐르게
        Gold = startGold;
        CastleHp = castleMaxHp;
        IsGameOver = false;
        IsVictory = false;
        timeLeft = surviveSeconds;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        SetMessage("");
        RefreshUI();
        RefreshTimer();
    }

    void Start()
    {
        // 시작하자마자 미션 문구를 3초 동안 보여 준다
        if (!string.IsNullOrEmpty(missionText)) ShowMessage(missionText, 5f);
    }

    void Update()
    {
        // 게임 중이면 남은 시간을 줄이고, 0이 되면 승리
        if (!IsGameOver)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                Victory();
            }
            RefreshTimer();
        }

        // 게임이 끝난 상태(패배·승리)에서 R 키를 누르면 처음부터 다시
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
        SetMessage("");   // 안내 문구 지우기
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;   // 게임을 멈춘다
    }

    // 남은 시간을 지키면 승리: 승리 화면을 보여 주고 게임을 멈춘다
    void Victory()
    {
        IsGameOver = true;   // 다른 스크립트(좀비, 무기 등)가 멈추도록 같은 표시를 쓴다
        IsVictory = true;
        SetMessage("");
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // 남은 시간을 "남은 시간 2:31" 형태로 보여 준다
    void RefreshTimer()
    {
        if (timerText == null) return;
        int sec = Mathf.CeilToInt(timeLeft);
        timerText.text = "남은 시간 " + (sec / 60) + ":" + (sec % 60).ToString("00");
    }

    void RefreshUI()
    {
        if (goldText != null) goldText.text = "골드: " + Gold;
        if (castleText != null) castleText.text = "성 체력: " + CastleHp + " / " + castleMaxHp;
    }
}
