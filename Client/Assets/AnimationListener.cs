using System;
using UnityEngine;

public class AnimationListener : MonoBehaviour
{
    public void EndStatus()
    {
        onEndStatus ?.Invoke();
    }

    public event Action onEndStatus;

    private void OnDestroy()
    {
        onEndStatus = null;
    }
}
