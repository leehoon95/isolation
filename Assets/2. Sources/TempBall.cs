using Unity.Netcode;
using UnityEngine;

public class TempBall : NetworkBehaviour
{
	[SerializeField]
	Rigidbody2D _rigidbody;

	float _maxSpeed = 20f;
	Vector2 _targetPosition;
	Vector2 _integral;
	Vector2 _prevError;

	float _kp = 3f;
	float _ki = 0f;
	float _kd = 0.2f;

	float _forceScale = 5f;
	float _damping = 2f;

	Vector2 _startFrom;

	public Vector2 StartFrom { get => Vector2.zero; set => _startFrom = value; }

	[Rpc(SendTo.Server)]
	void DespawnObjectRpc(NetworkObjectReference target)
	{
		if (target.TryGet(out NetworkObject targetObject))
		{
			targetObject.Despawn();
		}
	}

	void FixedUpdate()
	{
		if (HasAuthority)
		{
			if (_rigidbody.linearVelocity.magnitude > _maxSpeed)
			{
				_rigidbody.linearVelocity = _rigidbody.linearVelocity * (_maxSpeed / _rigidbody.linearVelocity.magnitude);
			}


			// PID
			//Vector2 desired = (_targetPosition - (Vector2)transform.position).normalized * _maxSpeed;
			//Vector2 error = desired - _rigidbody.linearVelocity;
			//_integral += error * Time.fixedDeltaTime;
			//Vector2 derivative = (error - _prevError) / Time.fixedDeltaTime;
			//_prevError = error;

			//Vector2 force = _kp * error + _ki * _integral + _kd * derivative;


			// PD
			Vector2 desiredVelocity = (_targetPosition - (Vector2)transform.position).normalized * _maxSpeed;
			Vector2 currentVelocity = _rigidbody.linearVelocity;
			Vector2 velocityError = desiredVelocity - currentVelocity;
			Vector2 force = velocityError * _forceScale - _rigidbody.linearVelocity * _damping * 0.1f;
			
			_rigidbody.AddForce(force);
		}
	}

	public void AddForce(Vector2 force)
	{
		AddForceRpc(force);
	}

	[Rpc(SendTo.Server)]
	void AddForceRpc(Vector2 force)
	{
		_rigidbody.AddForce(force, ForceMode2D.Impulse);
	}

	public void AddCollisionEvent(CollisionEvent ce)
	{
		throw new System.NotImplementedException();
	}
}
