using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "TCPClientSO", menuName = "Scriptable Objects/TCPClientSO")]
public class TCPClientSO : ScriptableObject
{
	[SerializeField]
	public string ServerAddress;
	[SerializeField]
	public int Port;

	TcpClient _tcpClient;
	NetworkStream _networkStream;
	CancellationTokenSource _cancelToken;

	Func<byte[], int, Awaitable> _onReceived;

	public event Func<byte[], int, Awaitable> OnReceived
	{
		add { _onReceived += value; } 
		remove { _onReceived -= value; }
	}


	public bool Connnected
	{
		get
		{
			return (_tcpClient != null && _tcpClient.Connected);
		}
	}

	public async Task<bool> ConnectToServer()
	{
		try
		{
			if (_tcpClient != null && _tcpClient.Connected)
			{
				Debug.Log("Already connected to server");
				return true;
			}

			Debug.Log("ConnectToServer");
			_tcpClient = new TcpClient();
			await _tcpClient.ConnectAsync(ServerAddress, Port);

			_tcpClient.NoDelay = true;
			_tcpClient.LingerState = new LingerOption(false, 0);
			_networkStream = _tcpClient.GetStream();

			_cancelToken = new ();

			_ = Task.Run(ReceivingTask);

#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif

			Debug.Log("Connected to server!");

			return true;
		}
		catch (SocketException se)
		{
			Debug.LogError($"TCP Receiving socket exception: {se.SocketErrorCode}");
			return false;
		}
		catch (System.Exception ex)
		{
			Debug.LogError("ConnectToServer() Exception: " + ex.Message);
			return false;
		}
	}
	
#if UNITY_EDITOR
	void OnPlayModeChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			CloseConnection();
		}
	}
#endif

	async Task ReceivingTask()
	{
		byte[] buffer = new byte[4096];
		// destroyCancellationToken: MonoBehavior 파생 클래스 전용
		while (!_cancelToken.IsCancellationRequested)
		{
			try
			{
				int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, _cancelToken.Token);
				if (bytesRead == 0)
				{
					Debug.LogWarning("receive 0 byte");

					if (_onReceived != null)
					{
						_ = _onReceived.Invoke(null, 0);
					}

					break; // Connection closed
				}

				string header = System.Text.Encoding.UTF8.GetString(buffer, 0, 4);

				if (header != "prot")
				{
					Debug.Log("Received a invalid message from server...");
					continue;
				}

				Debug.Log($"tcp data received! {bytesRead}");

				if (_onReceived != null)
				{
					_ = _onReceived.Invoke(buffer, bytesRead);
				}

				//string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
			}
			catch (OperationCanceledException e)
			{
				Debug.Log($"TCPClientSO.ReceivingTask() cancelled. {e.Message}");
				break;
			}
			catch (Exception ex)
			{
				Debug.Log("TCPClientSO.ReceivingTask() Exception: " + ex.Message);
				break;
			}
		}

		CloseConnection();
	}

	public void CloseConnection()
	{
		Debug.Log("CloseConnection()");

		if (_tcpClient == null || !_tcpClient.Connected)
		{
			return;
		}

		try
		{
			_onReceived = null;

			_cancelToken?.Cancel();
			_cancelToken?.Dispose();
			_cancelToken = null;

			_networkStream?.Close();
			_networkStream?.Dispose();
			_networkStream = null;

			_tcpClient?.Close();
			_tcpClient?.Dispose();
			_tcpClient = null;

#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif

		}
		catch (System.Exception ex)
		{
			Debug.Log("CloseConnection() Exception: " + ex.Message);
		}
	}

	//public bool AddReceiveListner(Func<byte[], int, Awaitable> listner)
	//{
	//	Debug.Log("AddReceiveListner!!");
	//	_onReceived += listner;
		
	//	return true;
	//}

	//public void RemoveReceiveListner(Func<byte[], int, Awaitable> listner)
	//{
	//	Debug.Log("RemoveReceiveListner!!");
	//	_onReceived -= listner;
	//}

	//public bool AddOnDisconnectedListner(Action<byte[], int> listner)
	//{
	//	_onReceived += listner;

	//	return true;
	//}

	//public void RemoveOnDisconnectedListner(Action<byte[], int> listner)
	//{
	//	_onReceived -= listner;
	//}

	public async Task<bool> SendStringAsync(int type, string str)
	{
		return await SendDataAsync(type, Encoding.UTF8.GetBytes(str));
	}

	public async Task<bool> SendDataAsync(int type, byte[] data)
	{
		if (_tcpClient != null && _tcpClient.Connected)
		{
			try
			{
				int length = data.Length + 12;
				int typeInteger = Convert.ToInt32(type);

				byte[] buffer = new byte[data.Length + 12];

				var prot = Encoding.ASCII.GetBytes("prot");

				// |prot|type|length|serialized data|
				prot.CopyTo(buffer.AsSpan(0));
				MemoryMarshal.Write(buffer.AsSpan(4), ref typeInteger);
				MemoryMarshal.Write(buffer.AsSpan(8), ref length);
				data.CopyTo(buffer.AsSpan(12));

				await _networkStream.WriteAsync(buffer, 0, length);

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("WriteByteAsync() Exception: " + ex.Message);
			}
		}

		return false;
	}

	private void OnDestroy()
	{
		Debug.Log("TCPClientSO.OnDestroy() called.");
	}
}
