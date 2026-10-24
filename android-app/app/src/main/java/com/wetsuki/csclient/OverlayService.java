package com.wetsuki.csclient;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.PixelFormat;
import android.graphics.Typeface;
import android.os.Build;
import android.os.IBinder;
import android.provider.Settings;
import android.util.DisplayMetrics;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.SeekBar;
import android.widget.TextView;
import android.widget.Toast;

/**
 * Suruklenebilir "CS" tusu + menu + ekranin ortasina cizilen cember/cizgiler.
 * Cember ve cizgiler artik bu uygulamanin kendisi tarafindan cizilir (Unity gerekmez).
 * Ayrica ayni komutlar "name:value" bicimiyle uygulama ici broadcast olarak Unity'ye de yollanir.
 */
public class OverlayService extends Service {
    public static final String ACTION_TOGGLE_GUI = "com.wetsuki.csclient.TOGGLE_GUI";
    public static final String ACTION_COMMAND = "com.wetsuki.csclient.COMMAND";
    private static final String CHANNEL_ID = "csclient_overlay";
    private static final String[] COLOR_NAMES = {"Kirmizi", "Yesil", "Camgobegi", "Sari", "Mor", "Beyaz"};
    private static final int[] COLORS = {Color.RED, Color.GREEN, Color.CYAN, Color.YELLOW, Color.MAGENTA, Color.WHITE};

    private WindowManager wm;
    private CircleView circleView;
    private View hLine, vLine, handle, menu;
    private WindowManager.LayoutParams handleParams, circleParams, hParams, vParams;
    private boolean circle = true, markers = true, labels = true, lines = false;
    private int colorIndex = 0, fov = 90, thickness = 2;

    /** Ekran ortasindaki kucuk, sabit boyutlu pencerede cemberi cizer. */
    private class CircleView extends View {
        private final Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);

        CircleView(Context c) {
            super(c);
            paint.setStyle(Paint.Style.STROKE);
        }

        @Override protected void onDraw(Canvas canvas) {
            float cx = getWidth() / 2f, cy = getHeight() / 2f;
            float r = Math.min(getWidth(), getHeight()) / 2f - dp(10);
            if (r < 1f) return;
            paint.setColor(COLORS[colorIndex]);
            paint.setStrokeWidth(thickness * getResources().getDisplayMetrics().density);
            canvas.drawCircle(cx, cy, r, paint);
        }
    }

    @Override public void onCreate() {
        super.onCreate();
        startAsForeground();
        wm = (WindowManager) getSystemService(WINDOW_SERVICE);

        if (!Settings.canDrawOverlays(this)) {
            Toast.makeText(this, "Ustte gosterme izni yok", Toast.LENGTH_LONG).show();
            stopSelf();
            return;
        }

        try {
            // 1) Ekran ortasinda sabit boyutlu, dokunmatik gecirgen katmanlar: cember + iki merkez cizgisi
            circleView = new CircleView(this);
            hLine = new View(this);
            vLine = new View(this);
            circleParams = passThrough();
            hParams = passThrough();
            vParams = passThrough();
            layoutOverlays();
            wm.addView(circleView, circleParams);
            wm.addView(hLine, hParams);
            wm.addView(vLine, vParams);
            refreshOverlays();

            // 2) Suruklenebilir "CS" tusu (cemberin ustunde)
            TextView h = new TextView(this);
            h.setText("CS"); h.setTextColor(Color.WHITE); h.setTextSize(14);
            h.setTypeface(Typeface.DEFAULT, Typeface.BOLD); h.setGravity(Gravity.CENTER);
            h.setBackgroundColor(Color.rgb(190, 30, 45));
            handle = h;
            handleParams = params(dp(48), dp(48));
            handleParams.gravity = Gravity.TOP | Gravity.START;
            handleParams.x = dp(8); handleParams.y = dp(120);
            h.setOnTouchListener(new View.OnTouchListener() {
                private int startX, startY; private float touchX, touchY; private boolean moved;
                @Override public boolean onTouch(View v, MotionEvent e) {
                    switch (e.getAction()) {
                        case MotionEvent.ACTION_DOWN:
                            startX = handleParams.x; startY = handleParams.y;
                            touchX = e.getRawX(); touchY = e.getRawY(); moved = false; return true;
                        case MotionEvent.ACTION_MOVE:
                            float dx = e.getRawX() - touchX, dy = e.getRawY() - touchY;
                            if (Math.abs(dx) > dp(6) || Math.abs(dy) > dp(6)) moved = true;
                            if (moved) {
                                handleParams.x = startX + (int) dx; handleParams.y = startY + (int) dy;
                                try { wm.updateViewLayout(handle, handleParams); } catch (Exception ignored) { }
                            }
                            return true;
                        case MotionEvent.ACTION_UP:
                            if (!moved) { if (menu == null) showMenu(); else hideMenu(); }
                            return true;
                    }
                    return false;
                }
            });
            wm.addView(handle, handleParams);
            Toast.makeText(this, "Daire hazir (" + circleParams.width + "px)", Toast.LENGTH_SHORT).show();
        } catch (Exception e) {
            Toast.makeText(this, "Overlay acilamadi: " + e.getMessage(), Toast.LENGTH_LONG).show();
            removeAll();
            stopSelf();
        }
    }

    /** startForegroundService ile baslatildigi icin 5 sn icinde cagrilmasi sart (yoksa uygulama coker). */
    private void startAsForeground() {
        NotificationManager nm = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
        Notification.Builder b;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            nm.createNotificationChannel(new NotificationChannel(CHANNEL_ID, "CsClient", NotificationManager.IMPORTANCE_LOW));
            b = new Notification.Builder(this, CHANNEL_ID);
        } else {
            b = new Notification.Builder(this);
        }
        int flags = PendingIntent.FLAG_UPDATE_CURRENT | (Build.VERSION.SDK_INT >= 23 ? PendingIntent.FLAG_IMMUTABLE : 0);
        PendingIntent open = PendingIntent.getActivity(this, 0, new Intent(this, MainActivity.class), flags);
        Notification n = b.setContentTitle("CsClient").setContentText("Menu acik")
                .setSmallIcon(android.R.drawable.ic_menu_manage).setContentIntent(open).setOngoing(true).build();
        startForeground(1, n);
    }

    private int overlayType() {
        return Build.VERSION.SDK_INT >= Build.VERSION_CODES.O
                ? WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY
                : WindowManager.LayoutParams.TYPE_PHONE;
    }

    private WindowManager.LayoutParams params(int w, int h) {
        return new WindowManager.LayoutParams(w, h, overlayType(),
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE | WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS,
                PixelFormat.TRANSLUCENT);
    }

    private int dp(int n) { return (int) (n * getResources().getDisplayMetrics().density + 0.5f); }

    /** Durum degisti: cemberi yeniden ciz ve Unity'ye de bildir. */
    private void changed(String command) {
        refreshOverlays();
        send(command);
    }

    /** Dokunmayi alta gecirir, ekranin tam ortasinda durur. */
    private WindowManager.LayoutParams passThrough() {
        WindowManager.LayoutParams p = new WindowManager.LayoutParams(
                dp(10), dp(10), overlayType(),
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
                        | WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE
                        | WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS,
                PixelFormat.TRANSLUCENT);
        p.gravity = Gravity.CENTER;
        if (Build.VERSION.SDK_INT >= 28) {
            p.layoutInDisplayCutoutMode = WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES;
        }
        return p;
    }

    /** Pencere boyutlarini ekranin gercek olcusune gore hesaplar (sabit piksel degerleri). */
    @SuppressWarnings("deprecation")
    private void layoutOverlays() {
        DisplayMetrics real = new DisplayMetrics();
        wm.getDefaultDisplay().getRealMetrics(real);
        float r = fov * Math.min(real.widthPixels, real.heightPixels) / 360f;
        int size = (int) (2 * r) + 2 * dp(10);
        circleParams.width = size; circleParams.height = size;
        int line = Math.max(1, dp(1));
        hParams.width = real.widthPixels; hParams.height = line;
        vParams.width = line; vParams.height = real.heightPixels;
    }

    /** Boyut, renk ve gorunurlugu uygular, pencereleri gunceller. */
    private void refreshOverlays() {
        if (circleView == null || hLine == null || vLine == null) return;
        layoutOverlays();
        hLine.setBackgroundColor(COLORS[colorIndex]);
        vLine.setBackgroundColor(COLORS[colorIndex]);
        circleView.setVisibility(circle ? View.VISIBLE : View.INVISIBLE);
        hLine.setVisibility(lines ? View.VISIBLE : View.INVISIBLE);
        vLine.setVisibility(lines ? View.VISIBLE : View.INVISIBLE);
        try {
            wm.updateViewLayout(circleView, circleParams);
            wm.updateViewLayout(hLine, hParams);
            wm.updateViewLayout(vLine, vParams);
        } catch (Exception ignored) { }
        circleView.invalidate();
    }

    @Override public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        refreshOverlays();
    }

    private void showMenu() {
        LinearLayout box = new LinearLayout(this); box.setOrientation(LinearLayout.VERTICAL);
        box.setPadding(dp(12), dp(10), dp(12), dp(10)); box.setBackgroundColor(Color.rgb(25, 27, 34));
        TextView title = new TextView(this); title.setText("CsClient V11.9"); title.setTextColor(Color.WHITE);
        title.setTextSize(20); title.setTypeface(Typeface.DEFAULT, Typeface.BOLD); box.addView(title);
        TextView note = new TextView(this); note.setText("Cember ekran ortasinda; hedef isaretleri Unity icindir");
        note.setTextColor(Color.LTGRAY); box.addView(note);

        addToggle(box, "FOV circle", circle, on -> { circle = on; changed("FOV circle:" + (on ? "on" : "off")); });
        addToggle(box, "Center lines", lines, on -> { lines = on; changed("Center lines:" + (on ? "on" : "off")); });
        addToggle(box, "Target markers (Unity)", markers, on -> { markers = on; changed("Target markers:" + (on ? "on" : "off")); });
        addToggle(box, "Distance labels (Unity)", labels, on -> { labels = on; changed("Distance labels:" + (on ? "on" : "off")); });

        addSeek(box, "Daire boyutu", 10, 180, fov, val -> { fov = val; changed("fov:" + val); });
        addSeek(box, "Kalinlik", 1, 6, thickness, val -> { thickness = val; changed("thickness:" + val); });

        Button color = new Button(this); color.setAllCaps(false);
        color.setText("Renk: " + COLOR_NAMES[colorIndex]);
        color.setOnClickListener(v -> {
            colorIndex = (colorIndex + 1) % COLOR_NAMES.length;
            color.setText("Renk: " + COLOR_NAMES[colorIndex]);
            changed("color:" + colorIndex);
        });
        box.addView(color);

        Button close = new Button(this); close.setText("Kapat"); close.setOnClickListener(v -> hideMenu()); box.addView(close);

        // Yatay (landscape) ekranda tasmasin: yukseklik ekranin %85'ini gecmesin, gerekirse kaydirilir.
        DisplayMetrics dm = getResources().getDisplayMetrics();
        int mw = dp(280);
        box.measure(View.MeasureSpec.makeMeasureSpec(mw, View.MeasureSpec.EXACTLY),
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));
        int mh = Math.min(box.getMeasuredHeight(), (int) (dm.heightPixels * 0.85f));

        ScrollView sv = new ScrollView(this);
        sv.setBackgroundColor(Color.rgb(25, 27, 34));
        sv.addView(box);

        menu = sv;
        WindowManager.LayoutParams p = params(mw, mh);
        p.gravity = Gravity.TOP | Gravity.START;
        int x = handleParams.x + dp(56);
        if (x + mw > dm.widthPixels) x = Math.max(0, handleParams.x - mw - dp(8));
        p.x = x;
        p.y = Math.max(0, Math.min(handleParams.y, dm.heightPixels - mh));
        try {
            wm.addView(menu, p);
        } catch (Exception e) {
            menu = null;
        }
    }

    private interface Changed { void change(boolean value); }
    private interface IntChanged { void change(int value); }

    private void addToggle(LinearLayout box, String label, boolean initial, Changed cb) {
        Button b = new Button(this); b.setAllCaps(false); b.setText(label + (initial ? "  [ACIK]" : "  [KAPALI]")); b.setTag(initial);
        b.setOnClickListener(v -> {
            boolean next = !((Boolean) b.getTag()); b.setTag(next);
            b.setText(label + (next ? "  [ACIK]" : "  [KAPALI]")); cb.change(next);
        });
        box.addView(b);
    }

    private void addSeek(LinearLayout box, String label, int min, int max, int initial, IntChanged cb) {
        TextView t = new TextView(this); t.setTextColor(Color.WHITE); t.setText(label + ": " + initial); box.addView(t);
        SeekBar s = new SeekBar(this); s.setMax(max - min); s.setProgress(initial - min);
        s.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
            @Override public void onProgressChanged(SeekBar sb, int progress, boolean fromUser) {
                if (!fromUser) return;
                int v = min + progress; t.setText(label + ": " + v); cb.change(v);
            }
            @Override public void onStartTrackingTouch(SeekBar sb) { }
            @Override public void onStopTrackingTouch(SeekBar sb) { }
        });
        box.addView(s);
    }

    private void send(String command) {
        Intent i = new Intent(ACTION_COMMAND); i.setPackage(getPackageName()); i.putExtra("cmd", command); sendBroadcast(i);
    }

    private void hideMenu() {
        if (menu != null) { try { wm.removeView(menu); } catch (IllegalArgumentException ignored) { } menu = null; }
    }

    private void removeAll() {
        hideMenu();
        if (handle != null) { try { wm.removeView(handle); } catch (IllegalArgumentException ignored) { } handle = null; }
        if (circleView != null) { try { wm.removeView(circleView); } catch (IllegalArgumentException ignored) { } circleView = null; }
        if (hLine != null) { try { wm.removeView(hLine); } catch (IllegalArgumentException ignored) { } hLine = null; }
        if (vLine != null) { try { wm.removeView(vLine); } catch (IllegalArgumentException ignored) { } vLine = null; }
    }

    @Override public int onStartCommand(Intent i, int flags, int id) { return START_NOT_STICKY; }
    @Override public IBinder onBind(Intent i) { return null; }

    @Override public void onDestroy() {
        removeAll();
        super.onDestroy();
    }
}
