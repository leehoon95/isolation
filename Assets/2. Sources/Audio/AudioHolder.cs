using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public interface IAudioPlayable
{
	IAudioHolderPool Pool { get; set; }
	GameObject GO { get; }
	void Play(AudioResource resource);
}

public class AudioHolder : MonoBehaviour, IAudioPlayable
{
	[SerializeField]
	AudioSource _audioSource;

	public IAudioHolderPool Pool { get; set; }
	public GameObject GO => gameObject;

	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	public void Play(AudioResource ar)
	{
		_audioSource.resource = ar;
		StartCoroutine(PlayAudio());
	}

	IEnumerator PlayAudio()
	{
		_audioSource.Play();
		yield return null;
		
		while (_audioSource.isPlaying)
		{
			yield return null;
		}

		Pool.Release(this);
	}
}
