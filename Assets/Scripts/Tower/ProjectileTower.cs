using UnityEngine;

public class ProjectileTower : TowerBase
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    // Implementam metoda abstracta din TowerBase (Polimorfism)
    protected override void Attack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{towerName}: Lipseste projectilePrefab!");
            return;
        }

        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 2f;

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        TowerProjectile projectile = projectileObj.GetComponent<TowerProjectile>();

        if (projectile != null)
        {
            // Folosim 'target' si 'damage' mostenite direct din TowerBase
            projectile.Initialize(target, damage);
        }
    }
}