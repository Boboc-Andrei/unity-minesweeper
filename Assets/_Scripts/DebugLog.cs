using UnityEngine;

public class DebugLog : MonoBehaviour
{
    public static void Log(string message) {
        Debug.Log(message);
    }

    public static void LogError(string message) {
        Debug.LogError(message);
    }
}
