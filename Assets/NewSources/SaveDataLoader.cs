using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class SaveData
{
	public string nickName = "";
	public int level = 0;
}

public class SaveDataLoader : MonoBehaviour
{
	[SerializeField]
	UILoginServiceLocatorSO _uisl;
	string _saveFilePath;
	SaveData _SDOrigin = new SaveData();
	SaveData _SDCached = new SaveData();
	public SaveData SaveData
	{
		get { return _SDCached; }
		set { _SDCached = value; }
	}

	public void LoadSaveData()
	{
		try
		{
			print("LoadSaveData() called");

			if (File.Exists(_saveFilePath))
			{
				var json = File.ReadAllText(_saveFilePath);
				_SDOrigin = JsonUtility.FromJson<SaveData>(json);

				print(json);
			}
			else
			{
				string json = JsonUtility.ToJson(_SDOrigin, true);
				File.WriteAllText(_saveFilePath, json);
			}

			_SDCached = _SDOrigin;
		}
		catch (Exception ex)
		{
			Debug.LogError("LoadSaveData Exception: " + ex.Message);
		}
	}

	public async Task LoadSaveDataAsync()
	{
		try
		{
			print("LoadSaveDataAsync() called");

			if (File.Exists(_saveFilePath))
			{
				var json = await File.ReadAllTextAsync(_saveFilePath);
				//string json = File.ReadAllText(_saveFilePath);
				_SDOrigin = JsonUtility.FromJson<SaveData>(json);

				print(json);
				//Debug.Log("Loaded Save Data: " + saveData._nickName + ", Level: " + saveData._level);
			}
			else
			{
				string json = JsonUtility.ToJson(_SDOrigin, true);
				await File.WriteAllTextAsync(_saveFilePath, json);
			}

			_SDCached = _SDOrigin;
		}
		catch (Exception ex)
		{
			Debug.LogError("LoadSaveDataAsync Exception: " + ex.Message);
		}
	}

	public void WriteSaveData()
	{
		try
		{
			print("WriteSaveData() called");

			_SDOrigin = _SDCached;

			string json = JsonUtility.ToJson(_SDCached, true);
			File.WriteAllText(_saveFilePath, json);
		}
		catch (Exception ex)
		{
			Debug.LogError("WriteSaveData Exception: " + ex.Message);
		}
	}

	public async Task WriteSaveDataAsync()
	{
		try
		{
			print("WriteSaveData() called");

			_SDOrigin = _SDCached;

			string json = JsonUtility.ToJson(_SDCached, true);
			await File.WriteAllTextAsync(_saveFilePath, json);
		}
		catch (Exception ex)
		{
			Debug.LogError("WriteSaveData Exception: " + ex.Message);
		}
	}

	void Awake()
	{
		var obj = FindAnyObjectByType<SaveDataLoader>();

		if (obj != null && obj != this)
		{
			Destroy(obj.gameObject);
			return;
		}
		else
		{
			print("SaveDataLoader Awake() called");
			_saveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

			LoadSaveData();

			DontDestroyOnLoad(gameObject);
		}
	}
}
