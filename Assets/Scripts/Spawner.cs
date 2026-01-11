// Assets/Scripts/Spawner.cs
using UnityEngine;

// Spawner endless: genera targets delante (Z alto) y con X aleatorio.
public class Spawner : MonoBehaviour
{
    [Header("Pool")]
    public SimplePool targetPool;

    [Header("Spawn Area")]
    public float minX = -6f;
    public float maxX = 6f;
    public float spawnY = 1f;
    public float spawnZ = 18f;

    [Header("Timing")]
    public float baseSpawnDelay = 0.75f;
    public float minSpawnDelay = 0.25f;

    private float nextSpawn;

    void Update()
    {
        if (targetPool == null) return;

        // Dificultad mínima: reducir delay según nivel.
        int lvl = (GameManager.I != null) ? GameManager.I.GetLevel() : 1;

        float delay = baseSpawnDelay - (lvl - 1) * 0.05f;
        delay = Mathf.Clamp(delay, minSpawnDelay, baseSpawnDelay);

        if (Time.time >= nextSpawn)
        {
            SpawnOne();
            nextSpawn = Time.time + delay;
        }
    }

    private void SpawnOne()
    {
        GameObject t = targetPool.Get();
        float x = Random.Range(minX, maxX);
        t.transform.position = new Vector3(x, spawnY, spawnZ);
    }
}
