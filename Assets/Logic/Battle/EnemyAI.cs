using UnityEngine;
using System.Collections.Generic;

public static class EnemyAI
{
  public static MonsterStatus DecideTarget(List<MonsterStatus> players)
  {
    if (players == null || players.Count == 0) return null;

    int randomIndex = Random.Range(0, players.Count);
    return players[randomIndex];
  }
}