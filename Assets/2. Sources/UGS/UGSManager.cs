using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UGSManager : MonoBehaviour
{
	public string PlayerId
	{
		get => IsInitialized() ? AuthenticationService.Instance.PlayerId : null;
	}

	public static bool IsInitialized()
		=> (UnityServices.State == ServicesInitializationState.Initialized)
		&& AuthenticationService.Instance.IsSignedIn;

	public static async Task InitServices()
	{

		if (UnityServices.State != ServicesInitializationState.Initialized)
		{
			await UnityServices.InitializeAsync();
		}

		try
		{
			if (!AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
			}

			Debug.LogWarning($"Signed Player ID: {AuthenticationService.Instance.PlayerId}");
		}
		catch (AuthenticationException ex)
		{
			Debug.LogWarning($"RelayManager.InitServices AuthenticationException. ErrorCode: {ex.ErrorCode}");
		}
		catch (RequestFailedException ex)
		{
			Debug.LogWarning($"RelayManager.InitServices RequestFailedException. ErrorCode: {ex.ErrorCode}");
		}
	}
}
