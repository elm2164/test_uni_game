using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LevelGrowRate
{
  public int level;       // このレベル「以降」に適用する成長率
  public float rate;      // 成長率の数値
}

[System.Serializable]
public class MonsterData
{
  public int hp { get; set; }
  public int mp { get; set; }
  public int attack { get; set; }
  public int defense { get; set; }
  public string monsterName { get; set; }

  // 成長率リスト（初期値は空配列）
  public List<LevelGrowRate> growsHp { get; set; } = new List<LevelGrowRate>();
  public List<LevelGrowRate> growsMp { get; set; } = new List<LevelGrowRate>();
  public List<LevelGrowRate> growsAttack { get; set; } = new List<LevelGrowRate>();
  public List<LevelGrowRate> growsDefense { get; set; } = new List<LevelGrowRate>();

  public bool isBoss { get; set; } = false;

  public MonsterData MakeMonster()
  {
    return new MonsterData
    {
      hp = this.hp,
      mp = this.mp,
      attack = this.attack,
      defense = this.defense,
      monsterName = this.monsterName,
      growsHp = new List<LevelGrowRate>(this.growsHp),
      growsMp = new List<LevelGrowRate>(this.growsMp),
      growsAttack = new List<LevelGrowRate>(this.growsAttack),
      growsDefense = new List<LevelGrowRate>(this.growsDefense),
      isBoss = this.isBoss
    };
  }
}