using System.Collections.Generic;
using UnityEngine;

// 플레이어의 무기 3종류. 숫자키 1, 2, 3으로 바꾸고 마우스 왼쪽 버튼(또는 F 키)으로 공격한다
// 1: 검(주변 근접 공격)  2: 활(화살 발사)  3: 폭탄(던지면 범위 폭발)
// 조준: 마우스로 카메라(FreeLookCameraRig)를 돌리고, 화면 가운데(조준점)를 향해 공격한다
// 붙일 곳: 플레이어 (ThirdPersonController)
public class PlayerWeapons : MonoBehaviour
{
    public enum WeaponType { Sword, Bow, Bomb }

    [Header("공통")]
    public KeyCode attackKey = KeyCode.F;   // 공격 키 (마우스 왼쪽 버튼도 가능)
    public Transform attackPoint;           // 화살, 폭탄이 나가는 위치 (비우면 가슴 앞에서 자동)
    public bool aimWithCamera = true;       // true: 화면 가운데를 조준 / false: 캐릭터가 보는 방향
    public float aimDistance = 100f;        // 조준점이 아무것도 안 맞을 때 쓰는 거리
    public bool bowUnlocked = true;         // false면 2번 무기 사용 불가 (아이템으로 해금할 때 사용)
    public bool bombUnlocked = true;        // false면 3번 무기 사용 불가

    [Header("1. 검 (근접)")]
    public int swordDamage = 1;
    public float swordRange = 1.8f;         // 공격 범위 (반지름)
    public float swordCooldown = 0.5f;      // 다음 공격까지 기다리는 시간
    public GameObject swordEffect;          // 휘두를 때 파티클 (비워 둬도 됨)
    public AudioClip swordSound;

    [Header("2. 활 (원거리)")]
    public Projectile arrowPrefab;          // 화살 프리팹 (Projectile 붙은 것)
    public float arrowSpeed = 25f;
    public float bowCooldown = 0.8f;
    public AudioClip bowSound;

    [Header("3. 폭탄 (범위)")]
    public Bomb bombPrefab;                 // 폭탄 프리팹 (Bomb 붙은 것)
    public float throwForward = 8f;         // 앞으로 던지는 힘
    public float throwUp = 5f;              // 위로 던지는 힘
    public float bombCooldown = 3f;
    public AudioClip throwSound;

    WeaponType current = WeaponType.Sword;
    float nextAttackTime;

    void Start()
    {
        UpdateWeaponText();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // 무기 바꾸기
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(WeaponType.Sword);
        if (Input.GetKeyDown(KeyCode.Alpha2) && bowUnlocked) Select(WeaponType.Bow);
        if (Input.GetKeyDown(KeyCode.Alpha3) && bombUnlocked) Select(WeaponType.Bomb);

        // 공격
        bool attackPressed = Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(0);
        if (attackPressed && Time.time >= nextAttackTime) Attack();

        UpdateWeaponText();
    }

    void Select(WeaponType type)
    {
        current = type;
    }

    // 아이템을 주웠을 때 다른 스크립트가 불러서 무기를 해금한다
    public void Unlock(WeaponType type)
    {
        if (type == WeaponType.Bow) bowUnlocked = true;
        if (type == WeaponType.Bomb) bombUnlocked = true;
        Select(type);
    }

    void Attack()
    {
        // 공격하기 전에 몸을 조준 방향(수평)으로 돌린다
        Vector3 aimPoint = GetAimPoint();
        FaceTowards(aimPoint);

        switch (current)
        {
            case WeaponType.Sword:
                SwordAttack();
                nextAttackTime = Time.time + swordCooldown;
                break;
            case WeaponType.Bow:
                if (arrowPrefab == null) return;
                ShootArrow(aimPoint);
                nextAttackTime = Time.time + bowCooldown;
                break;
            case WeaponType.Bomb:
                if (bombPrefab == null) return;
                ThrowBomb();
                nextAttackTime = Time.time + bombCooldown;
                break;
        }
    }

    // 화면 가운데에서 앞으로 레이저를 쏴서, 처음 맞는 곳을 조준점으로 쓴다 (내 몸은 무시)
    Vector3 GetAimPoint()
    {
        Camera cam = Camera.main;
        if (!aimWithCamera || cam == null)
        {
            return AttackOrigin() + transform.forward * aimDistance;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, aimDistance, ~0, QueryTriggerInteraction.Ignore);

        float closest = float.MaxValue;
        Vector3 point = ray.GetPoint(aimDistance);
        foreach (RaycastHit h in hits)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;   // 플레이어 자신은 건너뜀
            if (h.distance < closest)
            {
                closest = h.distance;
                point = h.point;
            }
        }
        return point;
    }

    // 위아래는 빼고, 좌우로만 조준점 쪽을 바라보게 한다
    void FaceTowards(Vector3 point)
    {
        Vector3 flat = point - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(flat);
    }

    // 공격이 나가는 위치: attackPoint가 있으면 거기, 없으면 가슴 높이 앞쪽
    Vector3 AttackOrigin()
    {
        if (attackPoint != null) return attackPoint.position;
        return transform.position + Vector3.up * 1.2f + transform.forward * 0.8f;
    }

    void SwordAttack()
    {
        // 캐릭터 앞쪽에 보이지 않는 구를 만들고, 그 안의 좀비에게 피해를 준다
        Vector3 center = transform.position + Vector3.up * 1f + transform.forward * (swordRange * 0.6f);
        Collider[] hits = Physics.OverlapSphere(center, swordRange);
        HashSet<Health> damaged = new HashSet<Health>();
        foreach (Collider c in hits)
        {
            Health h = c.GetComponentInParent<Health>();
            if (h != null && damaged.Add(h)) h.TakeDamage(swordDamage);
        }

        if (swordEffect != null)
        {
            GameObject fx = Instantiate(swordEffect, center, Quaternion.identity);
            Destroy(fx, 2f);
        }
        if (swordSound != null) AudioSource.PlayClipAtPoint(swordSound, transform.position);
    }

    void ShootArrow(Vector3 aimPoint)
    {
        Vector3 origin = AttackOrigin();
        Vector3 dir = (aimPoint - origin).normalized;
        Projectile arrow = Instantiate(arrowPrefab, origin, Quaternion.LookRotation(dir));
        arrow.IgnoreOwner(gameObject);
        arrow.Launch(dir * arrowSpeed);
        if (bowSound != null) AudioSource.PlayClipAtPoint(bowSound, origin);
    }

    void ThrowBomb()
    {
        Vector3 origin = AttackOrigin();
        Bomb bomb = Instantiate(bombPrefab, origin, Quaternion.identity);
        bomb.IgnoreOwner(gameObject);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = transform.forward * throwForward + Vector3.up * throwUp;
        if (throwSound != null) AudioSource.PlayClipAtPoint(throwSound, origin);
    }

    void UpdateWeaponText()
    {
        if (GameManager.Instance == null) return;

        string weaponName = "검";
        if (current == WeaponType.Bow) weaponName = "활";
        if (current == WeaponType.Bomb) weaponName = "폭탄";

        float wait = nextAttackTime - Time.time;
        string state = wait > 0f ? wait.ToString("0.0") + "초" : "준비됨";

        string list = "[1]검";
        if (bowUnlocked) list += " [2]활";
        if (bombUnlocked) list += " [3]폭탄";

        GameManager.Instance.SetWeaponText("무기: " + weaponName + " (" + state + ")   " + list);
    }
}
