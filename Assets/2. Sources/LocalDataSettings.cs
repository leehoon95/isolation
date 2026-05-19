using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class IsolationLocalData
{
	// 마지막으로
	public string Id = "";
	public string Password = "";
}

public class LocalDataSettings
{
	static LocalDataSettings _instance;
	static string _dataPath;

	IsolationLocalData _data = new();

	public IsolationLocalData Data
	{
		get { return _data; }
		set { _data = value; }
	}

	public static LocalDataSettings Instance
	{
		get { return _instance; }
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void Init()
	{
		_instance = new();
		_dataPath = Path.Combine(Application.persistentDataPath, "SaveData.json");
	}

	public async Task LoadAsync()
	{
		try
		{
			if (File.Exists(_dataPath))
			{
				var json = await File.ReadAllTextAsync(_dataPath);
				_data = JsonUtility.FromJson<IsolationLocalData>(json);
				//GLogger.Log($"--- Load Local Data ---\n{json}");
			}
			else
			{
				string json = JsonUtility.ToJson(_data, true);
				await File.WriteAllTextAsync(_dataPath, json);
			}
		}
		catch (Exception e)
		{
			GLogger.LogError($"LocalDataSettings.LoadSaveDataAsync Exception: {e.Message}");
		}
	}

	public async Task SaveAsync()
	{
		try
		{
			string json = JsonUtility.ToJson(_data, true);
			await File.WriteAllTextAsync(_dataPath, json);
			GLogger.Log($"--- Save Local Data ---\n{json}");
		}
		catch (Exception e)
		{
			GLogger.LogError($"LocalDataSettings.SaveAsync Exception: {e.Message}");
		}
	}
}
