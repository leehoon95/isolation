using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIIndicatorBarContainer : UIBehaviour
{
	[SerializeField]
	List<GameObject> _barList;

	int _count;

	public int Count
	{
		get => _count;
		set
		{
			SetIndicator(value);
		}
	}

	public void SetIndicator(int count)
	{
		if (count < 0)
		{
			return;
		}

		_count = count;
		if (count > _barList.Count)
		{
			//GLogger.Log($"UIIndicatorBarContainer.SetIndicator count is too high ({count} / {_barList.Count})");
			count = _barList.Count;
		}

		for (int i = 0; i < count; i++)
		{
			_barList[i].SetActive(true);
		}

		for (int i = count; i < _barList.Count; i++)
		{
			_barList[i].SetActive(false);
		}
	}
}
