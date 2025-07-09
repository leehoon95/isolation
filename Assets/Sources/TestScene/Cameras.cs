using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor.Rendering;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    public List<CinemachineCamera> Cameras;

    int _cameraActivated = 0;

    public void ActivateNextCamera()
    {
        if (Cameras.Count < 2)
        {
            return;
        }

		_cameraActivated++;

		if (_cameraActivated >= Cameras.Count)
		{
			_cameraActivated = 0;
		}

		for (int i = 0; i < Cameras.Count; i++)
		{
			if (i != _cameraActivated)
			{
				Cameras[i].Priority = 0;
			}
			else
			{
				Cameras[i].Priority = 1;
			}
		}
	}
}
