using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRoomList : UIBehaviour, IRoomListUI
{
	[SerializeField]
	UILobbySO _uiso;
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
	RoomItem _roomPrefap;

	Dictionary<int, RoomItem> _roomListCache = new();
	int _tempIndex = 0;

	protected override void Start()
	{
		_uiso.RoomList = this;

		_createRoomButton.onClick.AddListener(() => _uiso.RaiseOnClickCreateRoom());
		_settingButton.onClick.AddListener(() => _uiso.RaiseOnClickSettings());
		_refreshButton.onClick.AddListener(() => _uiso.RaiseOnClickRefresh());
		_exitButton.onClick.AddListener(() => _uiso.RaiseOnClickExit());
	}

	void OnCreateRoom()
	{
		print("OnCreateRoom");

		RoomItem roomItem = Instantiate(_roomPrefap);
		roomItem.RoomIndex = _tempIndex++;
		roomItem.OnClick += OnClickRoomEntry;

		roomItem.transform.SetParent(_scrollRect.content);
		_roomListCache.Add(roomItem.RoomIndex, roomItem);
		roomItem.FitSize(_scrollRect.content.GetComponent<RectTransform>());
		roomItem.transform.localScale = Vector3.one;
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

	void ClearRoomList()
	{
		foreach (var item in _roomListCache)
		{
			item.Value.ClearButtonEvent();
			Destroy(item.Value);
		}
	}

	void RefreshRoomList()
	{
		foreach (var item in _roomListCache)
		{
			
		}

		_scrollRect.verticalNormalizedPosition = 1f;
	}

	void OnClickRoomEntry(int roomIndex)
	{
		print($"OnClickRoomEntry {roomIndex}");
	}

	protected override void OnRectTransformDimensionsChange()
	{
		if (_roomListCache.Count == 0)
		{
			return;
		}
		print("fitsize");
		RectTransform rectTransform = _scrollRect.content.GetComponent<RectTransform>();

		foreach (var item in _roomListCache)
		{
			item.Value.FitSize(rectTransform);
		}
	}

	public async Awaitable SetRoomItem(M_ResponseSessionList rri)
	{
		print($"SetRoomItem count:{rri.Count}, list item count:{rri.List.Count}");

		if (rri.Count == 0 || rri.List.Count == 0)
		{
			return;
		}

		Dictionary<int, RoomItem> roomList = new();

		foreach (var ri in rri.List)
		{
			//roomList.Add(item.RoomIndex, item.RoomName);
			RoomItem roomItem = Instantiate(_roomPrefap);
			roomItem.RoomIndex = ri.SessionIndex;
			roomItem.OnClick += OnClickRoomEntry;
		}

		_roomListCache = roomList;

		await Awaitable.MainThreadAsync();

		ClearRoomList();

		RefreshRoomList();
		//go.transform.SetParent(_scrollRect.content.transform);

		//RefreshRoomList();
	}

	protected override void OnDestroy()
	{
		_createRoomButton.onClick.RemoveAllListeners();
		_settingButton.onClick.RemoveAllListeners();
		_refreshButton.onClick.RemoveAllListeners();
		_exitButton.onClick.RemoveAllListeners();
	}

	public void SetRoomList(M_ResponseSessionList rrl)
	{
		throw new System.NotImplementedException();
	}
}
