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

	void Play(AudioResource resource, float time);
	void Play(AudioClip clip);
}

public class AudioHolder : MonoBehaviour, IAudioPlayable
{
	[SerializeField]
	AudioSource _audioSource;

	Coroutine _task;
	string _audioName;
	long _playTime;

	public IAudioHolderPool Pool { get; set; }
	public GameObject GO => gameObject;
	public string AudioName { get; set; }
	public long PlayTime => _playTime;

	void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void Play(AudioResource resource, float time)
	{
		if (_task != null)
		{
			return;
		}

		_audioSource.resource = resource;
		_task = StartCoroutine(PlayAudio(time));
	}

	public void Play(AudioClip clip)
	{
		if (_task != null)
		{
			return;
		}

		_audioSource.clip = clip;
		_task = StartCoroutine(PlayAudio(clip.length));
	}

	IEnumerator PlayAudio(float time)
	{
		_playTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		yield return null;
		_audioSource.Play();
		yield return new WaitForSeconds(time);
		
		_task = null;
		Pool.Release(this);
	}
}
