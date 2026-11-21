using UnityEngine;

public abstract class TowerBase : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] protected string towerName = "Base Tower";
    [SerializeField] protected int level = 1;

    [Header("Combat Stats")]
    [SerializeField] protected float range = 15f;
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected int damage = 20;

    [Header("Visuals & Targeting")]
    [SerializeField] protected Transform partToRotate;
    [SerializeField] protected float turnSpeed = 10f;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected string enemyTag = "Enemy";

    protected Transform target;
    private float fireCountdown = 0f;

    public string TowerName => towerName;
    public int Level => level;
    public float Range => range;
    protected virtual void Start()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
    }

    protected virtual void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
            target = nearestEnemy.transform;
        else
            target = null;
    }

    protected virtual void Update()
    {
        if (fireCountdown > 0f)
            fireCountdown -= Time.deltaTime;

        if (target == null)
            return;

        RotateTowardsTarget();

        if (fireCountdown <= 0f)
        {
            Attack();
            fireCountdown = 1f / fireRate;
        }
    }

    protected virtual void RotateTowardsTarget()
    {
        if (partToRotate == null)
            return;

        Vector3 dir = target.position - partToRotate.position;
        dir.y = 0f;

        if (dir == Vector3.zero)
            return;

        Quaternion lookRotation = Quaternion.LookRotation(dir);
        partToRotate.rotation = Quaternion.Lerp(
            partToRotate.rotation,
            lookRotation,
            Time.deltaTime * turnSpeed
        );
    }

    // Fiecare tip de turn isi va defini aici propriul mod de atac (Polimorfism)
    protected abstract void Attack();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}