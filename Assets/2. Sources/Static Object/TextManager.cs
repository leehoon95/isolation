using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct TextConfig
{
	public string TextName;
	public TMP_Text Text;
}

// deprecated
[RequireComponent(typeof(NetworkObject))]
public class TextManager : NetworkBehaviour
{
	[SerializeField]
	List<TextConfig> _textConfigs;

	Dictionary<string, TMP_Text> _texts = new();
	Coroutine _taskCo;

	void Start()
	{
		foreach (var config in _textConfigs)
		{
			_texts[config.TextName] = config.Text;
		}
	}

	public void WriteText(string key, string text)
	{
		if (!IsHost || key == null || text == null)
		{
			return;
		}

		WriteTextRpc(key, text);
	}

	[Rpc(SendTo.Everyone)]
	void WriteTextRpc(FixedString32Bytes key, FixedString32Bytes text)
	{
		if (_texts.TryGetValue(key.ToString(), out var t))
		{
			t.text = text.ToString();
		}
		else
		{
			GLogger.LogWarning($"WriteTextRpc Invalid key. {key.ToString()}");
		}
	}

	public void FadeOutText(string key)
	{
		if (!IsHost || key == null)
		{
			return;
		}

		FadeOutTextRpc(key);
	}

	[Rpc(SendTo.Everyone)]
	void FadeOutTextRpc(FixedString32Bytes key)
	{
		if (_texts.TryGetValue(key.ToString(), out var t))
		{
			if (_taskCo != null)
			{
				StopCoroutine(_taskCo);
			}

			_taskCo = StartCoroutine(FadeOutCounter(t));
		}
		else
		{
			GLogger.LogWarning($"FadeOutTextRpc Invalid key. {key.ToString()}");
		}
	}

	IEnumerator FadeOutCounter(TMP_Text text, float fadeDuration = 1f)
	{
		var t = fadeDuration;

		Color c = Color.white;

		while (t > 0f)
		{
			c = text.color;
			c.a = t / fadeDuration;
			text.color = c;

			t -= Time.deltaTime;
			yield return null;
		}

		c.a = 0f;
		text.color = c;

		_taskCo = null;
	}
}
