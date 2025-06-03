using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ChatPannel : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI chatText;
	[SerializeField]
	ScrollRect chatScrollRect;
	[SerializeField]
	ServerEventSO serverEventSO;

	public void ShowMessage(string data)
    {
		//print($"show {data}");
		//ChatText.text += ChatText.text == "" ? data : "\n" + data;
		chatText.text += data;

		if (chatText.text.Length > 2000)
		{
			// Remove the first 100 characters if the text exceeds 1000 characters
			chatText.text = chatText.text.Substring(200);
		}

		Invoke("ScrollDelay", 0.1f);
	}

    void ScrollDelay() => chatScrollRect.verticalScrollbar.value = 0f;

	private void Start()
	{
		serverEventSO.OnServerMessageReceived += (string message) => {
			ShowMessage(message);
		};
	}

	void Update()
    {
		//if (Input.GetKeyDown(KeyCode.Space))
		//{
		//	ShowMessage("test" + Random.Range(0, 1000));
		//}
	}
}
