using UnityEngine;

[System.Serializable]
public class MonsterData
{
  public int hp { get; set; }
  public int mp { get; set; }
  public int attack { get; set; }
  public int defense { get; set; }
  public string monsterName { get; set; }

  public MonsterData MakeMonster()
  {
    return new MonsterData
    {
      hp = this.hp,
      mp = this.mp,
      attack = this.attack,
      defense = this.defense,
      monsterName = this.monsterName
    };
  }
}