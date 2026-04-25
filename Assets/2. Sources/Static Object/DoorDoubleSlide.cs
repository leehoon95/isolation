using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DoorDoubleSlide : NetworkBehaviour, IDoorHandler
{
	[SerializeField]
	SpriteRenderer _doorTop;
	[SerializeField]
	SpriteRenderer _doorBottom;
	[SerializeField]
	BoxCollider2D _colliderTop;
	[SerializeField]
	BoxCollider2D _colliderBottom;
	[Space]
	[Header("슬라이드 도어가 움직이는 방향, 닫혀있는 상태 기준")]
	[SerializeField]
	float _width;
	[SerializeField]
	float _height;
	[SerializeField]
	float _openingDuration;

	Coroutine _taskCo;

	void Start()
	{

	}

	void OnEnable()
	{
		OpenDoor();
	}

	void InitChildObjectPosition()
	{
		var half = _height / 2f;
		var quater = _height / 4f;
		_doorTop.gameObject.transform.localPosition = new Vector3(0f, quater);
		_doorTop.size = new Vector2(_width, half);
		_colliderTop.size = new Vector2(_width, half);
		_doorBottom.gameObject.transform.localPosition = new Vector3(0f, -quater);
		_doorBottom.size = new Vector2(_width, half);
		_colliderBottom.size = new Vector2(_width, half);
	}

	public void Open()
	{
		if (!IsHost || _taskCo != null)
		{
			return;
		}

		_taskCo = StartCoroutine(OpenDoor());
	}

	IEnumerator OpenDoor()
	{
		var half = _height / 2f;
		var quater = _height / 4f;
		_doorTop.transform.localPosition = new Vector2(0f, quater);
		_doorBottom.transform.localPosition = new Vector2(0f, quater);

		var pos = _doorTop.transform.localPosition;
		var slideEndPoint = new Vector2(0f, half + quater);
		var speed = (half + quater) / _openingDuration;

		do
		{
			yield return null;

			pos.y += speed * Time.deltaTime;
			_doorTop.transform.localPosition = pos;
			_doorBottom.transform.localPosition = -pos;
		} while (pos.y < slideEndPoint.y);

		_doorTop.transform.localPosition = slideEndPoint;
		_doorBottom.transform.localPosition = -slideEndPoint;
		_taskCo = null;
	}
}
