using UnityEngine;

public class AreaPulseTower : TowerBase
{
    [Header("Area Pulse Settings")]
    [Range(0f, 0.9f)]
    [SerializeField] private float slowPercent = 0.4f; // 40% incetinire
    [SerializeField] private float slowDuration = 2.5f;

    [Header("Optional Visual Effect")]
    [SerializeField] private GameObject pulseEffectPrefab;

    // Suprascriem rotatia pentru ca un turn cu unda circulara nu trebuie sa se invarta
    protected override void RotateTowardsTarget()
    {
        // Nu face nimic
    }

    // Implementam diferit metoda abstracta Attack() din TowerBase (Polimorfism)
    protected override void Attack()
    {
        if (pulseEffectPrefab != null)
        {
            GameObject effect = Instantiate(pulseEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(effect, 1.5f);
        }

        // Gasim toti inamicii aflati in raza turnului
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag(enemyTag))
            {
                col.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

                if (slowPercent > 0f)
                {
                    col.SendMessage("ApplySlow", new Vector2(slowPercent, slowDuration), SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}