using UnityEngine;

// Disparo automático constante, sin taps.
// Muzzle VFX: soporta ParticleSystem en escena o prefab asset.
public class AutoShooter : MonoBehaviour
{
    [Header("Shooting")]
    public Transform muzzle;
    public float fireRate = 0.15f;

    [Header("Projectile")]
    public SimplePool projectilePool;
    public float projectileSpeed = 20f;
    public float projectileLifeTime = 2.0f;

    [Header("FX")]
    public AudioClip sfxShoot;
    [Range(0f, 1f)] public float shootVolume = 0.35f;

    // Puede ser un ParticleSystem en escena (ideal) o un prefab asset (también lo soportamos).
    public ParticleSystem vfxShoot;

    private float nextFire;

    void Update()
    {
        if (Time.time >= nextFire)
        {
            Fire();
            nextFire = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        if (projectilePool == null) return;

        GameObject go = projectilePool.Get();
        if (go == null) return;

        if (!go.activeSelf) go.SetActive(true);

        Transform m = (muzzle != null) ? muzzle : transform;

        go.transform.position = m.position;
        go.transform.rotation = m.rotation;

        Projectile proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.speed = projectileSpeed;
            proj.lifeTime = projectileLifeTime;
        }

        if (sfxShoot != null)
            AudioManager.I?.PlaySfx(sfxShoot, shootVolume);

        PlayMuzzleVfx(m);
    }

    private void PlayMuzzleVfx(Transform m)
    {
        if (vfxShoot == null) return;

        // Si el ParticleSystem pertenece a una escena, se puede mover y reproducir directamente.
        if (vfxShoot.gameObject.scene.IsValid())
        {
            vfxShoot.transform.SetPositionAndRotation(m.position, m.rotation);
            vfxShoot.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            vfxShoot.Play(true);
            return;
        }

        // Si NO pertenece a una escena, es un asset/prefab. Hay que instanciarlo.
        GameObject v = Instantiate(vfxShoot.gameObject, m.position, m.rotation);

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
            maxLife = 1.5f;
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
