using UnityEngine;

// Kendi Unity oyunun icin hedef kilidi (lock-on): kamerayi/oyuncuyu en yakin gorunen hedefe cevirir.
// Kullanim: kameraya (ya da donen nesneye) ekle. Hedefler: "Player" etiketli nesneler
// ve TestTarget bilesenli nesneler. Ekranin sag altindaki "Kilit" tusu ile ac/kapa.
// Kendi bakis (mouse-look) scriptin varsa, TargetLock.IsLocked true iken onu durdur.
public class TargetLock : MonoBehaviour
{
    public static bool lockEnabled;
    public static bool IsLocked { get; private set; }

    public Transform aimOrigin;            // bos birakilirsa Camera.main
    public string targetTag = "Player";
    public float maxDistance = 60f;
    public float maxAngle = 35f;           // kameranin baktigi yone en fazla bu aciya kadar olan hedefler
    public float turnSpeed = 8f;           // buyudukce daha hizli doner
    public float aimHeight = 1f;           // hedefin ayaginin ustunde bakilacak yukseklik
    public bool requireLineOfSight = true; // duvar arkasindaki hedefe kilitlenme
    public LayerMask obstacleMask = ~0;
    public bool showButton = true;

    private Transform current;

    public static void Toggle() { lockEnabled = !lockEnabled; }

    private Transform Origin()
    {
        if (aimOrigin != null) return aimOrigin;
        Camera cam = Camera.main;
        if (cam == null && Camera.allCamerasCount > 0) cam = Camera.allCameras[0];
        return cam != null ? cam.transform : transform;
    }

    private void Update()
    {
        if (!lockEnabled) { current = null; IsLocked = false; return; }

        Transform origin = Origin();
        if (current == null || !IsValid(origin, current, 1.2f)) current = FindBest(origin);

        IsLocked = current != null;
        if (current == null) return;

        Vector3 dir = AimPoint(current) - origin.position;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion want = Quaternion.LookRotation(dir.normalized);
        float t = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);
        origin.rotation = Quaternion.Slerp(origin.rotation, want, t);
    }

    private Vector3 AimPoint(Transform t) { return t.position + Vector3.up * aimHeight; }

    private bool IsValid(Transform origin, Transform t, float rangeScale)
    {
        if (t == null || !t.gameObject.activeInHierarchy) return false;
        Vector3 to = AimPoint(t) - origin.position;
        if (to.magnitude > maxDistance * rangeScale) return false;
        if (requireLineOfSight)
        {
            RaycastHit hit;
            if (Physics.Linecast(origin.position, AimPoint(t), out hit, obstacleMask, QueryTriggerInteraction.Ignore)
                && hit.transform != t && !hit.transform.IsChildOf(t) && !t.IsChildOf(hit.transform))
                return false;
        }
        return true;
    }

    private Transform FindBest(Transform origin)
    {
        Transform best = null;
        float bestAngle = maxAngle;

        GameObject[] tagged;
        try { tagged = GameObject.FindGameObjectsWithTag(targetTag); }
        catch (UnityException) { tagged = new GameObject[0]; }

        foreach (GameObject g in tagged) Consider(origin, g.transform, ref best, ref bestAngle);
        foreach (TestTarget t in TestTarget.All) Consider(origin, t.transform, ref best, ref bestAngle);
        return best;
    }

    private void Consider(Transform origin, Transform t, ref Transform best, ref float bestAngle)
    {
        if (t == null || t.root == origin.root) return;
        if (!IsValid(origin, t, 1f)) return;
        float angle = Vector3.Angle(origin.forward, AimPoint(t) - origin.position);
        if (angle < bestAngle) { bestAngle = angle; best = t; }
    }

    private void OnGUI()
    {
        if (!showButton) return;
        float s = Mathf.Clamp(Screen.dpi > 0f ? Screen.dpi / 160f : 2f, 1f, 4f);
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
        float w = 120f, h = 40f;
        Rect r = new Rect(Screen.width / s - w - 12f, Screen.height / s - h - 12f, w, h);
        if (GUI.Button(r, lockEnabled ? "Kilit: ACIK" : "Kilit: KAPALI")) Toggle();
        GUI.matrix = old;
    }
}
