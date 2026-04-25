using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class AudioiContainer : MonoBehaviour
{
	[SerializeField]
	AudioSource _audioSource;

	void Start()
	{
		StartCoroutine(LoadAudioEffectAddressableAsset());
	}

	IEnumerator LoadAudioEffectAddressableAsset()
	{
		yield return null;



		//List<string> keys = new()
		//{
		//	"AudioEffect"
		//};

		//Action<AudioClip> callback = (AudioClip ac) =>
		//{
		//	GLogger.Log($"Loaded addressable asset {ac.name}");
		//};

		//var operationHandle = Addressables.LoadAssetsAsync(keys, callback, Addressables.MergeMode.Intersection);
		var operationHandle = Addressables.LoadAssetAsync<AudioResource>("audio-arc-explosion");
		yield return operationHandle;

		var result = operationHandle.Result;
		_audioSource.resource = result;
		_audioSource.Play();
		GLogger.Log("어드레서블 로드 완료");
	}
}
