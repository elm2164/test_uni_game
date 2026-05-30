using UnityEngine;

[System.Serializable]
public class BattleMonsterData : MonsterData
{
  // バトル中にリアルタイムで変動する現在値
  public int currentHp { get; set; }
  public int currentMp { get; set; }

  // コンストラクタ：Battle.CreateTestPlayers等で返ってくるマスターデータを元に初期化
  public BattleMonsterData(MonsterData baseData)
  {
    this.monsterName = baseData.monsterName;
    this.hp = baseData.hp;     // 元の値を最大値として保持
    this.mp = baseData.mp;     // 元の値を最大値として保持
    this.attack = baseData.attack;
    this.defense = baseData.defense;

    // 初期化時はHP/MPを満タンにする
    this.currentHp = baseData.hp;
    this.currentMp = baseData.mp;
  }
}