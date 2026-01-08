using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UILobbyList : UIBehaviour, IUILobbyList
{
	[SerializeField]
	ScrollRect _scrollRect;
	[SerializeField]
	UISessionItem _sessionPrefap;
	[SerializeField]
	GameObject _emptyLobbyListNotification;
	[SerializeField]
	CanvasGroup _canvasGroup;

	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.LobbyList = this;

		_scrollRect = GetComponent<ScrollRect>();
	}

	void OnClickLobbyEntry(string lobbyId)
	{
		print($"UILobbyList.OnClickLobbyEntry {lobbyId}");

		_uiso.RaiseOnClickSession(lobbyId);
	}

	public void SetLobbyInfoIndex(
		uint index,
		string name,
		int maxPlayers,
		int currentPlayers,
		string lobbyId)
	{
		if (_scrollRect.content.childCount <= index)
		{
			Debug.LogError($"UILobbyList.SetLobbyInfoIndex - index out of range: {index}");
			return;
		}

		var sitem = _scrollRect.content.GetChild((int)index)?.GetComponent<UISessionItem>();

		sitem.SetLobbyInfo(
			name,
			maxPlayers,
			currentPlayers,
			lobbyId);
	}

	/*
	 * session list item 개수가 변경되었을 때
	 * item개수를 미리 조절함
	 */
	public void ResizeLobbyList(uint size, bool destroy = false)
	{
		var childCount = _scrollRect.content.childCount;

		if (size > childCount)
		{
			int countToAdd = (int)size - childCount;
			var rt = _scrollRect.content.GetComponent<RectTransform>();

			for (int i = 0; i < countToAdd; i++)
			{
				var sessionItem = Instantiate(_sessionPrefap, _scrollRect.transform);
				sessionItem.OnClick += OnClickLobbyEntry;
				sessionItem.transform.SetParent(_scrollRect.content);
				sessionItem.transform.localScale = Vector3.one;
			}
		}
		else if (size < childCount)
		{
			for (int i = childCount - 1; i >= size; i--)
			{
				if (destroy)
				{
#if UNITY_EDITOR
					DestroyImmediate(_scrollRect.content.GetChild(i).gameObject);
#else
					Destroy(_scrollRect.content.GetChild(i).gameObject);
#endif
				}
				else
				{
					_scrollRect.content.GetChild(i).gameObject.SetActive(false);
				}
			}
		}

		_emptyLobbyListNotification.SetActive(size == 0 ? true : false);
	}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}

#if UNITY_EDITOR
	public void AddTempSession()
	{
		int count = _scrollRect.content.childCount;

		ResizeLobbyList((uint)count + 1);

		SetLobbyInfoIndex(
			(uint)count,
			$"temp {count + 1}",
			0,
			0,
			"");
	}

	public void AddTempSession_10()
	{
		int count = _scrollRect.content.childCount;

		ResizeLobbyList((uint)count + 10);

		for (uint i = 0; i < 10; i++)
		{
			SetLobbyInfoIndex(
				(uint)count + i,
				$"temp {count + i}",
				0,
				0,
				"");
		}
	}

	public void ClearSessionList()
	{
		ResizeLobbyList(0, true);
	}
#endif
}
