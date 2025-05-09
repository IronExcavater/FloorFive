using Animation;
using Audio;
using UnityEngine;
using Utils;

namespace Level
{
	[RequireComponent(typeof(AudioSource))]
	public class Door : Interactable
	{
		[Header("Door")]
		[SerializeField] private bool isOpen;
		[SerializeField] private float openAngle = 0;
		[SerializeField] private float closedAngle = 90;
		
		private Quaternion _startRotation;
		
		private AudioSource _audioSource;

		protected override void Awake()
		{
			base.Awake();
			_audioSource = GetComponentInChildren<AudioSource>();
			_startRotation = transform.localRotation;
		}

		protected override void Interact()
		{
			AnimationManager.RemoveTweens(this);
			isOpen = !isOpen;
			_audioSource.Stop();
			_audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue(isOpen ? "doorOpen" : "doorClose").GetFirstClip());
			
			AnimationManager.CreateTween(this, rotation => transform.localRotation = rotation,
				transform.localRotation,
				_startRotation * Quaternion.Euler(0, isOpen ? openAngle : closedAngle, 0),
				1, Easing.EaseOutCubic);
		}
	}
}