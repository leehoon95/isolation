using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{
	[SerializeField]
	UILobbySO _uiSO;
	[SerializeField]
	ScrollRect _scrollRect;
	[SerializeField]
	Button _createRoomButton;
	[SerializeField]
	Button _settingButton;
	[SerializeField]
	Button _refreshButton;
	[SerializeField]
	Button _exitButton;
	[SerializeField]
	GameObject _roomPrefap;


	void Start()
	{
		_createRoomButton.onClick.AddListener(OnCreateRoom);
		_settingButton.onClick.AddListener(OnSetting);
		_refreshButton.onClick.AddListener(OnRefresh);
		_exitButton.onClick.AddListener(OnExit);
	}

	void OnCreateRoom()
	{
		print("OnCreateRoom");
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

	public async Awaitable SetRoomItem(Dictionary<int, GameObject> rooms)
	{
		await Awaitable.MainThreadAsync();

		//go.transform.SetParent(_scrollRect.content.transform);

		//RefreshRoomList();
	}
}
