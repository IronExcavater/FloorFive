using SpaceFusion.SF_Stealth.Scripts;
using UnityEngine;

namespace Anomaly
{
    [RequireComponent(typeof(StealthController))]
    public class CloakAnomaly : AnomalyBase
    {
        private bool _found;
        private StealthController _stealth;

        protected override void Awake()
        {
            base.Awake();
            _stealth = GetComponent<StealthController>();
        }

        protected override void Activate(bool active)
        {
            base.Activate(active);
            _found = false;
            
            if (active)
            {
                _stealth.Activate(0);
            }
            else
            {
                _stealth.Deactivate(1);
            }
        }

        protected override void Grab()
        {
            if (_found) return;
            _found = true;
            _stealth.Deactivate(1);
        }
    }
}