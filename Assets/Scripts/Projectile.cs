// Assets/Scripts/Projectile.cs
using UnityEngine;

// Proyectil: avanza en +Z y vuelve al pool al expirar.
public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed = 20f;
    [HideInInspector] public float lifeTime = 2f;

    private float t;
    private PooledObject pooled;

    void OnEnable()
    {
        t = 0f;
        pooled = GetComponent<PooledObject>();
    }

    void Update()
    {
        // Movimiento hacia delante (eje Z positivo).
        transform.position += Vector3.forward * (speed * Time.deltaTime);

        // Control de vida.
        t += Time.deltaTime;
        if (t >= lifeTime)
        {
            // Retorno al pool (o desactivar si no hay pool).
            if (pooled != null && pooled.pool != null) pooled.pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
