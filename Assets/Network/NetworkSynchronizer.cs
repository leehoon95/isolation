using Google.Protobuf.WellKnownTypes;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkSynchronizer : MonoBehaviour
{
	TcpClient _tcpClient;
	NetworkStream _networkStream;
	CancellationTokenSource _cancelToken;

	UdpClient _udpClient;
	IPEndPoint _udpEndPoint;
	bool _runUDPReceive;

	public event Action<byte[], int> OnReceivedTCP;
	public event Action<byte[]> OnReceivedUDP;

	public bool Connected => _tcpClient != null && _tcpClient.Connected;

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

			_ = Task.Run(ReceiveLoop);

			Debug.Log("Connected to server!");

			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogError("ConnectToServer() Exception: " + ex.Message);
			return false;
		}
	}

	async Task ReceiveLoop()
	{
		byte[] buffer = new byte[4096];

		while (!_cancelToken.Token.IsCancellationRequested)
		{
			try
			{
				int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, _cancelToken.Token);
				if (bytesRead == 0)
				{
					print("receive 0 byte");
					CloseConnection();

					break; // Connection closed
				}

				string header = System.Text.Encoding.UTF8.GetString(buffer, 0, 4);

				if (header != "prot")
				{
					print("Received a invalid message from server...");
					continue;
				}

				OnReceivedTCP?.Invoke(buffer, bytesRead);

				//string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
			}
			catch (OperationCanceledException)
			{
				print("ReceiveLoop() Receive loop cancelled.");
				break; // Exit the loop if cancellation is requested
			}
			catch (Exception ex)
			{
				print("ReceiveLoop() Exception: " + ex.Message);
				CloseConnection();
				break; // Exit the loop on error
			}
		}

		print("ReceiveLoop has canceled.");
	}

	//public void WriteAsync(PROTO_MessageType type, byte[] data)
	//{
	//	_ = WriteByteImpl(type, data);
	//}

	// use for debug.
	public async Task SendTCPStringAsync(int type, string str)
	{
		await SendTCPDataAsync(type, Encoding.UTF8.GetBytes(str));
	}

	public async Task SendTCPDataAsync(int type, byte[] data)
	{
		if (_tcpClient != null && _tcpClient.Connected)
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

				await _networkStream.WriteAsync(buffer, 0, length);
			}
			catch (Exception ex)
			{
				Debug.LogError("WriteByteAsync() Exception: " + ex.Message);
			}
		}
	}

	public void CloseConnection()
	{
		print("CloseConnection()");

		if (_tcpClient == null || !_tcpClient.Connected)
		{
			print("CloseConnection() - TcpClient is null or not connected");
			return;
		}

		try
		{
			_cancelToken?.Cancel();
			_networkStream?.Close();
			_networkStream = null;
			_tcpClient?.Close();
			_tcpClient = null;

			print("Disconnected from the server.");
		}
		catch (System.Exception ex)
		{
			print("CloseConnection() Exception: " + ex.Message);
		}
	}

	void Awake()
	{
		var obj = FindAnyObjectByType<NetworkSynchronizer>();

		if (obj != null && obj != this)
		{
			Destroy(obj.gameObject);
			return;
		}
		else
		{
			DontDestroyOnLoad(gameObject);
		}
	}

	public void StartUDPReceive()
	{
		_udpClient = new UdpClient();
		_runUDPReceive = true;
		_udpEndPoint = new IPEndPoint(IPAddress.Parse("172.23.12.33"), 51022);
		_ = ReceiveUDPData();
	}

	public void StopUDPReceive()
	{
		_udpClient?.Close();
		_runUDPReceive = false;
	}

	async Task ReceiveUDPData()
	{
		while (_runUDPReceive)
		{
			try
			{
				print("Waiting for UDP data...");
				var result = await _udpClient.ReceiveAsync();
				print($"UDP received {result.RemoteEndPoint.Address.ToString()}:{result.RemoteEndPoint.Port}");
				OnReceivedUDP?.Invoke(result.Buffer);
			}
			catch (Exception ex)
			{
				Debug.LogError("ReceiveUDPData() Exception: " + ex.Message);
				//break; // Exit the loop on error
			}
		}

		print("ReceiveUDPData() has stopped.");
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

			int res = await _udpClient.SendAsync(data, data.Length, _udpEndPoint);
			print($"UDP Send res: {res}");
		}
		catch (Exception ex)
		{
			Debug.LogError("SendUDPDataAsync() Exception: " + ex.Message);
		}
	}
}
