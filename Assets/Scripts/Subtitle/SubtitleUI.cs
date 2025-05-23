using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Utils;

namespace Subtitle
{
    [DoNotDestroySingleton]
    public class SubtitleUI : Singleton<SubtitleUI>
    {
    
        private TMP_Text _subtitleText;
        private CanvasGroup _canvasGroup;
        private AudioSource _audioSource;

        private SubtitleSequence _sequence;
        private List<int> _entryIndices;
        private Coroutine _coroutine;
        private float _targetAlpha;

        protected override void Awake()
        {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
            _subtitleText = GetComponentInChildren<TMP_Text>();
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        private void Update()
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * 2f);
        }

        public static void LoadSequence(SubtitleSequence sequence)
        {
            Instance._sequence = sequence;
            Instance._entryIndices = new List<int>(new int[sequence.entries.Count]);
        }
        
        public static void TriggerEvent(SubtitleEvent triggeredEvent)
        {
            if (Instance._sequence == null || Instance._entryIndices == null) return;

            for (int i = 0; i < Instance._sequence.entries.Count; i++)
            {
                var entry = Instance._sequence.entries[i];
                int stepIndex = Instance._entryIndices[i];
                
                if (stepIndex >= entry.steps.Count) continue;
                
                var step = entry.steps[stepIndex];
                if (SubtitleUtils.Matches(step.subtitleEvent, triggeredEvent))
                {
                    if (Instance._coroutine != null) Instance.StopCoroutine(Instance._coroutine);

                    Instance._coroutine = Instance.StartCoroutine(Instance.PlayStep(step));
                    Instance._entryIndices[i]++;
                    return;
                }
            }
        }

        private IEnumerator PlayStep(SubtitleStep step)
        {
            if (_canvasGroup.alpha > 0)
            {
                _targetAlpha = 0;
                yield return new WaitUntil(() => _canvasGroup.alpha == 0);
            }

            _subtitleText.text = step.text;
            if (step.audioClip != null) _audioSource.PlayOneShot(step.audioClip);
            _targetAlpha = 1;
            
            yield return Utils.Utils.WaitForAll(this,
                new WaitUntil(() => !_audioSource.isPlaying),
                Utils.Utils.WaitForSeconds(step.duration));
            
            _targetAlpha = 0;
            _coroutine = null;
        }
    }
}
