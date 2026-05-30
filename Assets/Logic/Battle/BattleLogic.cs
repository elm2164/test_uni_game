using UnityEngine;
using System.Collections.Generic;

public static class BattleLogic
{
    /// <summary>
    /// ドラクエ風の基本ダメージを計算します（最低1ダメージ保証）
    /// </summary>
    public static int CalculatePhysicalDamage(MonsterData attacker, MonsterData defender)
    {
        int baseDamage = (attacker.attack / 2) - (defender.defense / 4);
        return Mathf.Max(1, baseDamage);
    }

    /// <summary>
    /// プレイヤーの物理攻撃を実行し、結果のログ文字列を返します
    /// </summary>
    public static string ExecutePlayerAttack(PlayerAction action, out int damage)
    {
        damage = CalculatePhysicalDamage(action.user, action.targetEnemy);
        action.targetEnemy.hp = Mathf.Max(0, action.targetEnemy.hp - damage);

        return $"{action.user.monsterName} の攻撃！\n{action.targetEnemy.monsterName}に {damage} のダメージ！";
    }

    /// <summary>
    /// 敵の物理攻撃を実行し、結果のログ文字列を返します
    /// </summary>
    public static string ExecuteEnemyAttack(MonsterData enemy, MonsterData target, bool isTargetDefending, out int damage)
    {
        damage = CalculatePhysicalDamage(enemy, target);

        if (isTargetDefending)
        {
            damage = Mathf.Max(1, damage / 2);
        }

        target.hp = Mathf.Max(0, target.hp - damage);
        return $"敵の {enemy.monsterName} の攻撃！\n{target.monsterName}は {damage} のダメージ！";
    }
}