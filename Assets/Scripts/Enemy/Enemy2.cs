using UnityEngine;

public class Enemy2 : EnemyBase
{
    [Header("Enemy 2 Ability")]
    public float lowHpSpeedBoost = 1.4f;

    protected override void Awake()
    {
        enemyName = "Enemy 2";
        base.Awake();
    }

    protected override float GetCurrentMoveSpeed()
    {
        if (currentHealth < maxHealth * 0.5f)
            return moveSpeed * lowHpSpeedBoost;

        return moveSpeed;
    }
}