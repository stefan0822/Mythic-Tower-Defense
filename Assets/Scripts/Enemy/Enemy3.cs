using UnityEngine;

public class Enemy3 : EnemyBase
{
    [Header("Enemy 3 Armor")]
    public float flatArmorReduction = 5f;

    protected override void Awake()
    {
        enemyName = "Enemy 3";
        base.Awake();
    }

    protected override float CalculateIncomingDamage(float rawDamage)
    {
        return Mathf.Max(1f, rawDamage - flatArmorReduction);
    }
}