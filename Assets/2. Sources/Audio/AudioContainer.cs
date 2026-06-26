using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

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
	GameObject _audioHolderPrefab;

	public static AudioContainer Instance { get; private set; }

	public event UnityAction<long> AudioDownloadable;
	public event UnityAction<bool, float> AudioDownloadProgress;

	ObjectPool<IAudioPlayable> _audioHolderPool;
	Dictionary<string, AudioResource> _audioResourcePool = new();
	HashSet<IAudioPlayable> _playingHolderSet = new();
	Dictionary<string, long> _playTimeCache = new();
	List<AsyncOperationHandle> _handles = new();

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

	async void Start()
	{
		_audioHolderPool = new ObjectPool<IAudioPlayable>(
			createFunc: () =>
			{
				var obj = Instantiate(_audioHolderPrefab);
				var iap = obj.GetComponent<IAudioPlayable>();
#if UNITY_EDITOR
				if (iap == null )
				{
					throw new InvalidOperationException("Invalid prefab");
				}
#endif

				iap.Pool = this;

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
				_playingHolderSet.Remove(instance);
			},
			actionOnDestroy: (instance) =>
			{
				GLogger.Log("destroy holder");
				Destroy(instance.GO);
			}
			);

		try
		{
			await CheckCatalogUpdatable();
			var downloadableSize = await GetDownloadSizeAsync("Audio");

			if (downloadableSize > 0)
			{
				AudioDownloadable?.Invoke(downloadableSize);
			}
			else
			{
				await LoadAudioAsset();
			}
		}
		catch (Exception e)
		{
			GLogger.LogException(e);
		}
	}

	async Awaitable CheckCatalogUpdatable()
	{
		await Addressables.InitializeAsync().Task;

		var checkCatalogHandle = Addressables.CheckForCatalogUpdates(false);
		await checkCatalogHandle.Task;

		if (checkCatalogHandle.Status != AsyncOperationStatus.Succeeded)
		{
			GLogger.LogWarning($"카탈로그 업데이트 확인 실패({checkCatalogHandle.OperationException.Message})");
			Addressables.Release(checkCatalogHandle);
			return;
		}

		var catalogsToUpdate = checkCatalogHandle.Result;
		if (catalogsToUpdate.Count == 0)
		{
			GLogger.LogWarning("카탈로그가 최신 버전이다");
			Addressables.Release(checkCatalogHandle);
			return;
		}

		GLogger.LogWarning($"업데이트 가능한 카탈로그 개수: {catalogsToUpdate.Count}. 업데이트 시작...");
		foreach (var catalog in catalogsToUpdate) 
		{
			GLogger.LogWarning($"catalog: {catalog}");
		}

		var catalogUpdateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
		await catalogUpdateHandle.Task;

		if (catalogUpdateHandle.Status != AsyncOperationStatus.Succeeded)
		{
			GLogger.LogWarning($"카탈로그 업데이트 실패\n{catalogUpdateHandle.OperationException.Message}");
		}
		else
		{
			GLogger.LogWarning("카탈로그 업데이트 완료");
		}

		Addressables.Release(checkCatalogHandle);
		Addressables.Release(catalogUpdateHandle);
	}

	async Awaitable<long> GetDownloadSizeAsync(string key)
	{
		var sizeHandle = Addressables.GetDownloadSizeAsync(key);
		await sizeHandle.Task;

		var downloadSize = sizeHandle.Result;
		GLogger.LogWarning($"다운로드 사이즈: {downloadSize / (1024f * 1024f):F2} MB");
		return downloadSize;
	}

	public async Awaitable<bool> DownloadBundles(string key)
	{
		var downloadHandle = Addressables.DownloadDependenciesAsync(key);

		while (!downloadHandle.IsDone)
		{
			var status = downloadHandle.GetDownloadStatus();
			AudioDownloadProgress?.Invoke(false, (float)status.DownloadedBytes / status.TotalBytes);

			await Task.Yield();
		}

		var result = downloadHandle.Status == AsyncOperationStatus.Succeeded;

		if (result)
		{
			GLogger.LogWarning("에셋 번들 다운로드 완료");
			AudioDownloadProgress?.Invoke(true, 1f);
		}
		else
		{
			GLogger.LogWarning("에셋 번들 다운로드 실패");
		}

		Addressables.Release(downloadHandle);
		return result;
	}

	public async Awaitable LoadAudioAsset()
	{
		var locationsHandle = Addressables.LoadResourceLocationsAsync("Audio");
		await locationsHandle.Task;

		foreach (var location in locationsHandle.Result)
		{
			var loadAssetHandle = Addressables.LoadAssetAsync<AudioResource>(location);
			await loadAssetHandle.Task;

			_handles.Add(loadAssetHandle);
			if (loadAssetHandle.Result != null)
			{
				_audioResourcePool[location.PrimaryKey] = loadAssetHandle.Result;
				GLogger.LogWarning($"오디오 에셋 로드: {location.PrimaryKey}");
			}
			else
			{
				GLogger.LogWarning($"오디오 에셋 로드 실패: {location.PrimaryKey}");
			}
		}

		Addressables.Release(locationsHandle);
	}

	public async Awaitable ReleaseAudioResources()
	{
		_audioHolderPool.Clear();

		foreach (var holder in _playingHolderSet)
		{
			Destroy(holder.GO);
		}

		_playingHolderSet.Clear();
		_audioResourcePool.Clear();

		await Task.Yield();

		foreach (var handle in _handles)
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
		_handles.Clear();

		await Task.Yield();

		var clearHandle = Addressables.ClearDependencyCacheAsync("Audio", true);
		await clearHandle.Task;
	}

	public void Release(IAudioPlayable ap)
	{
		_audioHolderPool.Release(ap);
	}

	public void PlayAudio(string key, Vector2 position = default)
	{
		var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		if (_playTimeCache.TryGetValue(key, out var t))
		{
			if (now - t < 50)
			{
				return;
			}
		}

		if (_audioResourcePool.TryGetValue(key, out var ar))
		{
			var holder = _audioHolderPool.Get();
			holder.GO.transform.position = position;
			holder.Play(ar);
			_playTimeCache[key] = now;
			_playingHolderSet.Add(holder);
		}
#if UNITY_EDITOR
		else
		{
			GLogger.LogWarning($"Unknown audio key {key} / {position}");
		}
#endif
	}
}
