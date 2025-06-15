using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITextPooling : MonoBehaviour
{
    public bool CollectionChecks = true;
    public int MaxPoolSize = 10;

    [SerializeField] private UIText uiTextPrefab; // Reference to the UIText prefab

    private UnityEngine.Pool.ObjectPool<UIText> _pool;

    private void Awake()
    {

    }

    public UnityEngine.Pool.ObjectPool<UIText> Pool
    {
        get
        {
            if (_pool == null)
            {
                _pool = new UnityEngine.Pool.ObjectPool<UIText>(CreatePooledItem, OnTakeFromPool,
                    OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
            }

            return _pool;
        }
    }

    private UIText CreatePooledItem()
    {

        UIText itemGhostInstance = Instantiate(uiTextPrefab);
        return itemGhostInstance;
    }

    // Called when an item is returned to the pool using Release
    private void OnReturnedToPool(UIText data)
    {
        data.gameObject.SetActive(false);
        data.ResetDamageText();
    }

    private void OnTakeFromPool(UIText data)
    {
        data.ResetDamageText(); // Ensure all properties are reset
        data.gameObject.SetActive(true);
    }


    private void OnDestroyPoolObject(UIText data)
    {
        if (data != null)
            Destroy(data.gameObject);
    }
}