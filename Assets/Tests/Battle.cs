using UnityEngine;
using System.Collections.Generic;

public static class Battle
{
  /// <summary>
  /// 単体テスト用に、指定されたJSONファイルからMonsterStatusのプレイヤーリストを生成します
  /// </summary>
  public static List<MonsterStatus> CreateTestPlayers(TextAsset jsonFile)
  {
    List<MonsterStatus> players = new List<MonsterStatus>();
    var masterData = BattleInitializer.LoadMonsterDictionary(jsonFile);

    if (masterData.ContainsKey("ドラキー"))
    {
      // レベル5のドラキーAとBを生成する例
      MonsterData baseMonster = masterData["ドラキー"];

      MonsterData p1Master = baseMonster.MakeMonster();
      p1Master.monsterName = "ドラキーA";
      players.Add(new MonsterStatus(p1Master, 5));

      MonsterData p2Master = baseMonster.MakeMonster();
      p2Master.monsterName = "ドラキーB";
      players.Add(new MonsterStatus(p2Master, 5));
    }

    return players;
  }

  /// <summary>
  /// 単体テスト用に、指定されたJSONファイルからMonsterStatusの敵リストを生成します
  /// </summary>
  public static List<MonsterStatus> CreateTestEnemies(TextAsset jsonFile)
  {
    List<MonsterStatus> enemies = new List<MonsterStatus>();
    var masterData = BattleInitializer.LoadMonsterDictionary(jsonFile);

    if (masterData.ContainsKey("スライム"))
    {
      MonsterData baseMonster = masterData["スライム"];

      MonsterData e1Master = baseMonster.MakeMonster();
      e1Master.monsterName = "スライムA";
      enemies.Add(new MonsterStatus(e1Master, 1));

      MonsterData e2Master = baseMonster.MakeMonster();
      e2Master.monsterName = "スライムB";
      enemies.Add(new MonsterStatus(e2Master, 1));

      MonsterData base2Monster = masterData["ドラキー"];
      MonsterData e3Master = base2Monster.MakeMonster();
      enemies.Add(new MonsterStatus(e3Master, 1));
    }

    return enemies;
  }
}