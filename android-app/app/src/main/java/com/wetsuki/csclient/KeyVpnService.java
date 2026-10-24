package com.wetsuki.csclient;

import android.content.Intent;
import android.net.VpnService;
import android.os.ParcelFileDescriptor;

/**
 * Yerel, bos bir VPN arayuzu acar. Sadece durum cubugunda anahtar (VPN) simgesi gosterir.
 * Sadece kendi uygulamamizi tunele aldigi icin diger uygulamalarin interneti etkilenmez.
 */
public class KeyVpnService extends VpnService {
    private ParcelFileDescriptor tunnel;

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (tunnel == null) {
            try {
                Builder b = new Builder();
                b.setSession("CsClient");
                b.addAddress("10.8.0.2", 32);
                b.addRoute("0.0.0.0", 0);
                b.addAllowedApplication(getPackageName());
                tunnel = b.establish();
            } catch (Exception e) {
                stopSelf();
            }
        }
        return START_STICKY;
    }

    private void closeTunnel() {
        if (tunnel != null) {
            try { tunnel.close(); } catch (Exception ignored) { }
            tunnel = null;
        }
    }

    @Override public void onRevoke() { closeTunnel(); stopSelf(); }

    @Override public void onDestroy() { closeTunnel(); super.onDestroy(); }
}
