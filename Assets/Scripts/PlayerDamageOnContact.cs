using UnityEngine;

// Quita vida cuando un Target entra en el hitbox del jugador.
// VFX: spawnea en el punto del contacto (sin offsets random) y fuerza Play().
public class PlayerDamageOnContact : MonoBehaviour
{
    [Header("FX")]
    public AudioClip sfxPlayerHit;
    public GameObject vfxPlayerHitPrefab;

    [Header("Behavior")]
    public float hitCooldown = 0.25f;

    private float nextAllowedHitTime = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < nextAllowedHitTime) return;

        Target target = other.GetComponent<Target>();
        if (target == null) return;

        nextAllowedHitTime = Time.time + hitCooldown;

        GameManager.I?.LoseLife(sfxPlayerHit);

        SpawnVfx(vfxPlayerHitPrefab, other.transform.position);

        PooledObject po = other.GetComponent<PooledObject>();
        if (po != null && po.pool != null) po.pool.Return(other.gameObject);
        else other.gameObject.SetActive(false);
    }

    private void SpawnVfx(GameObject prefab, Vector3 worldPos)
    {
        if (prefab == null) return;

        GameObject v = Instantiate(prefab, worldPos, Quaternion.identity);

        ParticleSystem[] systems = v.GetComponentsInChildren<ParticleSystem>(true);
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
}
