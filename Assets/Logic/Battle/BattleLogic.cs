using UnityEngine;

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
}