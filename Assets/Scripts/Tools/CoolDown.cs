using UnityEngine;

[System.Serializable]
public class CoolDown
{
    [SerializeField] public float coolDownTime;
    private float coolDownTimer;

    public bool coolDownActive
    {
        get { return Time.time < coolDownTimer; }
    }

    public void StartCoolDown()
    {
        coolDownTimer = Time.time + coolDownTime;
    }
}
