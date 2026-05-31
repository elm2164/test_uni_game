using UnityEngine;
using System.Collections.Generic;

public static class BattleLogic
{
    public static int CalculatePhysicalDamage(MonsterStatus attacker, MonsterStatus defender)
    {
        // 攻撃力と防御力をベースに基本計算
        int baseDamage = (attacker.attack / 2) - (defender.defense / 4);
        return Mathf.Max(1, baseDamage);
    }

    public static string ExecutePlayerAttack(PlayerAction action, out int damage)
    {
        damage = CalculatePhysicalDamage(action.user, action.targetEnemy);

        // 🌟 currentHp を削るように修正
        action.targetEnemy.currentHp = Mathf.Max(0, action.targetEnemy.currentHp - damage);

        return $"{action.user.monsterName} の攻撃！\n{action.targetEnemy.monsterName}に {damage} のダメージ！";
    }

    public static string ExecuteEnemyAttack(MonsterStatus enemy, MonsterStatus target, bool isTargetDefending, out int damage)
    {
        damage = CalculatePhysicalDamage(enemy, target);

        if (isTargetDefending)
        {
            damage = Mathf.Max(1, damage / 2);
        }

        // 🌟 currentHp を削るように修正
        target.currentHp = Mathf.Max(0, target.currentHp - damage);
        return $"敵の {enemy.monsterName} の攻撃！\n{target.monsterName}は {damage} のダメージ！";
    }
}