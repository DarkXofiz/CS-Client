using System.Collections.Generic;
using UnityEngine;

// Add this component to an object in your own Unity test scene to display it in CsClient's debug ESP.
// Aktif TestTarget'lar All listesinde tutulur (WetsukiMenu ve TargetLock bunu kullanir).
public sealed class TestTarget : MonoBehaviour
{
    private static readonly List<TestTarget> all = new List<TestTarget>();
    public static IReadOnlyList<TestTarget> All { get { return all; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { all.Clear(); }

    private void OnEnable() { if (!all.Contains(this)) all.Add(this); }
    private void OnDisable() { all.Remove(this); }
}
