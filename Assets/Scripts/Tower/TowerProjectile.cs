using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 35f;

    [Header("Area Damage (0 = Single Target)")]
    [SerializeField] private float explosionRadius = 0f;

    [Header("Slow Effect (Optional)")]
    [Range(0f, 0.9f)]
    [SerializeField] private float slowPercent = 0f;
    [SerializeField] private float slowDuration = 2f;

    private Transform target;
    private int damage;

    public void Initialize(Transform targetTransform, int projectileDamage)
    {
        target = targetTransform;
        damage = projectileDamage;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    private void HitTarget()
    {
        if (explosionRadius > 0f)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    ApplyEffects(col.transform);
                }
            }
        }
        else
        {
            ApplyEffects(target);
        }

        Destroy(gameObject);
    }

    private void ApplyEffects(Transform enemy)
    {
        // Pregatit pentru scriptul de Enemy pe care il facem la valuri
        enemy.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        if (slowPercent > 0f)
        {
            enemy.SendMessage("ApplySlow", new Vector2(slowPercent, slowDuration), SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}