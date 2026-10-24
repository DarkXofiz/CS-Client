using UnityEngine;

// Sahnedeki herhangi bir GameObject'e ekle: oyun basladiginda kameranin onune
// birkac test kupu olusturur (TestTarget bilesenli). Boylece isaretleri hemen test edersin.
public class TestTargetSpawner : MonoBehaviour
{
    [SerializeField] private int count = 5;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 25f;

    private void Start()
    {
        Camera cam = Camera.main;
        if (cam == null && Camera.allCamerasCount > 0) cam = Camera.allCameras[0];
        Transform origin = cam != null ? cam.transform : transform;

        Vector3 forward = Vector3.ProjectOnPlane(origin.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();

        for (int i = 0; i < count; i++)
        {
            float angle = UnityEngine.Random.Range(-60f, 60f);
            float dist = UnityEngine.Random.Range(minDistance, maxDistance);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TestHedef" + (i + 1);
            cube.transform.position = origin.position + dir * dist;
            cube.AddComponent<TestTarget>();
        }
    }
}
