using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIPlayerSlot : UIBehaviour
{
	[SerializeField]
    TMP_Text _slotText;
	[SerializeField] 
	TMP_Text _hostSign;
	[SerializeField]
	TMP_Text _youSign;
	[SerializeField]
	UIPlayerSlotBackground _background;
	[SerializeField]
	Image _borderImage;
	[SerializeField]
	PlayerSlotStatus _status = PlayerSlotStatus.Empty;

	public PlayerSlotStatus SlotStatus
	{
		get => _status;
		set
		{
			SetSlotStatus(value);
		}
	}

	public string SlotText
	{
		set => _slotText.text = value;
	}

	public Color SlotTextColor
	{
		set => _slotText.color = value;
	}

	public bool You
	{
		set => _youSign.gameObject.SetActive(value);
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		SetSlotStatus(_status);
	}
#endif

	protected override void OnEnable()
	{
		_slotText.text = "";
		SetSlotStatus(0);
	}

	void SetSlotStatus(PlayerSlotStatus status)
	{
		_status = status;
		switch (status)
		{
			case PlayerSlotStatus.Empty:
				_slotText.text = "";
				_background.PatternColor = Color.gray;
				_background.Offset = 0.5f;
				_borderImage.color = Color.gray;
				_hostSign.gameObject.SetActive(false);
				break;
			case PlayerSlotStatus.InUse:
				_background.PatternColor = new Color(1f, 127f / 255f, 0f);
				_background.Offset = 0.5f;
				_borderImage.color = new Color(1f, 127f / 255f, 0f);
				_hostSign.gameObject.SetActive(false);
				break;
			case PlayerSlotStatus.Ready:
				_background.PatternColor = Color.green;
				_background.Offset = 0.3f;
				_borderImage.color = new Color(0f, 218f / 255f, 255f);
				_hostSign.gameObject.SetActive(false);
				break;
			case PlayerSlotStatus.Host:
				_background.PatternColor = new Color(1f, 69f / 255f, 0f);
				_background.Offset = 0.3f;
				_borderImage.color = new Color(1f, 69f / 255f, 0f);
				_hostSign.gameObject.SetActive(true); ;
				break;
			default:
				GLogger.LogWarning("UIPlayerSlot.SetSlotStatus Unknown status argument {status}");
				break;
		}
	}
}
