using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MonsterStatus : MonsterData
{
  [Header("--- 現在の状態 ---")]
  public int level { get; private set; }
  public int currentHp { get; set; }
  public int currentMp { get; set; }

  public MonsterData masterData { get; private set; }

  public MonsterStatus(MonsterData baseData, int initialLevel = 1)
  {
    this.masterData = baseData;
    this.isBoss = baseData.isBoss;

    // 🌟 アンダースコア（_）以降を無視して、純粋なモンスター名だけを抽出する処理
    if (!string.IsNullOrEmpty(baseData.monsterName))
    {
      int index = baseData.monsterName.IndexOf('_');
      if (index >= 0)
      {
        // 「スライム_boss」なら「スライム」の部分だけを切り取る
        this.monsterName = baseData.monsterName.Substring(0, index);
      }
      else
      {
        this.monsterName = baseData.monsterName;
      }
    }

    SetLevel(initialLevel);

    this.currentHp = this.hp;
    this.currentMp = this.mp;
  }

  /// <summary>
  /// レベルを変更し、可変成長率リストに基づいてステータスを再計算します
  /// </summary>
  public void SetLevel(int newLevel)
  {
    this.level = newLevel;

    // 1レベルごとの成長率を毎レベル足し算して計算
    this.hp = CalculateStatusValue(masterData.hp, masterData.growsHp);
    this.mp = CalculateStatusValue(masterData.mp, masterData.growsMp);
    this.attack = CalculateStatusValue(masterData.attack, masterData.growsAttack);
    this.defense = CalculateStatusValue(masterData.defense, masterData.growsDefense);

    // 現在値が最大値を超えないようにクランプ
    if (this.currentHp > this.hp) this.currentHp = this.hp;
    if (this.currentMp > this.mp) this.currentMp = this.mp;
  }

  /// <summary>
  /// 現在のレベルに到達するまでの成長値を動的に累積計算するメソッド
  /// </summary>
  private int CalculateStatusValue(int baseValue, List<LevelGrowRate> growList)
  {
    if (growList == null || growList.Count == 0)
    {
      return baseValue;
    }

    float totalGrow = 0f;

    // レベル2から現在のレベルまで、1レベル上がるごとの伸び代を計算
    for (int currentLvl = 2; currentLvl <= level; currentLvl++)
    {
      float applicableRate = 0f;
      int highestMatchingLevel = -1;

      foreach (var growData in growList)
      {
        if (currentLvl >= growData.level && growData.level > highestMatchingLevel)
        {
          highestMatchingLevel = growData.level;
          applicableRate = growData.rate;
        }
      }

      totalGrow += applicableRate;
    }

    return Mathf.RoundToInt(baseValue + totalGrow);
  }
}