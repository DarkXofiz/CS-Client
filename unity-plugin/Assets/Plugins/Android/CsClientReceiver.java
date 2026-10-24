package com.wetsuki.csclient;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;

import com.unity3d.player.UnityPlayer;

/**
 * Overlay servisinden (OverlayService) gelen broadcast'leri Unity'ye iletir.
 * Bu dosya, overlay ile AYNI APK'ya paketlenen Unity projesinde calisir.
 */
public class CsClientReceiver extends BroadcastReceiver {
    public static final String ACTION_TOGGLE_GUI = "com.wetsuki.csclient.TOGGLE_GUI";
    public static final String ACTION_COMMAND = "com.wetsuki.csclient.COMMAND";
    private static final String UNITY_OBJECT = "AndroidCommandBridge";
    private static final String UNITY_METHOD = "ReceiveCommand";

    private static CsClientReceiver instance;

    /** Unity'den: Activity ile bir kez cagrilir. */
    public static synchronized void register(Activity activity) {
        if (instance != null || activity == null) return;
        instance = new CsClientReceiver();
        IntentFilter filter = new IntentFilter();
        filter.addAction(ACTION_TOGGLE_GUI);
        filter.addAction(ACTION_COMMAND);
        Context app = activity.getApplicationContext();
        if (Build.VERSION.SDK_INT >= 33) {
            app.registerReceiver(instance, filter, Context.RECEIVER_NOT_EXPORTED);
        } else {
            app.registerReceiver(instance, filter);
        }
    }

    public static synchronized void unregister(Activity activity) {
        if (instance == null || activity == null) return;
        try { activity.getApplicationContext().unregisterReceiver(instance); }
        catch (Exception ignored) { }
        instance = null;
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        if (intent == null || intent.getAction() == null) return;
        String cmd;
        if (ACTION_TOGGLE_GUI.equals(intent.getAction())) {
            cmd = "toggle";
        } else {
            cmd = intent.getStringExtra("cmd");
            if (cmd == null) return;
        }
        UnityPlayer.UnitySendMessage(UNITY_OBJECT, UNITY_METHOD, cmd);
    }
}
