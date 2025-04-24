using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorCache : MonoBehaviour
{
    private Dictionary<string, int> _hashes;
    
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_hashes != null) return;
        
        _hashes = new Dictionary<string, int>();
        var ani = GetComponent<Animator>();
        foreach (var parameter in ani.parameters)
            _hashes.Add(parameter.name, parameter.nameHash);
    }
    
    public int GetHash(string parameterName)
    {
        Initialize();
        return _hashes.TryGetValue(parameterName, out var hash) ? hash : throw new KeyNotFoundException(parameterName);
    }
}