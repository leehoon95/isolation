using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public interface IAudioPlayable
{
	IAudioHolderPool Pool { get; set; }
	GameObject GO { get; }
	public string AudioName { get; set; }
	public long PlayTime { get; }

	//void Play(AudioResource resource, float time);
	//void Play(AudioClip clip);
	void Play(AudioResource resource);
}

public class AudioHolder : MonoBehaviour, IAudioPlayable
{
	[SerializeField]
	AudioSource _audioSource;

	long _playTime;

	public IAudioHolderPool Pool { get; set; }
	public GameObject GO => gameObject;
	public string AudioName { get; set; }
	public long PlayTime => _playTime;

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
		_playTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		_audioSource.Play();
		yield return null;
		
		while (_audioSource.isPlaying)
		{
			yield return null;
		}

		Pool.Release(this);
	}
}
