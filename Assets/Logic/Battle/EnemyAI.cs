using UnityEngine;
using System.Collections.Generic;

public static class EnemyAI
{
  /// <summary>
  /// 生存しているプレイヤーの中からランダムにターゲットを一人選択します
  /// </summary>
  public static MonsterData DecideTarget(List<MonsterData> players)
  {
    if (players == null || players.Count == 0) return null;

    int randomIndex = Random.Range(0, players.Count);
    return players[randomIndex];
  }
}