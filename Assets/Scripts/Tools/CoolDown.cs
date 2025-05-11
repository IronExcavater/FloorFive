using UnityEngine;

[System.Serializable]
public class CoolDown
{
    [SerializeField] public float coolDownTime;
    private float coolDownTimer;

    public bool coolDownActive
    {
        get { return Time.deltaTime < coolDownTimer; }
    }

    public void StartCoolDown()
    {
        coolDownTimer = Time.deltaTime + coolDownTime;
    }
}
