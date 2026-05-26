using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class PathDefines
{
	// GCP VM 인스턴스(instance-20260324-084128) 외부 IP
	public static string RemoteLoadPath = "http://34.11.242.48";
}

public interface IAudioHolderPool
{
	void Release(IAudioPlayable ap);
}

public interface IAudioContainer
{
	void PlayAudio(string key, Vector2 position);
}

public class AudioContainer : MonoBehaviour, IAudioHolderPool, IAudioContainer
{
	[SerializeField]
	string _downloadAddress;
	[SerializeField]
	GameObject _audioHolderPrefab;

	public static AudioContainer Instance { get; private set; }

	public event UnityAction<long> AudioDownloadable;
	public event UnityAction<bool, float> AudioDownloadProgress;

	Coroutine _downloadCo;
	Coroutine _loadAssetsCo;
	ObjectPool<IAudioPlayable> _audioHolderPool;
	Dictionary<string, AudioClip> _audioClipPool = new();
	Dictionary<string, AudioResourceConfig> _audioResourceConfigPool = new();
	Dictionary<string, IAudioPlayable> _playingHolderList = new();
	List<IAudioPlayable> _releaseReservedList = new();

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		_audioHolderPool = new ObjectPool<IAudioPlayable>(
			createFunc: () =>
			{
				var obj = Instantiate(_audioHolderPrefab);
				var iap = obj.GetComponent<IAudioPlayable>();
				iap.Pool = this;

#if UNITY_EDITOR
				if (iap == null )
				{
					throw new InvalidOperationException("Invalid prefab");
				}
#endif
				return iap;
			},
			actionOnGet: (instance) =>
			{
				instance.Pool = this;
				instance.GO.SetActive(true);
			},
			actionOnRelease: (instance) =>
			{
				instance.GO.SetActive(false);
				_playingHolderList.Remove(instance.AudioName);
			},
			actionOnDestroy: (instance) =>
			{
				GLogger.Log("destroy holder");
				Destroy(instance.GO);
			}
			);

		StartCoroutine(CheckAudioUpdatableCo());
	}

	void FixedUpdate()
	{
		if (_releaseReservedList.Count > 0)
		{
			CollectHolder();
		}
	}

	void CollectHolder()
	{
		foreach (var holder in _releaseReservedList)
		{
			_audioHolderPool.Release(holder);
		}
		_releaseReservedList.Clear();
	}

	IEnumerator CheckAudioUpdatableCo()
	{
		yield return null;

		// 카탈로그 업데이트
		var checkCatalogHandle = Addressables.CheckForCatalogUpdates(true);
		yield return checkCatalogHandle;

		// Audio 라벨 에셋을 다운로드 받아야 하는지 확인
		var sizeHandle = Addressables.GetDownloadSizeAsync("Audio");
		yield return sizeHandle;

		if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
		{
			GLogger.Log($"Audio 다운로드 사이즈: {sizeHandle.Result}");
			if (sizeHandle.Result > 0)
			{
				AudioDownloadable?.Invoke(sizeHandle.Result);
			}
			else if (sizeHandle.Result == 0)
			{
				StartCoroutine(LoadAudioAssets());
			}
		}
		Addressables.Release(sizeHandle);
	}

	public void DownloadAudio()
	{
		if (_downloadCo != null)
		{
			GLogger.LogWarning("오디오 다운로드가 진행 중");
			return;
		}

		_downloadCo = StartCoroutine(DownloadAudioCo());
	}

	IEnumerator DownloadAudioCo()
	{
		var downloadHandle = Addressables.DownloadDependenciesAsync("Audio", false);

		while (!downloadHandle.IsDone)
		{
			AudioDownloadProgress?.Invoke(false, downloadHandle.PercentComplete);
			yield return null;
		}

		if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
		{
			_loadAssetsCo = StartCoroutine(LoadAudioAssets());
		}
		else
		{
			GLogger.LogWarning("다운로드 실패");
			AudioDownloadProgress?.Invoke(true, -1f);
		}

		AudioDownloadProgress?.Invoke(true, 1f);

		Addressables.Release(downloadHandle);
		_downloadCo = null;
	}

	IEnumerator LoadAudioAssets()
	{
		if (_loadAssetsCo != null)
		{
			GLogger.LogWarning("오디오 에셋 로드가 이미 진행 중");
			yield break;
		}

		yield return null;

		var loadAudioLocationsHandle = Addressables.LoadResourceLocationsAsync("Audio");
		yield return loadAudioLocationsHandle;

		if (loadAudioLocationsHandle.Status == AsyncOperationStatus.Succeeded)
		{
			foreach (var location in loadAudioLocationsHandle.Result)
			{
				if (location.ResourceType == typeof(AudioClip))
				{
					var loadAssetHandle = Addressables.LoadAssetAsync<AudioClip>(location);
					yield return loadAssetHandle;

					if (loadAssetHandle.Status == AsyncOperationStatus.Succeeded)
					{
						_audioClipPool[location.PrimaryKey] = loadAssetHandle.Result;
					}
				}
				else if (location.ResourceType == typeof(AudioResourceDataSO))
				{
					var ard = Addressables.LoadAssetAsync<AudioResourceDataSO>(location);
					yield return ard;

					if (ard.Status == AsyncOperationStatus.Succeeded)
					{
						var data = ard.Result;
						foreach (var config in data.Configs)
						{
							if (!_audioResourceConfigPool.ContainsKey(config.Key))
							{
								_audioResourceConfigPool[config.Key] = config;
							}
						}
					}
				}
						
			}
		}
		
		_loadAssetsCo = null;
	}

	IEnumerator PrintAddressableKeys()
	{
		var loadResourceHandle = Addressables.LoadResourceLocationsAsync("Audio");
		yield return loadResourceHandle;

		if (loadResourceHandle.Status == AsyncOperationStatus.Succeeded)
		{
			var result = loadResourceHandle.Result;
			GLogger.Log("--- addressable keys --");
			foreach (var location in result)
			{
				GLogger.Log(location.PrimaryKey);
			}
			GLogger.Log("-----------------------");
		}

		Addressables.Release(loadResourceHandle);
	}

	public void Release(IAudioPlayable ap)
	{
		_releaseReservedList.Add(ap);
	}

	public void PlayAudio(string key, Vector2 position = default)
	{
		if (_playingHolderList.TryGetValue(key, out var iap))
		{
			if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond -  iap.PlayTime < 50)
			{
				return;
			}
		}

		if (_audioClipPool.TryGetValue(key, out var clip))
		{
			var holder = _audioHolderPool.Get();
			holder.GO.transform.position = new Vector3(position.x, position.y, -5f);
			holder.Play(clip);
			holder.AudioName = key;
				_playingHolderList[key] = holder;
		}
		else if (_audioResourceConfigPool.TryGetValue(key, out var config))
		{
			var holder = _audioHolderPool.Get();
			holder.GO.transform.position = new Vector3(position.x, position.y, -5f);
			holder.Play(config.Audio, config.Length);
			holder.AudioName = key;
			_playingHolderList[key] = holder;
		}
		else
		{
			GLogger.LogWarning($"Unknown audio key {key} / {position}");
		}
	}
}
