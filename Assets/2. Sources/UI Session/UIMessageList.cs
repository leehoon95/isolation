using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(ScrollRect))]
public class UIMessageList : UIBehaviour, IUIMessageList
{
	[SerializeField]
	GameObject _chatMessagePrefab;

    ScrollRect _scrollRect;
	UISessionSO _uiso;

	uint _chatCount;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.MessageList = this;
		_scrollRect = GetComponent<ScrollRect>();
	}

	public void AddMessage(string message, Color color)
	{
		var go = Instantiate(_chatMessagePrefab, _scrollRect.transform);
		var cm = go.GetComponent<UIChatMessage>();
		cm.text = message;
		cm.textColor = color;
		go.transform.SetParent(_scrollRect.content);

		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
		_scrollRect.verticalNormalizedPosition = 0f;
	}
}
