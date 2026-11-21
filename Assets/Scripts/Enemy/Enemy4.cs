using UnityEngine;

public class Enemy4 : EnemyBase
{
    [Header("Enemy 4 Abilities")]
    public float healthRegenPerSecond = 8f;

    protected override void Awake()
    {
        enemyName = "Enemy 4";
        slowResistance = 0.75f;
        base.Awake();
    }

    protected override void OnEnemyUpdate()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegenPerSecond * Time.deltaTime);
        }
    }
}