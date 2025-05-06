using System;
using UnityEngine;

namespace Animation
{
    public class AnimatorEventDispatcher : MonoBehaviour
    {
        public event Action<int> OnAnimatorIKUpdate;
        public event Action OnAnimatorMoveUpdate;

        private void OnAnimatorIK(int layerIndex)
        {
            OnAnimatorIKUpdate?.Invoke(layerIndex);
        }

        private void OnAnimatorMove()
        {
            OnAnimatorMoveUpdate?.Invoke();
        }
    }
}