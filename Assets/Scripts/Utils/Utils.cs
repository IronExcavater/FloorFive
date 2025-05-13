using System;
using System.Collections;
using UnityEngine;

namespace Utils
{
    public static class Utils
    {
        public static Vector3 ClosestPointOnLine(Vector3 linePoint, Vector3 lineDirection, Vector3 point, float length = float.MaxValue)
        {
            if (length == float.MaxValue && lineDirection.normalized != lineDirection) 
                length = lineDirection.magnitude;

            lineDirection.Normalize();

            return linePoint + lineDirection * ClosestDistanceOnLine(linePoint, lineDirection, point, length);
        }

        public static float ClosestDistanceOnLine(Vector3 linePoint, Vector3 lineDirection, Vector3 point, float length = float.MaxValue)
        {
            if (length == float.MaxValue && lineDirection.normalized != lineDirection)
                length = lineDirection.magnitude;

            lineDirection.Normalize();

            return Mathf.Clamp(Vector3.Dot(point - linePoint, lineDirection), 0, length);
        }

        public static IEnumerator WaitForAll(MonoBehaviour runner, params IEnumerator[] coroutines)
        {
            int count = coroutines.Length;
            foreach (var coroutine in coroutines)
            {
                runner.StartCoroutine(WaitForComplete(runner, coroutine, () => count--));
            }
            
            while (count > 0) yield return null;
        }

        public static IEnumerator WaitForComplete(MonoBehaviour runner, IEnumerator coroutine, Action onComplete)
        {
            yield return runner.StartCoroutine(coroutine);
            onComplete?.Invoke();
        }

        public static IEnumerator WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
        
        public static Bounds GetLocalBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Quaternion originalRotation = obj.transform.rotation;
            obj.transform.rotation = Quaternion.identity;
            
            Bounds combined = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                combined.Encapsulate(renderers[i].bounds);

            Vector3 localCenter = obj.transform.InverseTransformPoint(combined.center);
            obj.transform.rotation = originalRotation;
            localCenter = obj.transform.rotation * localCenter;
            localCenter = Vector3.Scale(localCenter, obj.transform.localScale);
            combined.center = localCenter;
            
            return combined;
        }

        /// <summary>
        /// Doesn't change the objects transform, but is rough and works for things at runtime.
        /// </summary>
        /// <returns></returns>
        public static Bounds GetBoundsRough(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);
            
            Bounds combined = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                combined.Encapsulate(renderers[i].bounds);
            
            return combined;
        }
        
        public static void SetLayerRecursive(GameObject obj, int layer)
        {
            foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = layer;
            }
        }
    }
}