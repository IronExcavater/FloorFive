 using Level;
 using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{

    [SerializeField] private TMP_Text timerText;

    private Room _room;

    private void Awake()
    {
        _room = GameObject.FindGameObjectWithTag("Room").GetComponent<Room>();
    }

    void Update()
    {
        int minutes = Mathf.FloorToInt(_room.remainingTime / 60);
        int seconds = Mathf.FloorToInt(_room.remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
