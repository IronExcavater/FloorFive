using UnityEngine;

namespace Tools
{
    public abstract class ToolBase : MonoBehaviour
    {
        public Vector3 toolOffsetPosition;
        public Vector3 toolOffsetRotation;
        public Vector3 handOffsetPosition;
        public Vector3 handOffsetRotation;

        public abstract void OnInteract();
    }
}