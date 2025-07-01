using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "TCPClientSO", menuName = "Scriptable Objects/TCPClientSO")]
public class TCPClientSO : ScriptableObject
{
	TcpClient _tcpClient;
	NetworkStream _networkStream;
	CancellationTokenSource _cancelToken;

	Action<byte[], int> _onReceived;
	Action _onDisconnected;

	public bool Connnected
	{
		get
		{
			return (_tcpClient != null && _tcpClient.Connected);
		}
	}

	public async Task<bool> ConnectToServer(string adress = "127.0.0.1", int port = 51010)
	{
		try
		{
			if (_tcpClient != null && _tcpClient.Connected)
			{
				Debug.Log("Already connected to server");
				return true;
			}
			
			_tcpClient = new TcpClient();
			await _tcpClient.ConnectAsync(adress, port);

			_tcpClient.NoDelay = true;
			_tcpClient.LingerState = new LingerOption(false, 0);
			_networkStream = _tcpClient.GetStream();

			_cancelToken = new CancellationTokenSource();

			_ = Task.Run(ReceivingTask);

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

	async Task ReceivingTask()
	{
		byte[] buffer = new byte[4096];

		while (!_cancelToken.Token.IsCancellationRequested)
		{
			try
			{
				int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, _cancelToken.Token);
				if (bytesRead == 0)
				{
					Debug.Log("receive 0 byte");
					CloseConnection();
					_onDisconnected?.Invoke();

					break; // Connection closed
				}

				string header = System.Text.Encoding.UTF8.GetString(buffer, 0, 4);

				if (header != "prot")
				{
					Debug.Log("Received a invalid message from server...");
					continue;
				}

				Debug.Log($"tcp data received! {bytesRead}");
				_onReceived?.Invoke(buffer, bytesRead);

				//string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
			}
			catch (OperationCanceledException)
			{
				Debug.Log("TCPClientSO.ReceivingTask() cancelled.");
				break;
			}
			catch (Exception ex)
			{
				Debug.Log("TCPClientSO.ReceivingTask() Exception: " + ex.Message);
				CloseConnection();
				break;
			}
		}
	}

	public void CloseConnection()
	{
		Debug.Log("CloseConnection()");

		if (_tcpClient == null || !_tcpClient.Connected)
		{
			Debug.Log("CloseConnection() - TcpClient is null or not connected");
			return;
		}

		try
		{
			_onReceived = null;
			_cancelToken?.Cancel();
			_networkStream?.Close();
			_networkStream = null;
			_tcpClient?.Close();
			_tcpClient = null;

			Debug.Log("Disconnected from the server.");
		}
		catch (System.Exception ex)
		{
			Debug.Log("CloseConnection() Exception: " + ex.Message);
		}
	}

	public bool AddReceiveListner(Action<byte[], int> listner)
	{
		_onReceived += listner;

		return true;
	}

	public void RemoveReceiveListner(Action<byte[], int> listner)
	{
		_onReceived -= listner;
	}

	public bool AddOnDisconnectedListner(Action<byte[], int> listner)
	{
		_onReceived += listner;

		return true;
	}

	public void RemoveOnDisconnectedListner(Action<byte[], int> listner)
	{
		_onReceived -= listner;
	}

	public async Task<bool> SendStringAsync(PROTO_MessageType type, string str)
	{
		return await SendDataAsync(type, Encoding.UTF8.GetBytes(str));
	}

	public async Task<bool> SendDataAsync(PROTO_MessageType type, byte[] data)
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
}
