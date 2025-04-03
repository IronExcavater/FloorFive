using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorCache : MonoBehaviour
{
    private readonly Dictionary<string, int> _hashes = new();
    
    private void Awake()
    {
        var ani = GetComponent<Animator>();
        foreach (var parameter in ani.parameters)
            _hashes.Add(parameter.name, parameter.nameHash);
    }
    
    public int GetHash(string parameterName)
    {
        return _hashes.TryGetValue(parameterName, out var hash) ? hash : throw new KeyNotFoundException(parameterName);
    }
}