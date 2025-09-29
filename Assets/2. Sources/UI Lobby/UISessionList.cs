using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISessionList : UIBehaviour, ISessionListUI
{
	[SerializeField]
	UILobbySO _uiso;
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
	
	//Dictionary<int, SessionItem> _roomListCache = new();
	//int _tempIndex = 0;

	protected override void Start()
	{
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

	void OnClickSessionEntry(int sessionIndex)
	{
		print($"OnClickRoomEntry {sessionIndex}");

		_uiso.RaiseOnClickSession(sessionIndex);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		Debug.Log("OnRectTransformDimensionsChange");

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

	protected override void OnValidate()
	{
		Debug.Log("onvalidate()");
	}

	public void SetSessionInfoIndex(
		int index,
		int sessionIndex,
		string name,
		int maxClientCount,
		int clientCount,
		string password,
		string joinCode)
	{
		var sitem = _scrollRect.content.GetChild(index).GetComponent<SessionItem>();

		sitem.SetSessionInfo(
			sessionIndex,
			name,
			maxClientCount,
			clientCount,
			password,
			joinCode
			);
	}

	/*
	 * session item개수를 조절하고 가능하면 재활용함
	 */
	public void ResizeSessionList(int minimumSession)
	{
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

#if UNITY_EDITOR
	public void AddTempSession()
	{
		int count = _scrollRect.content.childCount;

		ResizeSessionList(count + 1);

		SetSessionInfoIndex(
			count, 
			-1,
			$"temp {count + 1}",
			0,
			0,
			"",
			""
			);
	}
#endif
}
