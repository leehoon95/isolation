using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

public class LocalizationLoader
{
	LocalizedString _localizedString;

	public void Init(string tableName)
	{
		_localizedString = new()
		{
			TableReference = tableName,
		};
	}

	public string GetLocalizedString(string key)
	{
		_localizedString.TableEntryReference = key;
		return _localizedString.GetLocalizedString();
	}

	public Task<string> GetLocalizedStringAsync(string key)
	{
		_localizedString.TableEntryReference = key;
		return _localizedString.GetLocalizedStringAsync().Task;
	}
}
