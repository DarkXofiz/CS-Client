package com.wetsuki.csclient;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.net.VpnService;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity {
    private static final int REQ_OVERLAY = 101;
    private static final int REQ_VPN = 102;
    private boolean vpnAsked = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        root.setPadding(32, 48, 32, 32);

        TextView title = new TextView(this);
        title.setText("CsClient V11.9");
        title.setTextSize(28);
        root.addView(title);

        TextView info = new TextView(this);
        info.setText("Baslat: izinler istenir, VPN (anahtar) simgesi gorunur ve surukle-birak 'CS' tusu acilir.");
        root.addView(info);

        Button start = new Button(this);
        start.setText("Baslat (VPN + Menu)");
        start.setOnClickListener(v -> { vpnAsked = false; proceed(); });
        root.addView(start);

        Button stop = new Button(this);
        stop.setText("Durdur");
        stop.setOnClickListener(v -> {
            stopService(new Intent(this, OverlayService.class));
            stopService(new Intent(this, KeyVpnService.class));
        });
        root.addView(stop);

        setContentView(root);
    }

    /** Eksik izin varsa ister, hepsi tamamsa servisleri baslatir. */
    private void proceed() {
        if (!Settings.canDrawOverlays(this)) {
            startActivityForResult(new Intent(Settings.ACTION_MANAGE_OVERLAY_PERMISSION,
                    Uri.parse("package:" + getPackageName())), REQ_OVERLAY);
            return;
        }
        if (!vpnAsked) {
            vpnAsked = true;
            Intent vpn = VpnService.prepare(this);
            if (vpn != null) {
                startActivityForResult(vpn, REQ_VPN);
                return;
            }
        }
        // VPN izni verildiyse anahtar simgesi icin VPN servisini baslat (verilmediyse sadece menu acilir).
        if (VpnService.prepare(this) == null) {
            startService(new Intent(this, KeyVpnService.class));
        }
        Intent overlay = new Intent(this, OverlayService.class);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) startForegroundService(overlay);
        else startService(overlay);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == REQ_OVERLAY) {
            // Ayarlar ekrani her zaman RESULT_CANCELED doner; izne dogrudan bakilir.
            if (Settings.canDrawOverlays(this)) proceed();
            else Toast.makeText(this, "Diger uygulamalarin ustunde gosterme izni verilmedi", Toast.LENGTH_LONG).show();
        } else if (requestCode == REQ_VPN) {
            proceed();
        }
    }
}
