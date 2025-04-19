using UnityEngine;

public class Mirror : MonoBehaviour
{
    private Transform _camTrans;
    private Vector3 _offset;
    
    private void Awake()
    {
        _camTrans = Camera.main.transform;
        _offset = _camTrans.eulerAngles - transform.eulerAngles;
    }
    private void Update()
    {
        transform.rotation = Quaternion.Euler(_camTrans.eulerAngles - _offset * -1f);
    }
}
