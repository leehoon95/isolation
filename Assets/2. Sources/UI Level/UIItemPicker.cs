using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIItemPicker : UIBehaviour
{
	// L, F, R, B 순서대로 0, 1, 2, 3
	[SerializeField]
	Image _frontImage;
	[SerializeField]
	Image _leftImage;
	[SerializeField]
	Image _rightImage;
	[SerializeField]
	Image _bottomImage;
	[SerializeField]
	RectTransform _centerPoint;

	public void SelectedTile(int index)
	{
		_frontImage.color = _leftImage.color = _rightImage.color = new Color(1f, 0.5f, 0f) / 2f;
		_bottomImage.color = new Color(1f, 69f / 255f, 0f) / 2f;

		switch (index)
		{
			case 0:
				_rightImage.color = new Color(1f, 0.5f, 0f);
				break;
			case 1:
				_frontImage.color = new Color(1f, 0.5f, 0f);
				break;
			case 2:
				_leftImage.color = new Color(1f, 0.5f, 0f);
				break;
			case 3:
				_bottomImage.color = new Color(1f, 69f / 255f, 0f);
				break;
		}
	}
}
