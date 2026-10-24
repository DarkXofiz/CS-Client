using UnityEngine;

// Sahnede "AndroidCommandBridge" adli bir GameObject'e AndroidCommandBridge ile birlikte ekle.
// Android build'inde Java receiver'i kaydeder; boylece overlay komutlari Unity'ye ulasir.
public class CsClientAndroidInstaller : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject activity;
    private AndroidJavaClass receiver;

    private void Start()
    {
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            receiver = new AndroidJavaClass("com.wetsuki.csclient.CsClientReceiver");
            receiver.CallStatic("register", activity);
        }
        catch (System.Exception e) { Debug.LogWarning("CsClient receiver kaydedilemedi: " + e.Message); }
    }

    private void OnDestroy()
    {
        try { if (receiver != null) receiver.CallStatic("unregister", activity); }
        catch (System.Exception) { }
    }
#endif
}
