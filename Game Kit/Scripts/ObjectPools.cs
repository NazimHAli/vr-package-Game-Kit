using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPools : MonoBehaviour
{
    #region Variables:

    public static ObjectPools Instance;

    [NonReorderable]
    public List<Pool> pools = new List<Pool>();

    private Dictionary<string, Pool> namedPools = new Dictionary<string, Pool>();

    /// <summary>
    /// Temporary GameObject used when populating pools.
    /// </summary>
    private GameObject tmp;

    #endregion

    #region Initialization & Updates:

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        CreatePools();
    }

    #endregion

    #region Core Functions:

    public void CreatePools()
    {
        foreach (Pool pool in pools)
        {
            if (!namedPools.ContainsKey(pool.prefab.name))
            {
                PopulatePool(pool);
                namedPools.Add(pool.prefab.name, pool);
            }
            else
            {
                Debug.LogWarning("|Object Pool|: Failed to create pool (" + pool.prefab.name + ") a pool already exisits for that prefab, concider renaming.");
            }
        }
        tmp = null;
    }

    public void PopulatePool(Pool pool)
    {
        if (pool.pooledObjects.Count < pool.amount)
        {
            for (int i = 0; i < pool.amount; i++)
            {
                tmp = Instantiate(pool.prefab);
                tmp.SetActive(false);
                tmp.gameObject.hideFlags = pool.hideFlags;
                pool.pooledObjects.Add(tmp);
            }
        }
    }

    public void ExpandPool(Pool pool, int amount)
    {
        pool.amount += amount;
        PopulatePool(pool);
    }

    /// <summary>
    /// Destroys all objects in all pools.
    /// </summary>
    /// <param name="omitActive">Preserve pooled GameObjects that are currently active in the scene.</param>
    public void DestroyPools(bool omitActive)
    {
        foreach (Pool pool in pools)
        {
            for (int i = pool.pooledObjects.Count; i > 0; i--)
            {
                if (omitActive == false)
                {
                    Destroy(pool.pooledObjects[i]);
                }
                else
                {
                    if (pool.pooledObjects[i].activeSelf == false)
                    {
                        Destroy(pool.pooledObjects[i]);
                    }
                }
            }

            pool.pooledObjects.Clear();
        }
    }

    public void ActivatePooledObject(int index)
    {
        if (index <= pools.Count)
        {
            for (int i = 0; i < pools[index].amount; i++)
            {
                if (!pools[index].pooledObjects[i].activeInHierarchy)
                {
                    pools[index].pooledObjects[i].SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogWarning("|Object Pool|: failed to activate GameObject from pool " + index + " GameObject will be null, make sure the index is within range.");
        }
    }

    public void ActivatePooledObject(string prefabName)
    {
        if (!string.IsNullOrEmpty(prefabName) && namedPools.ContainsKey(prefabName))
        {
            for (int i = 0; i < namedPools[prefabName].amount; i++)
            {
                if (!namedPools[prefabName].pooledObjects[i].activeInHierarchy)
                {
                    namedPools[prefabName].pooledObjects[i].SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogWarning("|Object Pool|: failed to activate GameObject from pool (" + name + ") GameObject will be null, make sure the name is correct.");
        }
    }

    /// <summary>
    /// Gets a pooled GameObject by the index of its pool.
    /// </summary>
    /// <param name="index">The index of a pool in <see cref="pools"/>.</param>
    /// <returns>The first inactive prefab instance.</returns>
    public GameObject GetPooledObject(int index)
    {
        if (index <= pools.Count)
        {
            for (int i = 0; i < pools[index].amount; i++)
            {
                if (!pools[index].pooledObjects[i].activeInHierarchy)
                {
                    return pools[index].pooledObjects[i];
                }
            }
        }
        else
        {
            Debug.LogWarning("|Object Pool|: failed to get GameObject from pool " + index + " GameObject will be null, make sure the index is within range.");
        }

        return null;
    }

    /// <summary>
    /// Gets a pooled GameObject by its prefab name.
    /// </summary>
    /// <param name="prefabName">The name of the prefab to get an instance of.</param>
    /// <returns>The first inactive prefab instance.</returns>
    public GameObject GetPooledObject(string prefabName)
    {
        if (!string.IsNullOrEmpty(prefabName) && namedPools.ContainsKey(prefabName))
        {
            for (int i = 0; i < namedPools[prefabName].amount; i++)
            {
                if (!namedPools[prefabName].pooledObjects[i].activeInHierarchy)
                {
                    return namedPools[prefabName].pooledObjects[i];
                }
            }
        }
        else
        {
            Debug.LogWarning("|Object Pool|: failed to get GameObject from pool (" + name + ") GameObject will be null, make sure the name is correct.");
        }

        return null;
    }

    #endregion
}

[Serializable]
public struct Pool
{
    [Tooltip("The GameObject to pool.")]
    public GameObject prefab;
    [Tooltip("The initial amount of GameObjects to instantiate.")]
    public int amount;
    [Tooltip("HideFlags for prefabs instantiated via the pooling system. Useful for hiding pooled objects in the heirarchy.")]
    public HideFlags hideFlags;
    [Tooltip("The GameObjects currently pooled.")]
    public List<GameObject> pooledObjects;
}