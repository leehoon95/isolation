using Google.Protobuf;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Server : MonoBehaviour
{
	[SerializeField]
	ConnectPannel connectPannel;
	[SerializeField]
	ServerEventSO serverEventSO;
	[SerializeField]
	ChatPannel chatPannel;
	TcpClient tcpClient;
	NetworkStream networkStream;

	CancellationTokenSource cancellationTokenSource;

	public async Task ConnectToServer(string adress, int port)
	{
		try
		{
			if (tcpClient != null && tcpClient.Connected)
			{
				print("Already connected to server");
				return;
			}

			tcpClient = new TcpClient(); 

			await tcpClient.ConnectAsync(adress, port);

			tcpClient.NoDelay = true;
			tcpClient.LingerState = new LingerOption(false, 0);

			networkStream = tcpClient.GetStream();

			cancellationTokenSource = new CancellationTokenSource();

			serverEventSO.RaiseServerConnected("Connected to server!");
			//print("Connected to server!");

			await Task.Run(ReceiveLoop);
		}
		catch (System.Exception ex)
		{
			print("Error connecting to server: " + ex.Message);
		}
	}

	public void DisconnectFromServer()
	{
		CloseConnection();
	}

	public async Task SendProtobufDataToServer()
	{
		if (tcpClient != null && tcpClient.Connected)
		{
			//CharacterPositionList characterPositionList = new CharacterPositionList();
			//characterPositionList.Command = Command.CmdMove;

			//for (int i = 0; i < 10; i++)
			//{
			//	CharacterPosition characterPosition = new CharacterPosition();
			//	characterPosition.Id = i;
			//	characterPosition.X = 0.1f * i;
			//	characterPosition.Y = 0.2f * i;
			//	characterPosition.Z = 0.3f * i;
			//	characterPosition.Direction = 0.4f * i;
			//	characterPositionList.Characters.Add(characterPosition);
			//}

			//var serialized = characterPositionList.ToByteArray();

			//byte[] buffer = new byte[4 + 4 + serialized.Length];


			////int prot = 0x70726F74;
			////int prot = 0x746F7270; // "prot" in little-endian (0x70726F74)
			//byte[] prot = Encoding.ASCII.GetBytes("pro2");
			////MemoryMarshal.Write(buffer.AsSpan(), ref prot);
			//prot.CopyTo(buffer.AsSpan(0, 4));

			//int length = serialized.Length;
			//MemoryMarshal.Write(buffer.AsSpan(4), ref length);

			//serialized.CopyTo(buffer.AsSpan(8));

			//await networkStream.WriteAsync(buffer, 0, buffer.Length);

			int length = 1024 * 1024;

			byte[] buffer = new byte[length];

			int many = 0x796e616d; // "many"

			MemoryMarshal.Write(buffer.AsSpan(0, 4), ref many);

			MemoryMarshal.Write(buffer.AsSpan(4, 4), ref length);

			//MemoryMarshal.Write(buffer.AsSpan(4096, 4), ref many);

			await networkStream.WriteAsync(buffer, 0, buffer.Length);

			print($"Sent Protobuf data to server (serialized {length} byte)");
		}
		else
		{
			print("Not connected to server");
		}
	}

	async Task ReceiveLoop()
	{
		byte[] buffer = new byte[1024];

		while (!cancellationTokenSource.Token.IsCancellationRequested)
		{
			try
			{
				int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token);
				if (bytesRead == 0)
				{
					print("receive 0 byte");
					CloseConnection();

					break; // Connection closed
				}

				string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
				//print("Server:" + message);

				serverEventSO.RaiseServerMessageReceived($"Server:{message}");
			}
			catch (OperationCanceledException)
			{
				print("Receive loop cancelled");
				break; // Exit the loop if cancellation is requested
			}
			catch (Exception ex)
			{
				print("Error in ReceiveLoop: " + ex.Message);
				CloseConnection();
				break; // Exit the loop on error
			}
		}
	}

	public async void SendMessageToServer(string message)
	{
		if (tcpClient != null && tcpClient.Connected)
		{
			byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
			//networkStream.Write(data, 0, data.Length);
			await networkStream.WriteAsync(data, 0, data.Length);
			print("Sent: " + message);

			serverEventSO.RaiseServerMessageSent(message);
		}
		else
		{
			print("Not connected to server");
		}
	}

	void CloseConnection()
	{
		print("CloseConnection()");

		try
		{
			cancellationTokenSource?.Cancel();
			networkStream?.Close();
			networkStream = null;
			tcpClient?.Close();
			tcpClient = null;

			serverEventSO.RaiseServerDisconnected("Disconnected from server!");
		}
		catch (System.Exception ex)
		{
			print("Error closing connection: " + ex.Message);
		}
	}

	private void OnApplicationQuit()
	{
		CloseConnection();
	}

	//void Start()
	//{

	//}

	//void Update()
	//{

	//}
}
