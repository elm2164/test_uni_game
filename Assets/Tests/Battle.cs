using UnityEngine;
using System.Collections.Generic;

// ★ namespace MyGame.Test の囲みを削除しました！
public static class Battle
{
  public static List<MonsterData> CreateTestPlayers(Dictionary<string, MonsterData> masterData)
  {
    List<MonsterData> players = new List<MonsterData>();

    if (masterData.ContainsKey("ドラキー"))
    {
      players.Add(masterData["ドラキー"].MakeMonster());
      players.Add(masterData["ドラキー"].MakeMonster());
      players[0].monsterName = "ドラキーA";
      players[1].monsterName = "ドラキーB";
    }

    return players;
  }

  public static List<MonsterData> CreateTestEnemies(Dictionary<string, MonsterData> masterData)
  {
    List<MonsterData> enemies = new List<MonsterData>();

    if (masterData.ContainsKey("スライム"))
    {
      enemies.Add(masterData["スライム"].MakeMonster());
      enemies.Add(masterData["スライム"].MakeMonster());
      enemies.Add(masterData["スライム"].MakeMonster());
      enemies[0].monsterName = "スライムA";
      enemies[1].monsterName = "スライムB";
      enemies[2].monsterName = "スライムC";
    }

    return enemies;
  }
}