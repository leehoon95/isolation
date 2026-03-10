using UnityEngine;
using UnityEngine.InputSystem;

//
public class PlayerCameraTarget : MonoBehaviour
{
	[SerializeField]
	Vector2 _center;
	[Range(0f, 5f)]
	[SerializeField]
	float _maxMidpoint = 5f;

	Transform _parentTransform;

	void Start()
	{
		_parentTransform = transform.parent;
	}

	void Update()
	{
		var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		var mouseDirection = mousePosition - (Vector2)transform.position;
		var midpoint = mouseDirection.magnitude / 2f;
		var rangedMindpoint = Mathf.Clamp(midpoint, 0f, _maxMidpoint);
		transform.position = (Vector2)_parentTransform.position + mouseDirection.normalized * rangedMindpoint;
	}
}
