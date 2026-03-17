using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

//
public class PlayerCameraTarget : NetworkBehaviour
{
	[SerializeField]
	Vector2 _center;
	[Range(0f, 5f)]
	[SerializeField]
	float _maxMidpoint = 5f;

	Transform _parentTransform;

	public bool IsStopped { get; set; }

	void Start()
	{
		_parentTransform = transform.parent;
	}

	void Update()
	{
		if (!IsOwner || IsStopped)
		{
			return;
		}

		var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		var mouseDirection = mousePosition - (Vector2)transform.position;
		var midpoint = mouseDirection.magnitude / 2f;
		var rangedMindpoint = Mathf.Clamp(midpoint, 0f, _maxMidpoint);
		transform.position = (Vector2)_parentTransform.position + mouseDirection.normalized * rangedMindpoint;
	}
}
