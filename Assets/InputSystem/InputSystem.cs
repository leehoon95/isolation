using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputSystem : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
	InputSystem_Actions _inputSystemActions;
	InputSystem_Actions.PlayerActions _playerActions;

	// event
	public event UnityAction<Vector2> Move;
	public event UnityAction<Vector2> Look;
	public event UnityAction<bool> LeftTrigger;
	public event UnityAction<bool> RightTrigger;
	public event UnityAction<bool> UseItem;
	public event UnityAction<bool> SwitchCamera1;
	public event UnityAction<bool> SwitchCamera2;

	void Awake()
	{
		_inputSystemActions = new InputSystem_Actions();
		_playerActions = _inputSystemActions.Player;
		_playerActions.AddCallbacks(this);
		_playerActions.Enable();
	}

	void InputSystem_Actions.IPlayerActions.OnMove(InputAction.CallbackContext context)
	{
		Move?.Invoke(context.ReadValue<Vector2>());
	}

	void InputSystem_Actions.IPlayerActions.OnLeftTrigger(InputAction.CallbackContext context)
	{
		LeftTrigger?.Invoke(context.performed);
	}

	public void OnRightTrigger(InputAction.CallbackContext context)
	{
		RightTrigger?.Invoke(context.performed);
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

	void InputSystem_Actions.IPlayerActions.OnUseItem(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			return;
		}
		UseItem?.Invoke(context.performed);
	}
}
