using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D _body;
	[SerializeField]
	CameraManager _cm;
	
	InputSystem _is;
    Vector2 _characterVelocity;
    Vector2 _mousePos;
	int _cameraActivated;

	void EnableWithInputListner()
    {
        _is.Move += OnMove;
        _is.Look += OnLook;
        _is.Attack += OnAttack;
		_is.SwitchCamera1 += SwitchCamera1;
	}

	void DisableWithInputListner()
	{
		_is.Move -= OnMove;
		_is.Look -= OnLook;
		_is.Attack -= OnAttack;
	}

	void OnMove(Vector2 velocity)
    {
		_characterVelocity = velocity;
	}

    void OnLook(Vector2 pos)
    {
        _mousePos = Camera.main.ScreenToWorldPoint(pos);
    }

    void OnAttack(bool attack)
    {

    }

	void SwitchCamera1(bool pressed)
	{
		if (!pressed)
		{
			var cl = _cm.Cameras;

			_cameraActivated++;

			if (_cameraActivated >= cl.Count)
			{
				_cameraActivated = 0;
			}

			for (int i = 0; i < cl.Count; i++)
			{
				if (i != _cameraActivated)
				{
					cl[i].Priority = 0;
				}
				else
				{
					cl[i].Priority = 1;
				}
			}
		}
	}

	

	private void OnEnable()
	{
        EnableWithInputListner();
	}

	private void OnDisable()
	{
		DisableWithInputListner();
	}

	private void Awake()
	{
		_is = FindAnyObjectByType<InputSystem>();
	}

	void Start()
    {
        
    }

	void FixedUpdate()
	{
	    _body.linearVelocity = _characterVelocity * 2f;
	}

	void Update()
    {
     
    }

}
