using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTower : TowerBase
{
    [Header("Laser Settings")]
    [SerializeField] private int damagePerSecond = 35;
    [Range(0f, 0.9f)]
    [SerializeField] private float slowPercent = 0.25f;

    private LineRenderer lineRenderer;
    private float damageAccumulator = 0f;

    protected override void Start()
    {
        base.Start();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    // Suprascriem Update din TowerBase pentru ca laserul arde continuu, nu pe baza de fireCountdown
    protected override void Update()
    {
        if (target == null)
        {
            if (lineRenderer.enabled)
                lineRenderer.enabled = false;

            damageAccumulator = 0f;
            return;
        }

        RotateTowardsTarget();
        Attack();
    }

    // Implementarea polimorfica a atacului pentru laser
    protected override void Attack()
    {
        Vector3 startPos = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 2f;

        if (!lineRenderer.enabled)
            lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, target.position);

        // Acumulam damage-ul pe secunda si il aplicam ca numere intregi curate
        damageAccumulator += damagePerSecond * Time.deltaTime;
        if (damageAccumulator >= 1f)
        {
            int damageToApply = Mathf.FloorToInt(damageAccumulator);
            damageAccumulator -= damageToApply;

            target.SendMessage("TakeDamage", damageToApply, SendMessageOptions.DontRequireReceiver);
        }

        if (slowPercent > 0f)
        {
            target.SendMessage("ApplySlow", new Vector2(slowPercent, 0.2f), SendMessageOptions.DontRequireReceiver);
        }
    }
}