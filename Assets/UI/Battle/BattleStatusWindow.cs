using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BattleStatusWindow : MonoBehaviour
{
  [Header("--- プレハブの設定 ---")]
  [SerializeField] private GameObject statusPanelPrefab; // プレイヤー1人分の背景パネルPrefab
  [SerializeField] private GameObject statusTextPrefab;  // ステータス表示用のテキストPrefab

  [Header("--- 配置の設定 ---")]
  [SerializeField] private float stepX = 220f;            // パネルを横並びにする際の間隔
  [SerializeField] private float marginLeftName = 20f;    // 名前の左余白
  [SerializeField] private float marginLeftStatus = 35f;  // HP/MPの左余白（名前より右にズラすインデント用）
  [SerializeField] private float marginTop = 15f;         // パネル内での最初の行（名前）の上余白
  [SerializeField] private float textStepY = 30f;         // 行ごとの縦間隔

  // 生成したパネルのゲームオブジェクトを管理
  private List<GameObject> generatedPanels = new List<GameObject>();

  // リアルタイム更新用に、各プレイヤーの [HPテキスト, MPテキスト] を保持する配列のリスト
  private List<TextMeshProUGUI[]> playerStatusTexts = new List<TextMeshProUGUI[]>();

  /// <summary>
  /// 🌟 Battle.CreateTestPlayers(monsterDictionary) で生成された MonsterData のリストを直接受け取ります
  /// </summary>
  /// <param name="players">MonsterData型のプレイヤーリスト</param>
  public void CreateStatusWindow(List<BattleMonsterData> players, float startPosX, float startPosY)
  {
    ClearStatus();

    if (statusPanelPrefab == null || statusTextPrefab == null)
    {
      Debug.LogError("Status Prefabs が設定されていません。");
      return;
    }

    for (int i = 0; i < players.Count; i++)
    {
      BattleMonsterData player = players[i];
      if (player == null) continue;

      // 1. プレイヤー1人分の背景パネルを生成
      GameObject panelGo = Instantiate(statusPanelPrefab, this.transform);
      RectTransform panelRect = panelGo.GetComponent<RectTransform>();

      if (panelRect != null)
      {
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = Vector2.zero;

        // プレイヤーの人数分、横にスライドして配置
        float currentX = startPosX + (i * stepX);
        panelRect.anchoredPosition = new Vector2(currentX, startPosY);
      }

      generatedPanels.Add(panelGo);

      // 2. パネル内に「名前」「HP」「MP」をレイアウト通りに生成

      // 2-a. 名前（一番上の行、少し左寄り）
      GameObject nameGo = Instantiate(statusTextPrefab, panelGo.transform);
      RectTransform nameRect = nameGo.GetComponent<RectTransform>();
      TextMeshProUGUI nameTmp = nameGo.GetComponent<TextMeshProUGUI>();
      if (nameRect != null)
      {
        SetupTextAnchor(nameRect);
        nameRect.anchoredPosition = new Vector2(marginLeftName, -marginTop);
      }
      // 🌟 BattleMonsterDataクラスのプロパティ（monsterName）から名前を設定
      if (nameTmp != null) nameTmp.text = player.monsterName;

      // 2-b. HP（2行目、名前より右に引っ込める）
      GameObject hpGo = Instantiate(statusTextPrefab, panelGo.transform);
      RectTransform hpRect = hpGo.GetComponent<RectTransform>();
      TextMeshProUGUI hpTmp = hpGo.GetComponent<TextMeshProUGUI>();
      if (hpRect != null)
      {
        SetupTextAnchor(hpRect);
        hpRect.anchoredPosition = new Vector2(marginLeftStatus, -marginTop - textStepY);
      }

      // 2-c. MP（3行目、名前より右に引っ込める）
      GameObject mpGo = Instantiate(statusTextPrefab, panelGo.transform);
      RectTransform mpRect = mpGo.GetComponent<RectTransform>();
      TextMeshProUGUI mpTmp = mpGo.GetComponent<TextMeshProUGUI>();
      if (mpRect != null)
      {
        SetupTextAnchor(mpRect);
        mpRect.anchoredPosition = new Vector2(marginLeftStatus, -marginTop - (textStepY * 2));
      }

      // 更新用にHPとMPのテキストコンポーネントを記憶
      TextMeshProUGUI[] statusTexts = new TextMeshProUGUI[2] { hpTmp, mpTmp };
      playerStatusTexts.Add(statusTexts);

      // 🌟 BattleMonsterDataのプロパティ（currentHp, hp, currentMp, mp）から初期数値を反映
      UpdatePlayerUI(i, player.currentHp, player.hp, player.currentMp, player.mp);
    }
  }

  /// <summary>
  /// テキストオブジェクトのアンカーを一括で左上に設定する補助メソッド
  /// </summary>
  private void SetupTextAnchor(RectTransform rect)
  {
    rect.anchorMin = new Vector2(0f, 1f);
    rect.anchorMax = new Vector2(1f, 1f); // 横幅ストレッチ
    rect.pivot = new Vector2(0f, 1f);
  }

  /// <summary>
  /// プレイヤーのインデックスを指定して、HP/MPのテキスト表示をリアルタイム更新します
  /// </summary>
  public void UpdatePlayerUI(int playerIndex, int currentHp, int maxHp, int currentMp, int maxMp)
  {
    if (playerIndex < 0 || playerIndex >= playerStatusTexts.Count) return;

    TextMeshProUGUI[] texts = playerStatusTexts[playerIndex];

    if (texts[0] != null) texts[0].text = $"HP: {currentHp} / {maxHp}";
    if (texts[1] != null) texts[1].text = $"MP: {currentMp} / {maxMp}";
  }

  /// <summary>
  /// 生成されているステータスUIをすべて破棄します
  /// </summary>
  public void ClearStatus()
  {
    foreach (var panel in generatedPanels)
    {
      if (panel != null) Destroy(panel);
    }
    generatedPanels.Clear();
    playerStatusTexts.Clear();
  }
}