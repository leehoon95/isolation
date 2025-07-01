using Google.Protobuf;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class RemoteCharacterManager : MonoBehaviour
{
    [SerializeField]
	PlayerInfoSO _pinfo;
    [SerializeField]
    Character _character;
    [SerializeField]
    GameObject _remoteCharacterPrefab;
    [SerializeField]
    UDPClientSO _udpClient;

    public bool Synchronize
    {
        get; set;
    }

	Dictionary<int, GameObject> _remoteCharacters = new Dictionary<int, GameObject>();
    float _t;
    float _interval = 0.02f;
    Vector2 _clientPositionOnServer = Vector2.zero;
    
    public void SyncTransform(PROTO_ObjectTransform tr)
    {
        if (_pinfo.ClientIndex == tr.ClientIndex)
        {
			_clientPositionOnServer = new Vector2(tr.X, tr.Y);
            print($"cp on server: {_clientPositionOnServer}");
		}
        else
        {
            MainThreadDispatcher.Enqueue(() =>
            {
				GameObject remoteCharacter;
				if (_remoteCharacters.TryGetValue(tr.ClientIndex, out remoteCharacter))
				{
					remoteCharacter.transform.position = new Vector2(tr.X, tr.Y);
				}
				else
				{
					var obj = Instantiate(_remoteCharacterPrefab);
					obj.transform.position = new Vector2(tr.X, tr.Y);
					_remoteCharacters[tr.ClientIndex] = obj;
				}
			});
        }
    }

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(_clientPositionOnServer, 0.5f);
        //Gizmos.DrawCube(_clientPositionOnServer, 0.5f);
	}

	void FixedUpdate()
	{
        if (Synchronize)
        {
            _t += Time.fixedDeltaTime;

            if (_t > _interval)
            {
                _t -= _interval;

                var rcp = new PROTO_ReportCharacterPhysics();
                rcp.RoomIndex = _pinfo.roomIndex;
                rcp.Transform = new PROTO_ObjectTransform()
                {
                    ClientIndex = _pinfo.ClientIndex,
                    X = _character.transform.position.x,
                    Y = _character.transform.position.y,
                };

                _ = _udpClient.SendUDPDataAsync(
                    PROTO_MessageType.ReportCharacterPhysics,
                    rcp.ToByteArray());
                //print($"sent udp data(client {_pinfo.ClientIndex})");
            }
        }
	}

    public PROTO_ObjectTransform GetCharacterTransform()
    {
        var tr = _character.transform;

        return new PROTO_ObjectTransform()
        {
            ClientIndex = _pinfo.ClientIndex,
            X = tr.position.x,
            Y = tr.position.y,
        };
    }
}
