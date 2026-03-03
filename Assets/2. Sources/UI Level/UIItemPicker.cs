using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

public class UIItemPicker : UIBehaviour
{
	// R, F, L, B 순서대로 0, 1, 2, 3
	[SerializeField] Image _frontImage;
	[SerializeField] Image _leftImage;
	[SerializeField] Image _rightImage;
	[SerializeField] Image _bottomImage;
	[SerializeField] TMP_Text _leftText;
	[SerializeField] TMP_Text _rightText;
	[SerializeField] TMP_Text _itemName;
	
	bool _onlyFrontMode;

	public bool OnlyFronMode
	{
		get => _onlyFrontMode;
		set => _onlyFrontMode = value;
	}

	public string ItemName
	{
		set => _itemName.text = value;
	}

	public void SelectedTile(int index)
	{
		if (_onlyFrontMode)
		{
			_frontImage.color = new Color(1f, 0.5f, 0f) / 2f;
			_leftImage.color = _rightImage.color = new Color(0.25f, 0.25f, 0.25f);
			_leftText.color = _rightText.color = Color.gray;
		}
		else
		{
			_frontImage.color = _leftImage.color = _rightImage.color = new Color(1f, 0.5f, 0f) / 2f;
			_leftText.color = _rightText.color = Color.white;
		}
			
		_bottomImage.color = new Color(1f, 69f / 255f, 0f) / 2f;

		switch (index)
		{
			case 0:
				if (_onlyFrontMode)
				{
					return;
				}
				_rightImage.color = new Color(1f, 0.5f, 0f);
				break;
			case 1:
				_frontImage.color = new Color(1f, 0.5f, 0f);
				break;
			case 2:
				if (_onlyFrontMode)
				{
					return;
				}
				_leftImage.color = new Color(1f, 0.5f, 0f);
				break;
			case 3:
				_bottomImage.color = new Color(1f, 69f / 255f, 0f);
				break;
		}
	}
}
