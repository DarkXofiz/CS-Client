using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// CsClient V11.9: dark, tabbed visual/debug menu for a Unity project you control.
// Visualizes objects in the local test scene only; no external process memory access or auto-aim.
public class WetsukiMenu : MonoBehaviour
{
    public static bool ShowFovCircle = true;
    public static bool ShowEsp = true;
    public static bool ShowDistance = true;
    public static bool ShowBodyPoints = true;
    public static bool ShowCenterLines = false;
    public static float FovSize = 90f;
    public static float CircleThickness = 2f;

    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float refreshInterval = 0.35f;

    private bool menuVisible = true;
    private int colorIndex = 0;
    private int selectedTab = 0;
    private float nextRefresh;
    private bool playerTagMissing;
    private readonly List<Transform> targets = new List<Transform>();
    private Rect windowRect = new Rect(70, 80, 760, 520);

    private static readonly Color[] Palette =
        { new Color(1f, .28f, .31f), new Color(.22f, .88f, .48f), new Color(.25f, .82f, 1f),
          new Color(1f, .82f, .20f), new Color(.85f, .36f, 1f), Color.white };
    private static readonly string[] ColorNames =
        { "Kırmızı", "Yeşil", "Camgöbeği", "Sarı", "Mor", "Beyaz" };

    private GUIStyle titleStyle, sectionStyle, labelStyle, mutedStyle, buttonStyle, tabStyle, activeTabStyle;
    private Texture2D panelTexture, cardTexture, accentTexture, trackTexture, knobTexture;

    private void Awake()
    {
        panelTexture = MakeTexture(new Color(.045f, .052f, .068f, .98f));
        cardTexture = MakeTexture(new Color(.075f, .083f, .105f, 1f));
        accentTexture = MakeTexture(new Color(1f, .34f, .20f, 1f));
        trackTexture = MakeTexture(new Color(.20f, .22f, .27f, 1f));
        knobTexture = MakeTexture(new Color(.84f, .85f, .89f, 1f));
    }

    private Texture2D MakeTexture(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.hideFlags = HideFlags.HideAndDontSave;
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    private void OnDestroy()
    {
        Destroy(panelTexture); Destroy(cardTexture); Destroy(accentTexture);
        Destroy(trackTexture); Destroy(knobTexture);
    }

    private void OnEnable()
    {
        AndroidCommandBridge.CommandReceived += HandleCommand;
        RefreshTargets();
    }

    private void OnDisable()
    {
        AndroidCommandBridge.CommandReceived -= HandleCommand;
    }

    private void HandleCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return;
        if (cmd == "show") { menuVisible = true; return; }
        if (cmd == "hide") { menuVisible = false; return; }
        if (cmd == "toggle") { menuVisible = !menuVisible; return; }

        int split = cmd.LastIndexOf(':');
        if (split <= 0) return;
        string name = cmd.Substring(0, split).Trim();
        string value = cmd.Substring(split + 1).Trim();
        bool on = value.Equals("on", StringComparison.OrdinalIgnoreCase);
        float num;
        bool isNum = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out num);

        if (name == "FOV circle") ShowFovCircle = on;
        else if (name == "ESP" || name == "Target markers") ShowEsp = on;
        else if (name == "Distance labels") ShowDistance = on;
        else if (name == "Body points") ShowBodyPoints = on;
        else if (name == "Center lines") ShowCenterLines = on;
        else if (name == "color" && isNum) colorIndex = Mathf.Clamp((int)num, 0, Palette.Length - 1);
        else if (name == "fov" && isNum) FovSize = Mathf.Clamp(num, 10f, 180f);
        else if (name == "thickness" && isNum) CircleThickness = Mathf.Clamp(num, 1f, 6f);
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.F6)) menuVisible = !menuVisible;
#endif
        if (Time.unscaledTime >= nextRefresh)
        {
            nextRefresh = Time.unscaledTime + Mathf.Max(0.1f, refreshInterval);
            RefreshTargets();
        }
    }

    private void RefreshTargets()
    {
        targets.Clear();
        playerTagMissing = false;
        try
        {
            GameObject[] tagged = GameObject.FindGameObjectsWithTag(targetTag);
            foreach (GameObject go in tagged)
                if (go != null && !targets.Contains(go.transform)) targets.Add(go.transform);
        }
        catch (UnityException) { playerTagMissing = true; }

        foreach (TestTarget marker in TestTarget.All)
            if (marker != null && marker.isActiveAndEnabled && !targets.Contains(marker.transform))
                targets.Add(marker.transform);
    }

    private Camera GetSceneCamera()
    {
        Camera cam = Camera.main;
        if (cam != null) return cam;
        Camera[] cameras = Camera.allCameras;
        return cameras.Length > 0 ? cameras[0] : null;
    }

    private string GetStatus()
    {
        if (GetSceneCamera() == null) return "Kamera bulunamadı";
        if (!ShowEsp) return "ESP kapalı";
        if (targets.Count == 0)
            return playerTagMissing ? "Player etiketi veya TestTarget ekle" : "Test hedefi bulunamadı";
        return "Test hedefi: " + targets.Count;
    }

    private void InitStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        sectionStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(.86f, .87f, .90f) } };
        mutedStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(.62f, .65f, .71f) } };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 12, fixedHeight = 28 };
        tabStyle = new GUIStyle(GUI.skin.button) { fontSize = 12, fixedHeight = 34, normal = { textColor = new Color(.78f, .80f, .84f) } };
        activeTabStyle = new GUIStyle(tabStyle) { fontStyle = FontStyle.Bold, normal = { textColor = Color.white, background = accentTexture } };
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            if (ShowFovCircle) DrawFovCircle();
            if (ShowEsp) DrawEsp();
        }

        if (!menuVisible)
        {
            // Dokunmatik cihazda F6 yok: menuyu geri acmak icin kucuk bir tus.
            float bs = Mathf.Clamp(Screen.dpi > 0f ? Screen.dpi / 160f : 2f, 1f, 4f);
            Matrix4x4 prev = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(bs, bs, 1f));
            if (GUI.Button(new Rect(8f, 8f, 64f, 32f), "Menu")) menuVisible = true;
            GUI.matrix = prev;
            return;
        }
        InitStyles();

        float scale = Mathf.Clamp(Mathf.Min(Screen.width / 920f, Screen.height / 650f), .72f, 1f);
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        windowRect.width = 760;
        windowRect.height = 520;
        windowRect.x = Mathf.Clamp(windowRect.x, 8, Mathf.Max(8, Screen.width / scale - windowRect.width - 8));
        windowRect.y = Mathf.Clamp(windowRect.y, 8, Mathf.Max(8, Screen.height / scale - windowRect.height - 8));
        windowRect = GUI.Window(11800, windowRect, DrawWindow, GUIContent.none);
        GUI.matrix = old;
    }

    private void DrawWindow(int id)
    {
        GUI.DrawTexture(new Rect(0, 0, 760, 520), panelTexture);
        GUI.DrawTexture(new Rect(0, 0, 760, 6), accentTexture);

        // Header with game title at top-right, as requested.
        GUI.Label(new Rect(20, 16, 400, 30), "CsClient", titleStyle);
        GUI.Label(new Rect(515, 18, 225, 24), "COUNTER FLAME", sectionStyle);
        GUI.Label(new Rect(515, 42, 225, 18), "VISUAL DEBUG TOOLS", mutedStyle);
        GUI.DrawTexture(new Rect(18, 58, 724, 1), trackTexture);

        // Top tabs.
        string[] tabs = { "Visuals", "FOV", "Settings" };
        for (int i = 0; i < tabs.Length; i++)
        {
            if (GUI.Button(new Rect(20 + i * 145, 70, 137, 34), tabs[i], selectedTab == i ? activeTabStyle : tabStyle))
                selectedTab = i;
        }

        // Left rail.
        GUI.DrawTexture(new Rect(18, 116, 54, 384), cardTexture);
        GUI.Label(new Rect(32, 130, 28, 28), "V", sectionStyle);
        GUI.Label(new Rect(32, 182, 28, 28), "F", sectionStyle);
        GUI.Label(new Rect(32, 234, 28, 28), "S", sectionStyle);
        GUI.Label(new Rect(86, 119, 650, 24), selectedTab == 0 ? "Visuals  -  Targets" :
            selectedTab == 1 ? "FOV  -  Circle settings" : "Settings  -  Status", sectionStyle);

        if (selectedTab == 0) DrawVisualsPage();
        else if (selectedTab == 1) DrawFovPage();
        else DrawSettingsPage();

        GUI.Label(new Rect(88, 490, 650, 20), "F6: menu ac/kapat  |  " + GetStatus(), mutedStyle);
        GUI.DragWindow(new Rect(0, 0, 760, 60));
    }

    private void DrawVisualsPage()
    {
        DrawCard(new Rect(86, 150, 320, 155), "ESP");
        ShowEsp = DrawToggle(new Rect(100, 183, 285, 22), "Enabled", ShowEsp);
        ShowDistance = DrawToggle(new Rect(100, 208, 285, 22), "Distance labels", ShowDistance);
        ShowBodyPoints = DrawToggle(new Rect(100, 233, 285, 22), "Head / body / feet markers", ShowBodyPoints);
        ShowCenterLines = DrawToggle(new Rect(100, 258, 285, 22), "Center lines", ShowCenterLines);

        DrawCard(new Rect(420, 150, 320, 155), "Off-screen / target indicators");
        GUI.Label(new Rect(434, 185, 286, 42), "Only objects in your own Unity test scene\n(Player tag or TestTarget component).", labelStyle);
        GUI.Label(new Rect(434, 235, 286, 24), "Detected: " + targets.Count, sectionStyle);
        GUI.Label(new Rect(434, 262, 286, 20), "No external game process access", mutedStyle);

        DrawCard(new Rect(86, 320, 320, 150), "Box");
        GUI.Label(new Rect(100, 352, 280, 24), "Style: Regular", labelStyle);
        GUI.Label(new Rect(100, 380, 280, 22), "Line thickness", labelStyle);
        CircleThickness = GUI.HorizontalSlider(new Rect(100, 402, 280, 18), CircleThickness, 1, 6);
        GUI.Label(new Rect(100, 424, 280, 22), "Corner / outline size: " + CircleThickness.ToString("0.0"), mutedStyle);
        GUI.Label(new Rect(100, 446, 280, 18), "Box bounds use child Renderer bounds", mutedStyle);

        DrawCard(new Rect(420, 320, 320, 150), "FOV circle");
        ShowFovCircle = DrawToggle(new Rect(434, 353, 286, 22), "Enabled", ShowFovCircle);
        GUI.Label(new Rect(434, 382, 286, 22), "Size: " + FovSize.ToString("0"), labelStyle);
        FovSize = GUI.HorizontalSlider(new Rect(434, 404, 286, 18), FovSize, 10, 180);
        GUI.Label(new Rect(434, 428, 286, 22), "Color: " + ColorNames[colorIndex], labelStyle);
        if (GUI.Button(new Rect(434, 449, 135, 22), "Next color", buttonStyle))
            colorIndex = (colorIndex + 1) % Palette.Length;
        Color prevColor = GUI.color;
        GUI.color = Palette[colorIndex];
        GUI.DrawTexture(new Rect(685, 449, 28, 22), Texture2D.whiteTexture);
        GUI.color = prevColor;
    }

    private void DrawFovPage()
    {
        DrawCard(new Rect(86, 150, 654, 320), "FOV circle configuration");
        ShowFovCircle = DrawToggle(new Rect(108, 190, 590, 26), "Show FOV circle", ShowFovCircle);
        GUI.Label(new Rect(108, 235, 590, 22), "Circle size: " + FovSize.ToString("0"), labelStyle);
        FovSize = GUI.HorizontalSlider(new Rect(108, 260, 590, 20), FovSize, 10, 180);
        GUI.Label(new Rect(108, 300, 590, 22), "Thickness: " + CircleThickness.ToString("0.0"), labelStyle);
        CircleThickness = GUI.HorizontalSlider(new Rect(108, 325, 590, 20), CircleThickness, 1, 6);
        GUI.Label(new Rect(108, 366, 590, 22), "Colour", labelStyle);
        for (int i = 0; i < Palette.Length; i++)
        {
            if (GUI.Button(new Rect(108 + i * 84, 395, 74, 32), ColorNames[i], buttonStyle))
                colorIndex = i;
        }
    }

    private void DrawSettingsPage()
    {
        DrawCard(new Rect(86, 150, 654, 320), "Project status");
        GUI.Label(new Rect(108, 190, 600, 28), GetStatus(), sectionStyle);
        GUI.Label(new Rect(108, 228, 600, 44), "Camera: " + (GetSceneCamera() != null ? GetSceneCamera().name : "not found"), labelStyle);
        GUI.Label(new Rect(108, 272, 600, 44), "Targets are discovered by Player tag or TestTarget component.", labelStyle);
        GUI.Label(new Rect(108, 316, 600, 44), "Add this component to an active GameObject in your own Unity scene.", labelStyle);
        if (GUI.Button(new Rect(108, 372, 180, 34), "Refresh targets", buttonStyle)) RefreshTargets();
        if (GUI.Button(new Rect(302, 372, 180, 34), "Hide menu", buttonStyle)) menuVisible = false;
    }

    private void DrawCard(Rect r, string title)
    {
        GUI.DrawTexture(r, cardTexture);
        GUI.Label(new Rect(r.x + 14, r.y + 8, r.width - 28, 22), title, sectionStyle);
        GUI.DrawTexture(new Rect(r.x + 14, r.y + 34, r.width - 28, 1), trackTexture);
    }

    private bool DrawToggle(Rect r, string label, bool value)
    {
        GUI.Label(new Rect(r.x, r.y, r.width - 48, r.height), label, labelStyle);
        bool next = GUI.Toggle(new Rect(r.x + r.width - 42, r.y, 42, r.height), value, GUIContent.none);
        return next;
    }

    private void DrawFovCircle()
    {
        float radius = Mathf.Clamp(FovSize, 10f, 180f) * Mathf.Min(Screen.width, Screen.height) / 360f;
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Color old = GUI.color;
        GUI.color = Palette[colorIndex];
        const int segments = 96;
        Vector2 previous = center + new Vector2(radius, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector2 next = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            DrawLine(previous, next, CircleThickness);
            previous = next;
        }
        GUI.color = old;
    }

    private void DrawEsp()
    {
        Camera cam = GetSceneCamera();
        if (cam == null) return;
        Color old = GUI.color;
        GUI.color = Palette[colorIndex];

        foreach (Transform target in targets)
        {
            if (target == null || !target.gameObject.activeInHierarchy || target == cam.transform) continue;
            Bounds bounds;
            if (!TryGetBounds(target, out bounds)) bounds = new Bounds(target.position, Vector3.one * 0.5f);

            Vector3 top = cam.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
            Vector3 chest = cam.WorldToScreenPoint(bounds.center);
            Vector3 feet = cam.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
            if (chest.z <= 0f) continue;

            float x = chest.x, topY = Screen.height - top.y, bottomY = Screen.height - feet.y;
            float middleY = Screen.height - chest.y;
            float h = Mathf.Abs(bottomY - topY), w = Mathf.Clamp(h * 0.42f, 10f, 100f);
            if (top.z > 0f && feet.z > 0f && h > 2f)
                DrawBox(new Rect(x - w * .5f, Mathf.Min(topY, bottomY), w, h), 2f);
            else DrawBox(new Rect(x - 8f, middleY - 8f, 16f, 16f), 2f);

            if (ShowBodyPoints)
            {
                DrawPoint(top, "Head");
                DrawPoint(chest, "Body");
                DrawPoint(feet, "Feet");
            }
            if (ShowDistance)
            {
                float distance = Vector3.Distance(cam.transform.position, target.position);
                GUI.Label(new Rect(x + 12f, middleY - 12f, 200f, 22f), target.name + "  " + distance.ToString("0.0") + " m");
            }
            if (ShowCenterLines)
                DrawLine(new Vector2(Screen.width / 2f, Screen.height / 2f), new Vector2(x, middleY), 1f);
        }
        GUI.color = old;
    }

    private static bool TryGetBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        bool found = false;
        bounds = new Bounds(root.position, Vector3.one * .5f);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return found;
    }

    private static void DrawPoint(Vector3 p, string label)
    {
        if (p.z <= 0f) return;
        float y = Screen.height - p.y;
        GUI.Box(new Rect(p.x - 4f, y - 4f, 8f, 8f), "");
        GUI.Label(new Rect(p.x + 5f, y - 10f, 70f, 20f), label);
    }

    private static void DrawBox(Rect r, float t)
    {
        Texture2D tex = Texture2D.whiteTexture;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), tex);
        GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), tex);
        GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), tex);
        GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), tex);
    }

    private static void DrawLine(Vector2 a, Vector2 b, float width)
    {
        Matrix4x4 old = GUI.matrix;
        Vector2 delta = b - a;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(new Rect(a.x, a.y - width / 2f, delta.magnitude, width), Texture2D.whiteTexture);
        GUI.matrix = old;
    }
}
