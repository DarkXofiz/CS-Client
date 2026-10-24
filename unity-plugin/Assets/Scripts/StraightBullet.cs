using UnityEngine;

// Kendi Unity oyunun icin: mermiyi her karede isinla ilerletir, fizik motoru kullanmaz,
// bu yuzden hicbir yuzeyden sekmez. Mermi prefabindan Rigidbody ve Collider'i kaldir.
// Silahtan cikarken mermiyi namlunun yonune bakacak sekilde olustur.
public class StraightBullet : MonoBehaviour
{
    public float speed = 80f;
    public float life = 3f;
    public int damage = 10;
    public LayerMask hitMask = ~0;

    private void Start() { Destroy(gameObject, life); }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, step, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Hedefte "TakeDamage(int)" fonksiyonu varsa cagrilir.
            hit.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
            return;
        }
        transform.position += transform.forward * step;
    }
}
