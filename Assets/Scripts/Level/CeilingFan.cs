using UnityEngine;

namespace Level
{
    public class CeilingFan : MonoBehaviour
    {
        public float turnSpeed;
        [SerializeField] private Transform ceilingFanPivot;

        private void Awake()
        {
            ceilingFanPivot.Rotate(Vector3.up, Random.Range(0f, 90f));
        }
        
        private void Update()
        {
            ceilingFanPivot.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
        }
    }
}
