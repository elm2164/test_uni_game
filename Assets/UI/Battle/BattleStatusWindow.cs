using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BattleStatusWindow : MonoBehaviour
{
  [Header("--- プレハブの設定 ---")]
  [SerializeField] private GameObject statusPanelPrefab; // プレイヤー1人分の背景パネルPrefab
  [SerializeField] private GameObject statusTextPrefab;  // ステータス表示用のテキストPrefab

  [Header("--- 配置・マージンの設定 ---")]
  [SerializeField] private float panelSpaceX = 20f;       // 🌟 初期値を20に設定
  [SerializeField] private float marginLeftName = 40f;    // 🌟 初期値を40に設定
  [SerializeField] private float marginLeftStatus = 40f;  // 🌟 初期値を40に設定
  [SerializeField] private float marginTop = 40f;         // 🌟 初期値を40に設定

  [Header("--- 行間・マージンの設定 ---")]
  [SerializeField] private float nameToStatusMargin = 80f; // 🌟 初期値を80に設定
  [SerializeField] private float textStepY = 60f;         // 🌟 初期値を60に設定

  [Header("--- セーフティ（デフォルトサイズ） ---")]
  [SerializeField] private float defaultPanelWidth = 300f;  // 🌟 初期値を300に設定
  [SerializeField] private float defaultPanelHeight = 300f; // 🌟 初期値を300に設定

  // 生成したパネルのゲームオブジェクトを管理
  private List<GameObject> generatedPanels = new List<GameObject>();

  // リアルタイム更新用に、各プレイヤーの [HPテキスト, MPテキスト] を保持する配列のリスト
  private List<TextMeshProUGUI[]> playerStatusTexts = new List<TextMeshProUGUI[]>();

  /// <summary>
  /// 引数の型を MonsterStatus のリストに適合させ、UIを動的に横並び生成します
  /// </summary>
  public void CreateStatusWindow(List<MonsterStatus> players, float startPosX, float startPosY)
  {
    ClearStatus();

    if (statusPanelPrefab == null || statusTextPrefab == null)
    {
      Debug.LogError("Status Prefabs が設定されていません。");
      return;
    }

    // 次のパネルを配置するX座標のトラッキング用変数
    float currentX = startPosX;

    for (int i = 0; i < players.Count; i++)
    {
      MonsterStatus player = players[i];
      if (player == null) continue;

      // 1. プレイヤー1人分の背景パネルを生成
      GameObject panelGo = Instantiate(statusPanelPrefab, this.transform);
      RectTransform panelRect = panelGo.GetComponent<RectTransform>();

      float targetWidth = defaultPanelWidth;

      if (panelRect != null)
      {
        targetWidth = panelRect.sizeDelta.x;
        float targetHeight = panelRect.sizeDelta.y;

        if (targetWidth <= 0f) targetWidth = defaultPanelWidth;
        if (targetHeight <= 0f) targetHeight = defaultPanelHeight;

        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);

        panelRect.sizeDelta = new Vector2(targetWidth, targetHeight);

        // 現在蓄積されている currentX をそのまま配置座標として使用
        panelRect.anchoredPosition = new Vector2(currentX, -startPosY);
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

      if (nameTmp != null) nameTmp.text = player.monsterName;

      // 計算用の基準となるY座標（名前の位置から指定マージン分下げる）
      float hpY = -marginTop - nameToStatusMargin;

      // 2-b. HP（2行目、名前から専用のマージン分空けて配置）
      GameObject hpGo = Instantiate(statusTextPrefab, panelGo.transform);
      RectTransform hpRect = hpGo.GetComponent<RectTransform>();
      TextMeshProUGUI hpTmp = hpGo.GetComponent<TextMeshProUGUI>();
      if (hpRect != null)
      {
        SetupTextAnchor(hpRect);
        hpRect.anchoredPosition = new Vector2(marginLeftStatus, hpY);
      }

      // 2-c. MP（3行目、HPの位置から通常のtextStepY分下げる）
      GameObject mpGo = Instantiate(statusTextPrefab, panelGo.transform);
      RectTransform mpRect = mpGo.GetComponent<RectTransform>();
      TextMeshProUGUI mpTmp = mpGo.GetComponent<TextMeshProUGUI>();
      if (mpRect != null)
      {
        SetupTextAnchor(mpRect);
        mpRect.anchoredPosition = new Vector2(marginLeftStatus, hpY - textStepY);
      }

      // 更新用にHPとMPのテキストコンポーネントを記憶
      TextMeshProUGUI[] statusTexts = new TextMeshProUGUI[2] { hpTmp, mpTmp };
      playerStatusTexts.Add(statusTexts);

      // 初期数値を反映
      UpdatePlayerUI(i, player.currentHp, player.hp, player.currentMp, player.mp);

      // 次のプレイヤーのために、今配置したパネルの横幅 ＋ 指定された隙間分だけX座標を進める
      currentX += targetWidth + panelSpaceX;
    }
  }

  private void SetupTextAnchor(RectTransform rect)
  {
    rect.anchorMin = new Vector2(0f, 1f);
    rect.anchorMax = new Vector2(1f, 1f);
    rect.pivot = new Vector2(0f, 1f);
  }

  public void UpdatePlayerUI(int playerIndex, int currentHp, int maxHp, int currentMp, int maxMp)
  {
    if (playerIndex < 0 || playerIndex >= playerStatusTexts.Count) return;

    TextMeshProUGUI[] texts = playerStatusTexts[playerIndex];

    if (texts[0] != null) texts[0].text = $"HP: {currentHp} / {maxHp}";
    if (texts[1] != null) texts[1].text = $"MP: {currentMp} / {maxMp}";
  }

  public void ClearStatus()
  {
    foreach (var panel in generatedPanels)
    {
      if (panel != null) Destroy(panel);
    }
    generatedPanels.Clear();
    playerStatusTexts.Clear();
  }

  // 🌟 追加：指定されたプレイヤーのステータスパネルを点滅させる
  public void FlashPlayerPanel(int playerIndex)
  {
    // インデックスが範囲内かつ、該当パネルが実在するか安全チェック
    if (playerIndex < 0 || playerIndex >= generatedPanels.Count) return;
    GameObject panelGo = generatedPanels[playerIndex];
    if (panelGo == null) return;

    // パネルについているImageコンポーネントを取得
    UnityEngine.UI.Image img = panelGo.GetComponent<UnityEngine.UI.Image>();
    if (img != null)
    {
      // 既存のバトルマネージャーのコルーチンシステムを利用するか、
      // このコンポーネント自身（MonoBehaviour）のコルーチンとして実行します
      StartCoroutine(FlashPanelRoutine(img));
    }
  }

  // 🌟 追加：点滅アニメーションの実体コルーチン
  private System.Collections.IEnumerator FlashPanelRoutine(UnityEngine.UI.Image img)
  {
    Color originalColor = img.color;
    // 赤っぽくフラッシュさせたい場合は、new Color(1f, 0f, 0f, originalColor.a) などにすると緊張感が出ます
    Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 今回は消える点補完

    for (int i = 0; i < 3; i++)
    {
      img.color = flashColor;
      yield return new WaitForSeconds(0.1f);
      img.color = originalColor;
      yield return new WaitForSeconds(0.1f);
    }
  }
}