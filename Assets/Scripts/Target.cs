using UnityEngine;
using DG.Tweening;

// Target con HP que se mueve hacia el jugador (en -Z).
// VFX: spawnea SIEMPRE en el punto del impacto (sin offsets random) y fuerza Play().
public class Target : MonoBehaviour
{
    [Header("Stats")]
    public int baseHp = 2;
    public float baseMoveSpeed = 6f;

    [Header("Rewards")]
    public int scoreOnBreak = 10;

    [Header("FX")]
    public AudioClip sfxHit;
    public AudioClip sfxBreak;
    public GameObject vfxHitPrefab;
    public GameObject vfxBreakPrefab;

    private int hp;
    private float moveSpeed;
    private bool breaking;
    private PooledObject pooled;

    void OnEnable()
    {
        pooled = GetComponent<PooledObject>();

        transform.DOKill();
        transform.localScale = Vector3.one;

        breaking = false;

        int lvl = (GameManager.I != null) ? GameManager.I.GetLevel() : 1;

        hp = baseHp + Mathf.FloorToInt((lvl - 1) * 0.5f);
        hp = Mathf.Clamp(hp, 2, 12);

        moveSpeed = baseMoveSpeed + (lvl - 1) * 0.25f;
        moveSpeed = Mathf.Clamp(moveSpeed, 6f, 14f);
    }

    void Update()
    {
        if (breaking) return;
        transform.position += Vector3.back * (moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (breaking) return;

        Projectile proj = other.GetComponent<Projectile>();
        if (proj == null) return;

        // Devolver proyectil al pool
        PooledObject projPooled = other.GetComponent<PooledObject>();
        if (projPooled != null && projPooled.pool != null) projPooled.pool.Return(other.gameObject);
        else other.gameObject.SetActive(false);

        hp--;

        AudioManager.I?.PlaySfx(sfxHit, 1f);
        SpawnVfx(vfxHitPrefab, transform.position);

        if (hp <= 0)
        {
            Break();
        }
        else
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.10f, 0.10f, 6, 1f)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }
    }

    private void Break()
    {
        breaking = true;

        transform.DOKill();

        transform.DOShakeScale(0.25f, 0.35f, 10, 90f)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .OnComplete(() =>
            {
                AudioManager.I?.PlaySfx(sfxBreak, 1f);
                SpawnVfx(vfxBreakPrefab, transform.position);

                GameManager.I?.AddScore(scoreOnBreak);

                if (pooled != null && pooled.pool != null) pooled.pool.Return(gameObject);
                else gameObject.SetActive(false);
            });
    }

    // Spawnea en worldPos (sin offsets random) y fuerza Play() a todos los ParticleSystems hijos.
    private void SpawnVfx(GameObject prefab, Vector3 worldPos)
    {
        if (prefab == null) return;

        GameObject v = Instantiate(prefab, worldPos, Quaternion.identity);

        // Asegura que arranca aunque Play On Awake esté mal
        ParticleSystem[] systems = v.GetComponentsInChildren<ParticleSystem>(true);

        // Duración real aproximada
        float maxLife = 0.5f;

        if (systems != null && systems.Length > 0)
        {
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem ps = systems[i];
                if (ps == null) continue;

                ps.Play(true);

                var main = ps.main;
                float delay = GetMaxCurveValue(main.startDelay);
                float lifetime = GetMaxCurveValue(main.startLifetime);
                float total = delay + main.duration + lifetime;

                if (total > maxLife) maxLife = total;
            }
        }
        else
        {
            maxLife = 2.0f;
        }

        Destroy(v, maxLife + 0.2f);
    }

    private float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant: return curve.constant;
            case ParticleSystemCurveMode.TwoConstants: return curve.constantMax;
            case ParticleSystemCurveMode.Curve: return curve.curve != null ? curve.curve.Evaluate(1f) : 0f;
            case ParticleSystemCurveMode.TwoCurves: return curve.curveMax != null ? curve.curveMax.Evaluate(1f) : 0f;
        }
        return 0f;
    }

    void OnDisable()
    {
        transform.DOKill();
    }
}
