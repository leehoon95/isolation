using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PlayerHand : MonoBehaviour
{
	[SerializeField]
	CircleCollider2D _handCollider;

	/*
	 * 가까이에 있어서 선택이 가능한 아이템들을 보관한다
	 */
	Dictionary<ulong, IItemHandler> _handligItem = new();
	IItemHandler _selectedItem;
	Coroutine _calcCo;

	public event UnityAction<IItemHandler> OnGrabbedItem;

	void OnEnable()
	{
		_calcCo = StartCoroutine(FindMostNearItem());
	}

	void OnDisable()
	{
		if (_calcCo != null )
		{
			StopCoroutine(_calcCo);
		}
	}

	IEnumerator FindMostNearItem()
	{
		var delay = new WaitForSeconds(0.05f);
		while (true)
		{
			float d = float.MaxValue;
			IItemHandler mostNear = null;
			foreach (var item in _handligItem)
			{
				var distance = (item.Value.GO.transform.position - transform.position).magnitude;
				if (distance < d)
				{
					if (mostNear != null)
					{
						mostNear.IsSelected = false;
					}

					d = distance;
					mostNear = item.Value;
					mostNear.IsSelected = true;
				}
				else
				{
					item.Value.IsSelected = false;
				}
			}

			OnGrabbedItem?.Invoke(mostNear);

			yield return delay;
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		var ih = collision.GetComponentInParent<IItemHandler>();
		if (ih != null)
		{
			_handligItem[ih.NO.NetworkObjectId] = ih;
			
		}
	}

	void OnTriggerExit2D(Collider2D collision)
	{
		var ih = collision.GetComponentInParent<IItemHandler>();
		if (ih != null)
		{
			ih.IsSelected = false;
			_handligItem.Remove(ih.NO.NetworkObjectId);
			
		}
	}
}
