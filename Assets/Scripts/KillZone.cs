using UnityEngine;

// Zona de muerte: si un Target llega aquí, se pierde una vida.
// Incluye SpawnVfx que respeta offset bakeado del prefab.
public class KillZone : MonoBehaviour
{
    [Header("FX")]
    public AudioClip sfxLoseLife;
    public GameObject vfxLoseLifePrefab;

    // Opcional: si ves varias pérdidas seguidas por entrar muchos targets casi a la vez.
    [Header("Safety")]
    public float globalCooldown = 0.0f;
    private float nextAllowedTime = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (globalCooldown > 0f && Time.time < nextAllowedTime) return;

        Target target = other.GetComponent<Target>();
        if (target == null) return;

        if (globalCooldown > 0f)
            nextAllowedTime = Time.time + globalCooldown;

        GameManager.I?.LoseLife(sfxLoseLife);

        SpawnVfx(vfxLoseLifePrefab, other.transform.position);

        // Devuelve el target al pool
        PooledObject po = other.GetComponent<PooledObject>();
        if (po != null && po.pool != null) po.pool.Return(other.gameObject);
        else other.gameObject.SetActive(false);
    }

    private void SpawnVfx(GameObject prefab, Vector3 worldPos)
    {
        if (prefab == null) return;

        GameObject v = Instantiate(prefab);

        Vector3 bakedOffset = v.transform.position;
        v.transform.position = worldPos + bakedOffset;

        Camera cam = Camera.main;
        if (cam != null)
            v.transform.LookAt(cam.transform);

        ParticleSystem[] systems = v.GetComponentsInChildren<ParticleSystem>(true);
        if (systems == null || systems.Length == 0)
        {
            Destroy(v, 2.0f);
            return;
        }

        float maxLife = 0.5f;

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

        Destroy(v, maxLife + 0.2f);
    }

    private float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.Curve:
                return curve.curve != null ? curve.curve.Evaluate(1f) : 0f;
            case ParticleSystemCurveMode.TwoCurves:
                return curve.curveMax != null ? curve.curveMax.Evaluate(1f) : 0f;
        }
        return 0f;
    }
}
