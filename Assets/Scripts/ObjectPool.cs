using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly Queue<T> _pool = new();
    private readonly T _prefab;
    private readonly Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (var i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        var obj = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}