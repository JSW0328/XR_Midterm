using UnityEngine;

// 바닥에 떨어진 아이템. 플레이어가 닿으면 효과를 주고 사라진다
// 붙일 곳: 아이템 프리팹 (골드 동전, 활, 폭탄 모양). Collider의 Is Trigger를 켜야 한다
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public enum PickupType { Gold, Bow, Bomb }

    public PickupType type = PickupType.Gold;
    public int goldAmount = 20;          // 골드 아이템이 주는 양 (무기를 이미 가졌을 때도 이만큼 줌)
    public float lifeTime = 20f;         // 이 시간 안에 안 주우면 사라짐 (0이면 안 사라짐)
    public AudioClip pickupSound;        // 주울 때 소리 (비워 둬도 됨)

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject root = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!root.CompareTag("Player")) return;

        PlayerWeapons weapons = root.GetComponent<PlayerWeapons>();
        string msg = "";

        switch (type)
        {
            case PickupType.Gold:
                if (GameManager.Instance != null) GameManager.Instance.AddGold(goldAmount);
                msg = "골드 +" + goldAmount;
                break;

            case PickupType.Bow:
                if (weapons != null && !weapons.bowUnlocked)
                {
                    weapons.Unlock(PlayerWeapons.WeaponType.Bow);
                    msg = "활을 얻었어요! [2]번으로 사용";
                }
                else
                {
                    if (GameManager.Instance != null) GameManager.Instance.AddGold(goldAmount);
                    msg = "이미 활이 있어서 골드 +" + goldAmount;
                }
                break;

            case PickupType.Bomb:
                if (weapons != null && !weapons.bombUnlocked)
                {
                    weapons.Unlock(PlayerWeapons.WeaponType.Bomb);
                    msg = "폭탄을 얻었어요! [3]번으로 사용";
                }
                else
                {
                    if (GameManager.Instance != null) GameManager.Instance.AddGold(goldAmount);
                    msg = "이미 폭탄이 있어서 골드 +" + goldAmount;
                }
                break;
        }

        if (GameManager.Instance != null) GameManager.Instance.ShowMessage(msg, 2f);
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }
}
