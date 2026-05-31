using UnityEngine;

public enum CommandType
{
  Attack,
  Defend
}

public class PlayerAction
{
  public MonsterStatus user { get; set; }        // 🌟 MonsterStatus型に修正
  public CommandType command { get; set; }
  public MonsterStatus targetEnemy { get; set; } // 🌟 MonsterStatus型に修正
}