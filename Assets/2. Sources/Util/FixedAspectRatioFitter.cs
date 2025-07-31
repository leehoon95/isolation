using UnityEngine;

public enum AspectReference
{
	width,
	height
}

public class FixedAspectRatioFitter : MonoBehaviour
{
	[SerializeField]
	RectTransform _rectTransform;
	[SerializeField]
	AspectReference _aspectReference;
	[SerializeField]
	float _ratio = 1f;


	private void Start()
	{
		if (_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
	}

	private void OnValidate()
	{
		if (_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
	}

	private void Update()
	{
		if (_rectTransform == null)
		{
			return;
		}

		FixAspectRaio();
	}

	void FixAspectRaio()
	{
		//_rectTransform.sizeDelta = Vector2.zero;
		if (_aspectReference == AspectReference.width)
		{
			var origin = _rectTransform.sizeDelta;
			origin.y = origin.x * _ratio;
			_rectTransform.sizeDelta = origin;
		}
		else if (_aspectReference == AspectReference.height)
		{

			var origin = _rectTransform.sizeDelta;
			print($"origin : {origin}");
			origin.x = origin.y * _ratio;
			_rectTransform.sizeDelta = origin;
			print($"origin2 : {origin}");
		}
	}
}
