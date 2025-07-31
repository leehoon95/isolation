using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CanvasFitter : MonoBehaviour
{
	[SerializeField]
    Canvas _canvas;
	[SerializeField]
	float _distanceFromCamera = 1f;

	RectTransform _rt;

	private void OnValidate()
	{
		
	}

	void Start()
	{
		_rt = _canvas.GetComponent<RectTransform>();

		Camera mainCamera = Camera.main;
		float height = mainCamera.orthographicSize * 2f;

		//_canvas.scaleFactor = height / _rt.sizeDelta.y;
		print($"reference resolution: {_canvas.renderingDisplaySize}");
	}

	void Update()
	{
		Camera mainCamera = Camera.main;
		float height = mainCamera.orthographicSize * 2f;
		float width = height * mainCamera.aspect;

		_rt.sizeDelta = new Vector2(width, height);


		transform.position = mainCamera.transform.position + mainCamera.transform.forward * _distanceFromCamera;

		print($"set {width} {height}");

	}
}
