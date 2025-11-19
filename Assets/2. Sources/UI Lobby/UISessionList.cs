using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISessionList : UIBehaviour, ISessionListUI
{
	
	[SerializeField]
	ScrollRect _scrollRect;
	[SerializeField]
	Button _createSessionButton;
	[SerializeField]
	Button _settingButton;
	[SerializeField]
	Button _refreshButton;
	[SerializeField]
	Button _exitButton;
	[SerializeField]
	SessionItem _sessionPrefap;
	[SerializeField]
	GameObject _emptySessionListNotification;

	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.SessionList = this;
		
		_createSessionButton.onClick.AddListener(() => _uiso.RaiseOnClickCreateSession());
		_settingButton.onClick.AddListener(() => _uiso.RaiseOnClickSettings());
		_refreshButton.onClick.AddListener(() => _uiso.RaiseOnClickRefresh());
		_exitButton.onClick.AddListener(() => _uiso.RaiseOnClickExit());
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

	protected override void OnRectTransformDimensionsChange()
	{
		if (_scrollRect.content.childCount == 0)
		{
			return;
		}

		RectTransform rectTransform = _scrollRect.content.GetComponent<RectTransform>();

		int count = _scrollRect.content.childCount;

		for (int i = 0; i < count; i++)
		{
			var si = _scrollRect.content.GetChild(i).GetComponent<SessionItem>();
			si.FitSize(rectTransform);
		}
	}

	protected override void OnDestroy()
	{
		_createSessionButton.onClick.RemoveAllListeners();
		_settingButton.onClick.RemoveAllListeners();
		_refreshButton.onClick.RemoveAllListeners();
		_exitButton.onClick.RemoveAllListeners();
	}

	public void SetSessionInfoIndex(
		int index,
		string name,
		int maxPlayerCount,
		int playerCount,
		string lobbyId)
	{
		var sitem = _scrollRect.content.GetChild(index).GetComponent<SessionItem>();

		sitem.SetSessionInfo(
			name,
			maxPlayerCount,
			playerCount,
			lobbyId);
	}

	/*
	 * session item개수를 조절하고 가능하면 재활용함
	 */
	public void ResizeSessionList(int minimumSession)
	{
		Debug.Log($"ResizeSessionList {minimumSession}");
		if (minimumSession > _scrollRect.content.childCount)
		{
			int countToAdd = minimumSession - _scrollRect.content.childCount;
			var rt = _scrollRect.content.GetComponent<RectTransform>();

			for (int i = 0; i < countToAdd; i++)
			{
				SessionItem sessionItem = Instantiate(_sessionPrefap);
				sessionItem.OnClick += OnClickSessionEntry;
				sessionItem.transform.SetParent(_scrollRect.content);
				sessionItem.FitSize(rt);
				sessionItem.transform.localScale = Vector3.one;

				Debug.Log("add session prefab!");
			}

			return;
		}
		else if (minimumSession < _scrollRect.content.childCount)
		{
			int countToRemove = _scrollRect.content.childCount - minimumSession;

			for (int i = 0; i < countToRemove; i++)
			{
				Destroy(_scrollRect.content.GetChild(i).gameObject);
				Debug.Log("destroy session prefab!");
			}
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
#endif
}
