using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TestNaviAgent : MonoBehaviour
{
	[SerializeField]
	NavMeshAgent _agent;

	InputSystem _inputSystem;


	void Start()
	{
		_inputSystem = FindAnyObjectByType<InputSystem>();
		_inputSystem.LeftTrigger += OnClick;
		_agent.updateRotation = false;
		_agent.updateUpAxis = false;
	}

	void Update()
	{
		//_agent.SetDestination(Vector3.zero);
	}

	void OnClick(bool trigger)
	{
		if (trigger)
		{
			var mouseScreenPosition = Mouse.current.position.ReadValue();
			var mouseScreenPositionWithDepth = new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 10f);
			var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPositionWithDepth);
			GLogger.Log($"set destination {mouseWorldPosition}");
			_agent.SetDestination(mouseWorldPosition);
		}
	}
}
