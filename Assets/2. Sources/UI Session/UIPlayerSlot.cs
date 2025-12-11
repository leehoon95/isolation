using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * Slot status
 * 0: empty
 * 1: occupied
 * 2: ready
 * 3: host
 */
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIPlayerSlot : UIBehaviour
{
    [SerializeField]
    TMP_Text _slotText;
	[SerializeField]
	UIPlayerSlotBackground _background;
	[SerializeField]
	Image _borderImage;


#if UNITY_EDITOR
	[SerializeField]
	[Range(0, 3)]
	int _slotStatus;
	protected override void OnValidate()
	{
		SetSlotStatus(_slotStatus);
	}
#endif

	protected override void OnEnable()
	{
		_slotText.text = "EMPTY";
		SetSlotStatus(0);
	}

	public void SetSlotText(string slotName) => _slotText.text = slotName;
	public void SetSlotStatus(int status)
	{
		switch (status)
		{
			case 0:
				_background.PatternColor = Color.gray;
				_background.Offset = 0.5f;
				_borderImage.color = Color.gray;
				break;
			case 1:
				_background.PatternColor = new Color(1f, 127f / 255f, 0f);
				_background.Offset = 0.5f;
				_borderImage.color = new Color(1f, 127f / 255f, 0f);
				break;
			case 2:
				_background.PatternColor = Color.green;
				_background.Offset = 0.3f;
				_borderImage.color = new Color(0f, 218f/255f, 255f);
				break;
			case 3:
				_background.PatternColor = new Color(1f, 69f / 255f, 0f);
				_background.Offset = 0.3f;
				_borderImage.color = new Color(1f, 69f / 255f, 0f);
				break;
			default:
				GLogger.LogWarning("UIPlayerSlot.SetSlotStatus Unknown status argument {status}");
				break;
		}
	}
}
