using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Utils;

namespace UI
{
    public class SubtitleUI : Singleton<SubtitleUI>
    {
    
        private TMP_Text _subtitleText;
        private CanvasGroup _canvasGroup;
        private AudioSource _audioSource;

        private SubtitleSequence _currentSequence;
        private int _sequenceIndex;
        private float _targetAlpha;

        private Queue<SubtitleStep> _subtitleQueue = new();
        private Coroutine _queueCoroutine;
        private Coroutine _subStepCoroutine;

        protected override void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _subtitleText = GetComponentInChildren<TMP_Text>();
        }

        private void Update()
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * 2f);
        }

        public static void LoadSequence(SubtitleSequence sequence)
        {
            Instance._currentSequence = sequence;
            Instance._sequenceIndex = 0;
            Instance._subtitleQueue.Clear();
        }

        public static SubtitleStep GetNextEvent()
        {
            return Instance._currentSequence.Steps[Instance._sequenceIndex++];
        }
        
        public static void TriggerEvent(SubtitleEvent subtitleEvent)
        {
            if (Instance._currentSequence == null || Instance._sequenceIndex >= Instance._currentSequence.Steps.Length) return;
            
            var step = Instance._currentSequence.Steps[Instance._sequenceIndex];
            if (step.subtitleEvent != subtitleEvent) return;

            if (Instance._subStepCoroutine != null) Instance.StopCoroutine(Instance._subStepCoroutine);
            Instance._subStepCoroutine = null;
            
            Instance._subtitleQueue.Enqueue(step);
            Instance._sequenceIndex++;
            
            if (Instance._queueCoroutine == null)
                Instance._queueCoroutine = Instance.StartCoroutine(Instance.ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            while (_subtitleQueue.Count > 0)
            {
                var step = _subtitleQueue.Dequeue();
                yield return StartCoroutine(PlaySubtitle(step));
            }
            
            _queueCoroutine = null;
        }

        private IEnumerator PlaySubtitle(SubtitleStep step)
        {
            yield return new WaitForSeconds(step.delay);
            
            _subtitleText.text = step.text;
            if (step.audioClip != null) _audioSource.PlayOneShot(step.audioClip);
            _targetAlpha = 1;
            
            yield return Utils.Utils.WaitForAll(this,
                new WaitUntil(() => !_audioSource.isPlaying),
                Utils.Utils.WaitForSeconds(step.duration));
            
            _targetAlpha = 0;

            if (step.hasSubStep)
            {
                _subStepCoroutine = StartCoroutine(PlaySubStep(step));
            }
        }

        private IEnumerator PlaySubStep(SubtitleStep step)
        {
            yield return new WaitForSeconds(step.subStepDelay);
            
            _subtitleText.text = step.subStepText;
            if (step.audioClip != null) _audioSource.PlayOneShot(step.subStepAudioClip);

            _targetAlpha = 1;

            yield return Utils.Utils.WaitForAll(this,
                new WaitUntil(() => !_audioSource.isPlaying),
                Utils.Utils.WaitForSeconds(step.subStepDuration));

            _targetAlpha = 0;
            
            _subStepCoroutine = null;
        }
    }
}
