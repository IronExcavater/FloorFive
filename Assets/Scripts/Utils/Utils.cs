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
    }
}