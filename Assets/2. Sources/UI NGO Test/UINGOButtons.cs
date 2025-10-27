using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINGOButton : UIBehaviour, INGOTestButtonUI
{
	[SerializeField] Button _1;
	[SerializeField] Button _2;
	[SerializeField] Button _3;
	[SerializeField] Button _4;
	[SerializeField] Button _5;
	[SerializeField] Button _6;

	UINGOTestSO _uiso;

	protected override void Start()
	{
		base.Start();

		_uiso = FindAnyObjectByType<UINGOTestSOHolder>().Data;
		_uiso.NGOTestButton = this;

		_1.onClick.AddListener(() => _uiso.Raise_1());
		_2.onClick.AddListener(() => _uiso.Raise_2());
		_3.onClick.AddListener(() => _uiso.Raise_3());
		_4.onClick.AddListener(() => _uiso.Raise_4());
		_5.onClick.AddListener(() => _uiso.Raise_5());
		_6.onClick.AddListener(() => _uiso.Raise_6());
	}
}
