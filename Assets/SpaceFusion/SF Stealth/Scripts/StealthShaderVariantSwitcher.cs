using System;
using UnityEngine;

namespace SpaceFusion.SF_Stealth.Scripts {
    public class StealthShaderVariantSwitcher : MonoBehaviour {

        public GameObject[] variants;


        private int index = 0;
        private int maxIndex;

        private void Start() {
            variants[index].SetActive(true);
            maxIndex = variants.Length - 1;
        }
        

        public void Next() {
            variants[index].SetActive(false);
            var updated = GetUpdatedIndex(true);
            variants[updated].SetActive(true);
        }

        public  void Previous() {
            variants[index].SetActive(false);
            var updated = GetUpdatedIndex(false);
            variants[updated].SetActive(true);
        }


        private int GetUpdatedIndex(bool up) {
            if (up) {
                index++;
                if (index > maxIndex) {
                    index = 0;
                }
            } else {
                index--;
                if (index < 0) {
                    index = maxIndex;
                }
            }

            return index;
        }

    }
}