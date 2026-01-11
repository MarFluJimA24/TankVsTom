// Assets/Scripts/SimplePool.cs
using System.Collections.Generic;
using UnityEngine;

// Pool simple para evitar Instantiate/Destroy en bucles endless.
public class SimplePool : MonoBehaviour
{
    [Header("Pool Setup")]
    public GameObject prefab;
    public int initialSize = 20;

    // Cola de objetos disponibles.
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        // Pre-crea objetos para rendimiento estable en móvil.
        for (int i = 0; i < initialSize; i++)
        {
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);

            // Garantiza que el objeto sabe a qué pool pertenece.
            PooledObject po = go.GetComponent<PooledObject>();
            if (po == null) po = go.AddComponent<PooledObject>();
            po.pool = this;

            pool.Enqueue(go);
        }
    }

    // Obtiene un objeto del pool (o crea uno si no quedan).
    public GameObject Get()
    {
        GameObject go = (pool.Count > 0) ? pool.Dequeue() : Instantiate(prefab, transform);

        PooledObject po = go.GetComponent<PooledObject>();
        if (po == null) po = go.AddComponent<PooledObject>();
        po.pool = this;

        go.SetActive(true);
        return go;
    }

    // Devuelve un objeto al pool.
    public void Return(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}

// Componente auxiliar para devolver objetos a su pool.
public class PooledObject : MonoBehaviour
{
    [HideInInspector] public SimplePool pool;
}
