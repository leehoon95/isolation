using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "UDPClientSO", menuName = "Scriptable Objects/UDPClientSO")]
public class UDPClientSO : ScriptableObject
{
	UdpClient _udpClient;
	IPEndPoint _udpEndPoint = new IPEndPoint(IPAddress.Parse("172.23.12.33"), 51022);
	CancellationTokenSource _cancelToken;
	Action<byte[], int> _onReceived;

	Task _receivingTask;

	bool IsRunning
	{
		get
		{
			return (_receivingTask != null && _receivingTask?.Status == TaskStatus.Running);
		}
	}
	
	public void RunReceiving(string adress = "127.0.0.1", int port = 51022)
	{
		if (IsRunning)
		{
			return;
		}

		_udpClient = new UdpClient();
		//_udpEndPoint = new IPEndPoint(IPAddress.Parse(adress), port);
		_cancelToken = new ();
		_receivingTask = Task.Run(ReceivingUDPDataTask);
#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
	}


	public void StopReceiving()
	{
		_udpClient?.Close();
		_cancelToken?.Cancel();
		_cancelToken.Dispose();
		_cancelToken = null;
		//_udpEndPoint = null;
#if UNITY_EDITOR
		EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif
	}

#if UNITY_EDITOR
	void OnPlayModeChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			StopReceiving();
		}
	}
#endif

	public async Task ReceivingUDPDataTask()
	{
		while (!_cancelToken.IsCancellationRequested)
		{
			try
			{
				var result = await _udpClient.ReceiveAsync();
				_onReceived?.Invoke(result.Buffer, result.Buffer.Length);
				//Debug.Log("udp received");
			}
			catch (SocketException se)
			{
				Debug.LogError($"UDP Receiving socket exception: {se.ErrorCode}");
				break;
			}
			catch (Exception ex)
			{
				Debug.LogError($"UDP Receiving exception: {ex.Message}.");
				break;
			}
		}

		StopReceiving();
	}

	public async Task SendUDPDataAsync(int type, byte[] data)
	{
		try
		{
			int length = data.Length + 12;
			//int typeInteger = Convert.ToInt32(type);

			byte[] buffer = new byte[data.Length + 12];

			var prot = Encoding.ASCII.GetBytes("prot");

			// |prot|type|length|serialized data|
			prot.CopyTo(buffer.AsSpan(0));
			MemoryMarshal.Write(buffer.AsSpan(4), ref type);
			MemoryMarshal.Write(buffer.AsSpan(8), ref length);
			data.CopyTo(buffer.AsSpan(12));

			int res = await _udpClient.SendAsync(buffer, length, _udpEndPoint);

			//Debug.Log($"UDP Send res: {res}");
		}
		catch (Exception ex)
		{
			Debug.LogError("SendUDPDataAsync() Exception: " + ex.Message);
		}
	}

	public void AddReceiveListner(Action<byte[], int> listner)
	{
		_onReceived += listner;
	}

	public void RemoveReceiveListner(Action<byte[], int> listner)
	{
		_onReceived -= listner;
	}
}
