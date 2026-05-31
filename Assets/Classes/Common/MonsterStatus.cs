using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class MonsterStatus : MonsterData
{
  [Header("--- 現在の状態（変動値） ---")]
  public int level { get; private set; }
  public int currentHp { get; set; }
  public int currentMp { get; set; }

  /// <summary>
  /// コンストラクタ：ベースデータの値を自動コピーし、名前の補正および画像名の4桁自動補完を行います
  /// </summary>
  public MonsterStatus(MonsterData baseData, int initialLevel = 1)
  {
    if (baseData == null) return;

    // 1. baseData の全フィールド（id, hp, monsterName, spriteName 等）を自身へ全自動コピー
    string json = JsonConvert.SerializeObject(baseData);
    JsonConvert.PopulateObject(json, this);

    // 2. monsterName のアンダースコア（_）以降を削って上書き
    if (!string.IsNullOrEmpty(this.monsterName))
    {
      int index = this.monsterName.IndexOf('_');
      if (index >= 0)
      {
        this.monsterName = this.monsterName.Substring(0, index);
      }
    }

    // 3. 🌟 spriteName が空（あるいは未指定）の場合、IDを4桁にパディング（例: 1 -> "0001"）して自動補完
    if (string.IsNullOrEmpty(this.spriteName))
    {
      this.spriteName = this.id.ToString("D4");
    }

    SetLevel(initialLevel);

    // 現在値の初期化
    this.currentHp = this.hp;
    this.currentMp = this.mp;
  }

  /// <summary>
  /// レベルを変更し、自身の持つ成長率リストに基づいてステータスを再計算します
  /// </summary>
  public void SetLevel(int newLevel)
  {
    this.level = newLevel;

    this.hp = CalculateStatusValue(this.hp, this.growsHp);
    this.mp = CalculateStatusValue(this.mp, this.growsMp);
    this.attack = CalculateStatusValue(this.attack, this.growsAttack);
    this.defense = CalculateStatusValue(this.defense, this.growsDefense);

    if (this.currentHp > this.hp) this.currentHp = this.hp;
    if (this.currentMp > this.mp) this.currentMp = this.mp;
  }

  private int CalculateStatusValue(int baseValue, List<LevelGrowRate> growList)
  {
    if (growList == null || growList.Count == 0)
    {
      return baseValue;
    }

    float totalGrow = 0f;

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