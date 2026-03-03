using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Collider2D))]
public class PlayerHand : MonoBehaviour
{
	[SerializeField]
	CircleCollider2D _handCollider;

	/*
	 * 가까이에 있어서 선택이 가능한 아이템들을 보관한다
	 */
	Dictionary<ulong, IItemHandler> _handligItem = new();
	List<ulong> _inactivedItems = new();
	IItemHandler _selectedItem;
	Coroutine _calcCo;

	public event UnityAction<IItemHandler> OnGrabbedItem;

	public void ActiveHand()
	{
		if (_calcCo != null)
		{
			StopCoroutine(_calcCo);
		}
		_handligItem.Clear();
		_calcCo = StartCoroutine(FindMostNearItem());
	}

	//void FixedUpdate()
	//{
	//	float d = float.MaxValue;
	//	IItemHandler mostNear = null;
	//	foreach (var item in _handligItem)
	//	{
	//		if (!item.Value.GO.activeInHierarchy)
	//		{
	//			_inactivedItems.Add(item.Key);
	//			continue;
	//		}

	//		var distance = (item.Value.GO.transform.position - transform.position).magnitude;
	//		if (distance < d)
	//		{
	//			if (mostNear != null)
	//			{
	//				mostNear.IsSelected = false;
	//			}

	//			d = distance;
	//			mostNear = item.Value;
	//			mostNear.IsSelected = true;
	//		}
	//		else
	//		{
	//			item.Value.IsSelected = false;
	//		}
	//	}

	//	OnGrabbedItem?.Invoke(mostNear);

	//	if (_inactivedItems.Count > 0)
	//	{
	//		foreach (var item in _inactivedItems)
	//		{
	//			_handligItem.Remove(item);
	//		}

	//		_inactivedItems.Clear();
	//	}
	//}

	IEnumerator FindMostNearItem()
	{
		var delay = new WaitForSeconds(0.05f);
		List<ulong> inactivedItems = new();

		while (true)
		{
			float d = float.MaxValue;
			IItemHandler mostNear = null;
			foreach (var item in _handligItem)
			{
				if (!item.Value.GO.activeInHierarchy)
				{
					inactivedItems.Add(item.Key);
					continue;
				}

				var sqrLen = (item.Value.GO.transform.position - transform.position).sqrMagnitude;
				if (sqrLen < d)
				{
					if (mostNear != null)
					{
						mostNear.IsSelected = false;
					}

					d = sqrLen;
					mostNear = item.Value;
					mostNear.IsSelected = true;
				}
				else
				{
					item.Value.IsSelected = false;
				}
			}

			OnGrabbedItem?.Invoke(mostNear);

			if (inactivedItems.Count > 0)
			{
				foreach (var item in inactivedItems)
				{
					_handligItem.Remove(item);
				}
				_inactivedItems.Clear();
			}

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
