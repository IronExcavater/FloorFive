using System;
using UnityEngine;

public class AnimatorIK : MonoBehaviour
{
    public event Action<int> OnAnimatorIKUpdate;
    
    private void OnAnimatorIK(int layerIndex)
    {
        OnAnimatorIKUpdate?.Invoke(layerIndex);
    }
}
