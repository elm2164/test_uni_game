using UnityEngine;

public enum CommandType
{
  Attack,
  Defend
}

public class PlayerAction
{
  public MonsterData user { get; set; }
  public CommandType command { get; set; }
  public MonsterData targetEnemy { get; set; }
}