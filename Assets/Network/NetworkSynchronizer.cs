using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkSynchronizer : MonoBehaviour
{
	TcpClient _tcpClient;
	NetworkStream _networkStream;
	CancellationTokenSource _cancelToken;
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

			await Task.Run(ReceiveLoop);

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
		byte[] buffer = new byte[1024];

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
				
				string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);


				//serverEventSO.RaiseServerMessageReceived($"Server:{message}");
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
	}


	void CloseConnection()
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

			//serverEventSO.RaiseServerDisconnected("Disconnected from server!");
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
}
