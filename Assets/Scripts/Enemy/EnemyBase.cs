using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("General Enemy Stats")]
    public string enemyName = "Enemy";
    public float maxHealth = 100f;
    public float moveSpeed = 18f;
    public float rotationSpeed = 10f;
    public int goldReward = 15;
    public int damageToBase = 1;

    [Header("Resistances")]
    [Range(0f, 1f)]
    public float slowResistance = 0f; // 0 = normal, 0.5 = 50% rezistenta, 1 = imun la slow

    protected float currentHealth;
    protected float slowMultiplier = 1f;
    protected float slowTimer = 0f;

    protected WaypointPath path;
    protected int currentWaypointIndex = 0;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        gameObject.tag = "Enemy";
    }

    public virtual void SetPath(WaypointPath assignedPath)
    {
        path = assignedPath;
        currentWaypointIndex = 0;

        if (path != null && path.waypoints != null && path.waypoints.Length > 0)
        {
            transform.position = path.waypoints[0].position;
        }

        OnEnemySpawned();
    }

    protected virtual void Update()
    {
        if (isDead || path == null || path.waypoints == null || path.waypoints.Length == 0) return;

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowMultiplier = 1f;
            }
        }

        OnEnemyUpdate();
        MoveAlongPath();
    }

    protected virtual void MoveAlongPath()
    {
        if (currentWaypointIndex >= path.waypoints.Length)
        {
            ReachEnd();
            return;
        }

        Transform targetPoint = path.waypoints[currentWaypointIndex];
        Vector3 targetPos = new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z);
        Vector3 dir = targetPos - transform.position;

        float step = GetCurrentMoveSpeed() * slowMultiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.25f)
        {
            currentWaypointIndex++;
        }
    }

    protected virtual float GetCurrentMoveSpeed()
    {
        return moveSpeed;
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        float finalDamage = CalculateIncomingDamage(amount);
        currentHealth -= finalDamage;
        OnDamaged(finalDamage);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual float CalculateIncomingDamage(float rawDamage)
    {
        return rawDamage;
    }

    public virtual void ApplySlow(float slowPercent, float duration = 2f)
    {
        float effectiveSlow = slowPercent * (1f - slowResistance);
        if (effectiveSlow <= 0f) return;

        slowMultiplier = Mathf.Clamp(1f - effectiveSlow, 0.1f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        OnEnemyDied();
        Destroy(gameObject);
    }

    protected virtual void ReachEnd()
    {
        if (isDead) return;
        isDead = true;
        OnReachedEnd();
        Destroy(gameObject);
    }

    // Hook-uri pe care le poti personaliza in fiecare subclasa de personaj
    protected virtual void OnEnemySpawned() { }
    protected virtual void OnEnemyUpdate() { }
    protected virtual void OnDamaged(float damageTaken) { }
    protected virtual void OnEnemyDied() { }
    protected virtual void OnReachedEnd() { }
}