using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public struct AudioResourceConfig
{
	public string Key;
	public AudioResource Audio;
	public float Length;
}


/*
 * Audio Random Container는 재생 길이와 같은 부가적 정보를 알 수 없으므로 SO 래핑한다
 */
[CreateAssetMenu(fileName = "AudioResourceDataSO", menuName = "Scriptable Objects/AudioResourceDataSO")]
public class AudioResourceDataSO : ScriptableObject
{
	[SerializeField]
	public List<AudioResourceConfig> Configs;
}
