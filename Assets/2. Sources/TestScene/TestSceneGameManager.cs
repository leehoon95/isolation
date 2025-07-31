using Google.Protobuf;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class TestSceneGameManager : MonoBehaviour
{
    [SerializeField]
    CameraManager _cm;
    [SerializeField]
    RemoteCharacterManager _rcm;
    [SerializeField]
	PlayerInfoSO _pinfo;
    [SerializeField]
    UITestSO _uiTest;
    [SerializeField] TCPClientSO _tcpClient;
    [SerializeField] UDPClientSO _udpClient;
	//NetworkSynchronizer _ns;

	void Start()
    {
        //_ns = FindAnyObjectByType(typeof(NetworkSynchronizer)) as NetworkSynchronizer;

        //_ns.OnReceivedTCP += OnTCPDataReceived;
        _udpClient.AddReceiveListner(OnUDPDataReceived);
        _udpClient.RunReceiving("172.23.12.33", 51022);

        _tcpClient.AddReceiveListner(OnTCPDataReceived);
		//_ = SendRequestSync();
	}

    void Update()
    {
        
    }

	//async Task SendRequestSync()
 //   {
 //       PROTO_RequestSync rs = new PROTO_RequestSync();

 //       rs.ClientIndex = _pinfo.ClientIndex;

 //       var data = rs.ToByteArray();

 //       //await _ns.SendTCPDataAsync(PROTO_MessageType.RequestSync, data);
 //       await _tcpClient.SendDataAsync(PROTO_MessageType.RequestSync, data);
	//}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
    {
		int type = BitConverter.ToInt32(buffer, 4);
	    if (type == (int)SM_Type.SmBcSync1)
        {
            SM_BCSync_1 bcs
                = SM_BCSync_1.Parser.ParseFrom(buffer, 12, length - 12);
            await Task.Run(() => { });
            print($"message from server: {bcs.Message}");
   //         string roomInde = bcs.Message;
   //         print($"roomIndex : {roomIndex}");
                
   //         if (roomIndex > 0)
   //         {
   //             _pinfo.roomIndex = roomIndex;
                
   //             _rcm.Synchronize = true;
			//	print($"sync on!!!");
			//}
		}
    }

    void OnUDPDataReceived(byte[] buffer, int length)
    {
		SM_Type type = (SM_Type)BitConverter.ToInt32(buffer, 4);
        print($"OnUDPDataReceived: {length}");
		if (type == SM_Type.SmBcSync1)
        {
            print($"UDP data received SyncCharacterPhysics({length} byte).");

            try
            {
                //PROTO_SyncCharacterPhysics scp
                //= PROTO_SyncCharacterPhysics.Parser.ParseFrom(buffer, 12, length - 12);

                //var transforms = scp.Transfoms;

                //foreach (var transform in transforms)
                //{
                //    _rcm.SyncTransform(transform);
                //}

            }
            catch (Exception ex)
            {
                Debug.LogError($"UDP Receiving exception: {ex.Message}.");
            }
		}

	}
}
