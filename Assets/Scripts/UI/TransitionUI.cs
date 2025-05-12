using System.Collections;
using Animation;
using UnityEngine;
using Utils;

namespace UI
{
    [DoNotDestroySingleton]
    public class TransitionUI : Singleton<TransitionUI>
    {
        private CanvasGroup _canvasGroup;
        [SerializeField] private float fadeDuration = 1f;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        public static IEnumerator FadeTransition(bool fadeIn)
        {
            Debug.Log("FadeTransition, fadeIn: " + fadeIn);
            AnimationManager.RemoveTweens(Instance);
            var fade = AnimationManager.CreateTween(Instance, alpha => Instance._canvasGroup.alpha = alpha,
                Instance._canvasGroup.alpha, fadeIn ? 0 : 1,
                Instance.fadeDuration, Easing.EaseInOutCubic);
            yield return new WaitUntil(() => !AnimationManager.HasTween(fade));
        }
    }
}