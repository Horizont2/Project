using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "ScriptableObjects/EnemyStats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
}