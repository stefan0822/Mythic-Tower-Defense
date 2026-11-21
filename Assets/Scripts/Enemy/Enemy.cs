using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 18f;
    public float rotationSpeed = 10f;
    public int goldReward = 15;
    public int damageToBase = 1;

    private float currentHealth;
    private float slowMultiplier = 1f;
    private float slowTimer = 0f;

    private WaypointPath path;
    private int currentWaypointIndex = 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        gameObject.tag = "Enemy";
    }

    public void SetPath(WaypointPath assignedPath)
    {
        path = assignedPath;
        currentWaypointIndex = 0;

        if (path != null && path.waypoints != null && path.waypoints.Length > 0)
        {
            transform.position = path.waypoints[0].position;
        }
    }

    private void Update()
    {
        if (path == null || path.waypoints == null || path.waypoints.Length == 0) return;

        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                slowMultiplier = 1f;
            }
        }

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (currentWaypointIndex >= path.waypoints.Length)
        {
            ReachEnd();
            return;
        }

        Transform targetPoint = path.waypoints[currentWaypointIndex];
        Vector3 targetPos = new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z);
        Vector3 dir = targetPos - transform.position;

        float step = moveSpeed * slowMultiplier * Time.deltaTime;
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

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void ApplySlow(float slowPercent, float duration = 2f)
    {
        slowMultiplier = Mathf.Clamp(1f - slowPercent, 0.1f, 1f);
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void ReachEnd()
    {
        Destroy(gameObject);
    }
}