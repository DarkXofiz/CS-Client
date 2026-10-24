using System;
using UnityEngine;

// Add this component to a GameObject named "AndroidCommandBridge".
// An integrated Android plugin can call UnityPlayer.UnitySendMessage(
// "AndroidCommandBridge", "ReceiveCommand", "toggle").
public class AndroidCommandBridge : MonoBehaviour
{
    public static event Action<string> CommandReceived;

    public void ReceiveCommand(string command)
    {
        CommandReceived?.Invoke(command);
    }

    public void SetVisible(string value)
    {
        CommandReceived?.Invoke(value == "true" ? "show" : "hide");
    }
}
