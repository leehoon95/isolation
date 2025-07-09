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
	int _cameraActivated;

	void EnableWithInputListner()
    {
        _is.Move += OnMove;
        _is.Attack += OnAttack;
		_is.SwitchCamera1 += SwitchCamera1;
	}

	void DisableWithInputListner()
	{
		_is.Move -= OnMove;
		_is.Attack -= OnAttack;
	}

	void OnMove(Vector2 velocity)
    {
		_characterVelocity = velocity;
	}

    void OnAttack(bool attack)
    {

    }

	void SwitchCamera1(bool pressed)
	{
		if (!pressed)
		{
			_cm.ActivateNextCamera();
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
		Vector2 dir = Camera.main.ScreenToWorldPoint(_is.MousePos) - transform.position;
		
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
		   
		transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}
