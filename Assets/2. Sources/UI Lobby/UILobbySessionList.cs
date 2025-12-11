using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UILobbySessionList : UIBehaviour, IUILobbySessionList
{
	[SerializeField]
	ScrollRect _scrollRect;
	[SerializeField]
	UISessionItem _sessionPrefap;
	[SerializeField]
	GameObject _emptySessionListNotification;

	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.SessionList = this;

		_scrollRect = GetComponent<ScrollRect>();
	}

	void OnExit()
	{
		print("OnExit");
	}

	void OnRefresh()
	{
		print("OnRefresh");
	}

	void OnSetting()
	{
		print("OnSetting");
	}

	void RefreshRoomList()
	{
		_scrollRect.verticalNormalizedPosition = 1f;
	}

	void OnClickSessionEntry(string lobbyId)
	{
		print($"OnClickSessionEntry {lobbyId}");

		_uiso.RaiseOnClickSession(lobbyId);
	}

	//protected override void OnRectTransformDimensionsChange()
	//{
	//	if (_scrollRect.content.childCount == 0)
	//	{
	//		return;
	//	}

	//	RectTransform rectTransform = _scrollRect.content.GetComponent<RectTransform>();

	//	int count = _scrollRect.content.childCount;

	//	for (int i = 0; i < count; i++)
	//	{
	//		var si = _scrollRect.content.GetChild(i).GetComponent<UISessionItem>();
	//		si.FitSize(rectTransform);
	//	}
	//}

	public void SetSessionInfoIndex(
		int index,
		string name,
		int maxPlayerCount,
		int playerCount,
		string lobbyId)
	{
		var sitem = _scrollRect.content.GetChild(index).GetComponent<UISessionItem>();

		sitem.SetSessionInfo(
			name,
			maxPlayerCount,
			playerCount,
			lobbyId);
	}

	/*
	 * session list item 개수가 변경되었을 때
	 * item개수를 미리 조절함
	 */
	public void ResizeSessionList(int minimumSession)
	{
		if (minimumSession > _scrollRect.content.childCount)
		{
			int countToAdd = minimumSession - _scrollRect.content.childCount;
			var rt = _scrollRect.content.GetComponent<RectTransform>();

			for (int i = 0; i < countToAdd; i++)
			{
				var sessionItem = Instantiate(_sessionPrefap, _scrollRect.transform);
				sessionItem.OnClick += OnClickSessionEntry;
				sessionItem.transform.SetParent(_scrollRect.content);
				sessionItem.transform.localScale = Vector3.one;
			}

			return;
		}
		else if (minimumSession < _scrollRect.content.childCount)
		{
			int countToRemove = _scrollRect.content.childCount - minimumSession;

#if UNITY_EDITOR
			countToRemove--;
			while (countToRemove >= 0)
			{
				DestroyImmediate(_scrollRect.content.GetChild(countToRemove).gameObject);
				countToRemove--;
			}
#else
			for (int i = 0; i < countToRemove; i++)
			{
				Destroy(_scrollRect.content.GetChild(i).gameObject);
			}
#endif
			}
		}

	public void ShowEmptySessionListNotification(bool show)
	{
		_emptySessionListNotification.SetActive(show);
	}

#if UNITY_EDITOR
	public void AddTempSession()
	{
		int count = _scrollRect.content.childCount;

		ResizeSessionList(count + 1);

		SetSessionInfoIndex(
			count,
			$"temp {count + 1}",
			0,
			0,
			"");
	}

	public void AddTempSession_10()
	{
		int count = _scrollRect.content.childCount;

		ResizeSessionList(count + 10);

		for (int i = 0; i < 10; i++)
		{
			SetSessionInfoIndex(
				count + i,
				$"temp {count + i}",
				0,
				0,
				"");
		}
	}

	public void ClearSessionList()
	{
		ResizeSessionList(0);
	}
#endif
}
