using System.Collections.Generic;
using UnityEngine;

// 던지는 폭탄. 일정 시간이 지나거나 좀비에 닿으면 터져서 주변에 피해를 주고 물체를 날려 보낸다
// 붙일 곳: 폭탄 프리팹 (Sphere + Rigidbody, Use Gravity 켜기)
[RequireComponent(typeof(Rigidbody))]
public class Bomb : MonoBehaviour
{
    public float fuseTime = 1.5f;        // 던진 뒤 터지기까지 시간
    public float radius = 4f;            // 폭발 범위
    public int damage = 3;               // 범위 안 좀비에게 주는 피해
    public float explosionForce = 600f;  // 주변 물체를 날려 보내는 힘
    public GameObject explosionEffect;   // 폭발 파티클 (Standard Assets의 Explosion 추천)
    public AudioClip explosionSound;     // 폭발 소리 (3D로 재생됨)
    [Range(0f, 1f)] public float soundVolume = 1f;  // 폭발 소리 크기
    public float soundMinDistance = 10f; // 이 거리 안에서는 최대 크기로 들림 (클수록 멀리서도 크게)
    public float soundMaxDistance = 60f; // 이 거리 밖에서는 안 들림

    bool exploded;

    void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    // 던진 플레이어 몸에 부딪히지 않도록 충돌을 무시한다
    public void IgnoreOwner(GameObject owner)
    {
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null || owner == null) return;
        foreach (Collider c in owner.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(myCollider, c);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 좀비에 직접 맞으면 바로 터진다
        if (collision.collider.GetComponentInParent<Health>() != null) Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        HashSet<Health> damaged = new HashSet<Health>();
        HashSet<Rigidbody> pushed = new HashSet<Rigidbody>();
        Rigidbody myRb = GetComponent<Rigidbody>();

        foreach (Collider c in hits)
        {
            // 좀비마다 피해는 한 번만
            Health h = c.GetComponentInParent<Health>();
            if (h != null && damaged.Add(h)) h.TakeDamage(damage);

            // 상자, 통 같은 물리 물체를 날려 보낸다 (플레이어는 제외)
            Rigidbody rb = c.attachedRigidbody;
            if (rb != null && rb != myRb && !rb.CompareTag("Player") && pushed.Add(rb))
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius, 1f);
            }
        }

        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 4f);
        }
        if (explosionSound != null) PlaySound();

        Destroy(gameObject);
    }

    // 폭발 위치에 3D 소리를 낸다. 가까울수록 크게, 멀수록 작게 들린다
    void PlaySound()
    {
        GameObject go = new GameObject("ExplosionSound");
        go.transform.position = transform.position;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = explosionSound;
        src.volume = soundVolume;
        src.spatialBlend = 1f;                          // 3D 소리
        src.rolloffMode = AudioRolloffMode.Linear;      // 거리에 따라 일정하게 작아짐
        src.minDistance = soundMinDistance;
        src.maxDistance = soundMaxDistance;
        src.Play();
        Destroy(go, explosionSound.length + 0.1f);      // 소리가 끝나면 지운다
    }
}
