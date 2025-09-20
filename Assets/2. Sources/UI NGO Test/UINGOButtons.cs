using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINGOButton : UIBehaviour, INGOTestButtonUI
{
	[SerializeField]
	UINGOTestSO _uiso;
	[SerializeField]
	Button _startHostButton;
	[SerializeField]
	Button _startClientButton;
	[SerializeField]
	Button _spawn1;
	[SerializeField]
	Button _shutdownButton;
	[SerializeField]
	Button _showStatusButton;

	protected override void Awake()
	{
		_uiso.NGOTestButton = this;

		_startHostButton.onClick.AddListener(() => _uiso.RaiseOnClickStartHost());
		_startClientButton.onClick.AddListener(() => _uiso.RaiseOnClickStartClient());
		_spawn1.onClick.AddListener(() => _uiso.RaiseOnClickSpawn());
		_shutdownButton.onClick.AddListener(() => _uiso.RaiseOnClickShutdown());
		_showStatusButton.onClick.AddListener(() => _uiso.RaiseOnClickShowStatus());
	}


}
