using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICurtain : UIBehaviour, IUICurtain
{
	[SerializeField]
	Image _image;
	[SerializeField]
	float _duration;

	//UILevelSO _uiso;
	Coroutine _task;

	protected override void Start()
	{
		//_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		//_uiso.Curtain = this;

		_image.gameObject.SetActive(true);
		_image.color = Color.black;
	}

	public void Open()
	{
		if (_task != null)
		{
			StopCoroutine(_task);
		}
		GLogger.Log("open curtain");
		_task = StartCoroutine(OpenCurtain());
	}

	public void Close()
	{
		if (_task != null)
		{
			StopCoroutine(_task);
		}

		_task = StartCoroutine(CloseCurtain());
	}
	

	IEnumerator OpenCurtain()
	{
		_image.gameObject.SetActive(true);
		yield return null;

		float t = 0;
		var color = Color.black;

		while (t < _duration)
		{
			color.a = 1f - t / _duration;
			_image.color = color;

			t += Time.deltaTime;
			yield return null;
		}

		color.a = 0f;
		_image.color = color;

		_image.gameObject.SetActive(false);
	}

	IEnumerator CloseCurtain()
	{
		var color = Color.black;
		_image.gameObject.SetActive(true);
		color.a = 0f;
		_image.color = color;
		yield return null;

		float t = 0;

		while (t < _duration)
		{
			color.a = t / _duration;
			_image.color = color;

			t += Time.deltaTime;
			yield return null;
		}

		color.a = 1f;
		_image.color = color;
	}
}
