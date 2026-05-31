using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct LevelGrowRate
{
  public int level;
  public float rate;
}

[System.Serializable]
public class MonsterData
{
  // 🌟 追加：モンスター固有の図鑑IDや管理用のID（例: 1, 2, 3...）
  public int id { get; set; } = 0;

  public int hp { get; set; }
  public int mp { get; set; }
  public int attack { get; set; }
  public int defense { get; set; }
  public string monsterName { get; set; }
  public string spriteName { get; set; } = "";

  public List<LevelGrowRate> growsHp { get; set; } = new List<LevelGrowRate>();
  public List<LevelGrowRate> growsMp { get; set; } = new List<LevelGrowRate>();
  public List<LevelGrowRate> growsAttack { get; set; } = new List<LevelGrowRate>();
  public List<LevelGrowRate> growsDefense { get; set; } = new List<LevelGrowRate>();

  public bool isBoss { get; set; } = false;

  public MonsterData MakeMonster()
  {
    return new MonsterData
    {
      id = this.id, // 🌟 複製処理にも追加
      hp = this.hp,
      mp = this.mp,
      attack = this.attack,
      defense = this.defense,
      monsterName = this.monsterName,
      spriteName = this.spriteName,
      growsHp = new List<LevelGrowRate>(this.growsHp),
      growsMp = new List<LevelGrowRate>(this.growsMp),
      growsAttack = new List<LevelGrowRate>(this.growsAttack),
      growsDefense = new List<LevelGrowRate>(this.growsDefense),
      isBoss = this.isBoss
    };
  }
}