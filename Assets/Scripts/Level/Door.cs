using Animation;
using Audio;
using UnityEngine;
using Utils;

namespace Level
{
	public class Door : Interactable
	{
		[SerializeField] private bool isOpen;
		[SerializeField] private float openAngle = -90;
		[SerializeField] private float closedAngle = 0;
		
		private AudioSource _audioSource;

		protected override void Awake()
		{
			base.Awake();
			_audioSource = GetComponentInChildren<AudioSource>();
		}

		protected override void Interact()
		{
			AnimationManager.RemoveTweens(this);
			isOpen = !isOpen;
			_audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue(isOpen ? "doorOpen" : "doorClose").GetFirstClip());
			
			AnimationManager.CreateTween(this, rotation => transform.localRotation = rotation,
				transform.localRotation, Quaternion.Euler(0, isOpen ? openAngle : closedAngle, 0),
				1, Easing.EaseOutCubic);
		}
	}
}