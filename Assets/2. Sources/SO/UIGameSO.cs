using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UIGameSO", menuName = "Scriptable Objects/UIGameSO")]
public class UIGameSO : ScriptableObject
{
    public event Action<int> OnTestEvent;

    public void RaiseTestEvent(int index) => OnTestEvent?.Invoke(index);
}

public class UIGameSOHolder : SOHolderSinglton<UIGameSO, UIGameSOHolder>
{}