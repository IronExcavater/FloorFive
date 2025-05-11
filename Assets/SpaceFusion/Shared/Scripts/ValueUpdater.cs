using UnityEngine;

namespace SpaceFusion.Shared.Scripts {

    /// <summary>
    /// Handles basic value updating between start and end value over a defined duration
    /// You can use Activate to update from start to end value and Deactivate to update from end to start value
    /// It tracks the status in a protected boolean "inProgress", to make sure that we do not activate or deactivate the change twice in a row
    /// </summary>
    public class ValueUpdater : MonoBehaviour {

        public float startValue;
        public float endValue = 1f;
        protected float currentValue;
        protected float elapsedTime;
        protected float duration;
        protected bool inProgress;

        private float _start;
        private float _end;

        public virtual void Activate(float activationDuration) {
            duration = activationDuration;
            _start = startValue;
            _end = endValue;
            currentValue = _start;
            elapsedTime = 0;
            inProgress = true;
        }

        public virtual void Deactivate(float shutDownDuration) {
            duration = shutDownDuration;
            _start = endValue;
            _end = startValue;
            currentValue = _start;
            elapsedTime = 0;
            inProgress = true;
        }


        /// <summary>
        /// Handles basic linear value updates
        /// </summary>
        protected float GetUpdatedValue() {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            currentValue = Mathf.Lerp(_start, _end, t);
            if (elapsedTime >= duration) {
                currentValue = _end;
            }

            return currentValue;
        }

        /// <summary>
        /// Handles value updates based on an animation curve
        /// </summary>
        /// <param name="animCurve"></param>
        protected float GetUpdatedValue(AnimationCurve animCurve) {
            // Increase the elapsed time by the time passed since the last frame
            elapsedTime += Time.deltaTime;
            // Calculate the fraction of the duration that has passed
            var t = elapsedTime / duration;
            // Evaluate the curve at t to get the curve's influence
            var curveValue = animCurve.Evaluate(t);
            // Lerp between startValue and endValue based on t
            currentValue = Mathf.Lerp(_start, _end, curveValue);
            // Check if the duration has been reached
            if (elapsedTime >= duration) {
                // Ensure the final value is exactly endValue
                currentValue = _end;
            }

            return currentValue;
        }

    }
}