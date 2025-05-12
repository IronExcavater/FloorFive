using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Level
{
    public class Timer : MonoBehaviour
    {

        
        private List<TMP_Text> _texts;
        private Room _room;

        private void Awake()
        {
            _room = GameObject.FindGameObjectWithTag("Room").GetComponent<Room>();
            _texts = GetComponentsInChildren<TMP_Text>().ToList();
        }

        void Update()
        {
            int minutes = Mathf.FloorToInt(_room.remainingTime / 60);
            int seconds = Mathf.FloorToInt(_room.remainingTime % 60);
            string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);
            _texts.ForEach(text => text.text = formattedTime);
        }
    }
}
