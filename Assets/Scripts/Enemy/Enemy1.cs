using UnityEngine;

public class Enemy1 : EnemyBase
{
    protected override void Awake()
    {
        enemyName = "Enemy 1";
        base.Awake();
    }
}