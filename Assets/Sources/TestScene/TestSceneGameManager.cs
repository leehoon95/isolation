using System;
using UnityEngine;

public class TestSceneGameManager : MonoBehaviour
{
    [SerializeField]
    CameraManager _cm;
    [SerializeField]
    RemoteCharacterManager _rcm;

	NetworkSynchronizer _ns;

	void Start()
    {
        _ns = FindAnyObjectByType(typeof(NetworkSynchronizer)) as NetworkSynchronizer;
        
        _ns.OnReceivedTCP += OnDataReceivedFromServer;
		SendRequestSync();
	}

    void Update()
    {
        
    }

	void SendRequestSync()
    {

    }

	void OnDataReceivedFromServer(byte[] buffer, int length)
    {
		PROTO_MessageType type = (PROTO_MessageType)BitConverter.ToInt32(buffer, 4);
	    //if (type == PROTO_MessageType.syncchara)
    }
}
