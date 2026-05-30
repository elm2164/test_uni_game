using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class BattleInitializer
{
  /// <summary>
  /// JSONファイルを読み込んでモンスターのマスターデータ辞書を返します
  /// </summary>
  public static Dictionary<string, MonsterData> LoadMonsterDictionary(TextAsset jsonFile)
  {
    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, MonsterData>>(jsonFile.text);
    foreach (var pair in dictionary)
    {
      pair.Value.monsterName = pair.Key;
    }
    return dictionary;
  }

  /// <summary>
  /// プレイヤーリストの安全性をチェックして返します
  /// </summary>
  public static List<MonsterData> SetupPlayers(List<MonsterData> inputPlayers)
  {
    if (inputPlayers != null && inputPlayers.Count > 0)
    {
      return inputPlayers;
    }
    return new List<MonsterData>();
  }

  /// <summary>
  /// 敵リストの安全性をチェックして返します
  /// </summary>
  public static List<MonsterData> SetupEnemies(List<MonsterData> inputEnemies)
  {
    if (inputEnemies != null && inputEnemies.Count > 0)
    {
      return inputEnemies;
    }
    return new List<MonsterData>();
  }
}