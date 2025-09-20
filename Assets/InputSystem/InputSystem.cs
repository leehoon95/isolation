	using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
	InputSystem_Actions _inputSystemActions;
	InputSystem_Actions.PlayerActions _playerActions;

	// event
	public event UnityAction<Vector2> Move;
	public event UnityAction<Vector2> Look;
	public event UnityAction<bool> Attack;
	public event UnityAction<bool> Attack2;
	public event UnityAction<bool> SwitchCamera1;
	public event UnityAction<bool> SwitchCamera2;

	public Vector2 MousePos
	{
		get; private set;
	}

	void Awake()
	{
		//var obj = FindAnyObjectByType<InputSystem>();

		//if (obj != null && obj != this)
		//{
		//	Destroy(obj.gameObject);
		//	return;
		//}
		//else
		//{
		//	_inputSystemActions = new InputSystem_Actions();
		//	_playerActions = _inputSystemActions.Player;
		//	_playerActions.AddCallbacks(this);
		//	_playerActions.Enable();

		//	DontDestroyOnLoad(gameObject);
		//}

		_inputSystemActions = new InputSystem_Actions();
		_playerActions = _inputSystemActions.Player;
		_playerActions.AddCallbacks(this);
		_playerActions.Enable();
	}

	void InputSystem_Actions.IPlayerActions.OnMove(InputAction.CallbackContext context)
	{
		//print($"OnMove: {context.ReadValue<Vector2>()}");
		Move?.Invoke(context.ReadValue<Vector2>());
	}

	void InputSystem_Actions.IPlayerActions.OnLook(InputAction.CallbackContext context)
	{
		//print($"OnLook: {Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>())}");
		MousePos = context.ReadValue<Vector2>();
		Look?.Invoke(context.ReadValue<Vector2>());
	}

	void InputSystem_Actions.IPlayerActions.OnAttack(InputAction.CallbackContext context)
	{
		//print($"OnAttack: {context.ReadValue<bool>()}");
		Attack?.Invoke(context.performed);
	}

	void InputSystem_Actions.IPlayerActions.OnSwitchCamera1(InputAction.CallbackContext context)
	{
		if (!context.started)
		{
			SwitchCamera1?.Invoke(context.performed);
		}
	}

	void InputSystem_Actions.IPlayerActions.OnSwitchCamera2(InputAction.CallbackContext context)
	{
		SwitchCamera2?.Invoke(context.performed);
	}

	public void OnAttack2(InputAction.CallbackContext context)
	{
		Attack2?.Invoke(context.performed);
	}
}
