using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 🌟 Imageコンポーネントを扱うために追加

public class BattleManager : MonoBehaviour
{
    [Header("--- 共通パネル・システム ---")]
    [SerializeField] private GameObject panelLogWindow;
    [SerializeField] private TextAsset jsonFile;

    [Header("--- 使い回す万能メニューウインドウ ---")]
    [SerializeField] private MenuWindow generalMenuWindow;

    [Header("--- ステータス画面の配置システム ---")]
    [SerializeField] private BattleStatusWindow statusWindow;

    [Header("--- 🌟 グラフィック関連の設定 ---")]
    [SerializeField] private GameObject enemyImagePrefab; // 先ほど作った敵のImageプレハブ
    [SerializeField] private Transform enemyGroupParent;   // 敵画像をまとめて配置する親（Canvas内の空オブジェクトなど。未指定ならCanvas直下）
    [SerializeField] private float enemyStepX = 250f;      // 敵が複数並ぶ時の横の間隔
    [SerializeField] private float enemyCenterY = 100f;     // 敵を表示する画面中央の高さ（Y座標）

    [Header("--- テキスト関連 ---")]
    [SerializeField] private TextMeshProUGUI logText;

    private List<MonsterStatus> players = new List<MonsterStatus>();
    private List<MonsterStatus> enemies = new List<MonsterStatus>();

    // 🌟 追加：生成した敵のGameObjectをモンスターの実体とペアで管理するための辞書
    private Dictionary<MonsterStatus, GameObject> enemyGameObjects = new Dictionary<MonsterStatus, GameObject>();

    enum BattlePhase
    {
        MainMenuSelect,
        BattleMenuSelect,
        TargetSelect,
        EventProcessing,
        Victory,
        Defeat
    }
    BattlePhase currentPhase = BattlePhase.MainMenuSelect;

    int currentPlayerIndex = 0;
    private List<PlayerAction> chosenActions = new List<PlayerAction>();
    private bool isInitializedExternally = false;

    public void InitializeBattle(List<MonsterStatus> inputPlayers, List<MonsterStatus> inputEnemies)
    {
        players = inputPlayers;
        enemies = inputEnemies;

        isInitializedExternally = true;

        if (statusWindow != null)
        {
            statusWindow.CreateStatusWindow(players, 50f, 50f);
        }

        // 🌟 追加：敵モンスターのグラフィックを動的に整列生成
        CreateEnemyGraphics();

        StartTurnSetup();
    }

    /// <summary>
    /// 敵の数に応じて、画面中央に等間隔にグラフィックを生成配置するメソッド
    /// </summary>
    private void CreateEnemyGraphics()
    {
        // 既存のオブジェクトがあればクリア
        foreach (var go in enemyGameObjects.Values) { if (go != null) Destroy(go); }
        enemyGameObjects.Clear();

        if (enemyImagePrefab == null) return;

        int count = enemies.Count;
        Transform parentTransform = enemyGroupParent != null ? enemyGroupParent : this.transform;

        float startX = -((count - 1) * enemyStepX) / 2f;

        for (int i = 0; i < count; i++)
        {
            MonsterStatus enemy = enemies[i];

            GameObject enemyGo = Instantiate(enemyImagePrefab, parentTransform);
            RectTransform rect = enemyGo.GetComponent<RectTransform>();
            Image img = enemyGo.GetComponent<Image>();

            if (rect != null)
            {
                // 🌟 アンカーを「下・中央（bottom - center）」に変更
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);

                // 🌟 ピボットも「下・中央（軸の足元）」に変更
                rect.pivot = new Vector2(0.5f, 0f);

                float posX = startX + (i * enemyStepX);

                // 🌟 下端からの立ち上がり高さとして配置（インスペクターの enemyCenterY で足元の高さを調整可能に）
                // ※値はインスペクター上で 300 や 400 など、砂地のラインに合わせて調整してください。
                rect.anchoredPosition = new Vector2(posX, enemyCenterY);
            }

            if (img != null)
            {
                Sprite loadedSprite = Resources.Load<Sprite>($"Images/Monster/{enemy.spriteName}");
                if (loadedSprite != null)
                {
                    img.sprite = loadedSprite;

                    // 🛑 先ほど追加した preserveAspect や AspectRatioFitter の記述はすべて削除（あるいはオフ）にします
                    img.preserveAspect = false;

                    // 🌟 1. 画像が本来持っている「等倍ピクセルサイズ」にオブジェクトの大きさを一発で強制変更する
                    img.SetNativeSize();

                    // 🌟 2. 812pxのままだと画面に対して大きすぎるので、
                    // RectTransformのローカルスケール（Scale）を使って、全体の比率を綺麗に保ったまま縮小調整する
                    RectTransform imgRect = enemyGo.GetComponent<RectTransform>();
                    if (imgRect != null)
                    {
                        // 例として「0.3f」にすると、本来のアス比を完全に保ったまま、元の大きさの30%のサイズ感になります
                        // 画面内のバランス（スライムとの対比）を見て、0.25f や 0.35f など自由に調整してください
                        float targetScale = 0.3f;
                        imgRect.localScale = new Vector3(targetScale, targetScale, 1f);
                    }
                }
                else
                {
                    Debug.LogWarning($"Sprite '{enemy.spriteName}' が Resources/Images/Monster/ に見つかりません。ID: {enemy.id}");
                }
            }

            enemyGameObjects.Add(enemy, enemyGo);
        }
    }


    void Start()
    {
        if (!isInitializedExternally)
        {
            var testPlayers = Battle.CreateTestPlayers(jsonFile);
            var testEnemies = Battle.CreateTestEnemies(jsonFile);
            Debug.Log($"テスト用プレイヤー数: {testPlayers.Count}, 敵数: {testEnemies.Count}");
            Debug.Log(JsonConvert.SerializeObject(testEnemies));
            InitializeBattle(testPlayers, testEnemies);
        }
    }

    void StartTurnSetup()
    {
        chosenActions.Clear();
        currentPlayerIndex = 0;
        ChangePhase(BattlePhase.MainMenuSelect);
    }

    void ChangePhase(BattlePhase nextPhase)
    {
        currentPhase = nextPhase;

        generalMenuWindow.Close();
        panelLogWindow.SetActive(false);

        switch (currentPhase)
        {
            case BattlePhase.MainMenuSelect:
                generalMenuWindow.CreateMenu(
                    new List<string> { "たたかう", "どうぐ", "スカウト", "にげる" },
                    0f, 0f,
                    OnMainMenuConfirmed
                );
                break;

            case BattlePhase.BattleMenuSelect:
                string pName = (currentPlayerIndex < players.Count) ? players[currentPlayerIndex].monsterName : "";

                generalMenuWindow.CreateMenu(
                    new List<string> { "こうげき", "スキル", "ぼうぎょ", "もどる" },
                    0f, 0f,
                    OnBattleMenuConfirmed,
                    OnBattleMenuCanceled,
                    pName
                );
                break;

            case BattlePhase.TargetSelect:
                string targetTitle = (currentPlayerIndex < players.Count) ? players[currentPlayerIndex].monsterName : "";

                List<string> targetNames = new List<string>();
                foreach (var enemy in enemies) targetNames.Add(enemy.monsterName);
                targetNames.Add("もどる");

                generalMenuWindow.CreateMenu(
                    targetNames,
                    0f, 0f,
                    OnTargetMenuConfirmed,
                    OnTargetMenuCanceled,
                    targetTitle
                );
                break;

            case BattlePhase.EventProcessing:
                panelLogWindow.SetActive(true);
                StartCoroutine(ExecuteTurnRoutine());
                break;
        }
    }

    void Update()
    {
        if (currentPhase == BattlePhase.EventProcessing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        generalMenuWindow.HandleInput(keyboard);
    }

    void OnMainMenuConfirmed(int selection)
    {
        if (selection == 0) ChangePhase(BattlePhase.BattleMenuSelect);
    }

    void OnBattleMenuConfirmed(int selection)
    {
        if (selection == 0) ChangePhase(BattlePhase.TargetSelect);
        else if (selection == 2) SaveAction(CommandType.Defend, null);
        else if (selection == 3) HandleGoBack();
    }

    void OnBattleMenuCanceled()
    {
        HandleGoBack();
    }

    void OnTargetMenuConfirmed(int selection)
    {
        if (selection == enemies.Count)
        {
            ChangePhase(BattlePhase.BattleMenuSelect);
        }
        else
        {
            SaveAction(CommandType.Attack, enemies[selection]);
        }
    }

    void OnTargetMenuCanceled()
    {
        ChangePhase(BattlePhase.BattleMenuSelect);
    }

    void HandleGoBack()
    {
        if (currentPlayerIndex == 0)
        {
            ChangePhase(BattlePhase.MainMenuSelect);
        }
        else
        {
            currentPlayerIndex--;
            chosenActions.RemoveAt(chosenActions.Count - 1);
            ChangePhase(BattlePhase.BattleMenuSelect);
        }
    }

    void SaveAction(CommandType command, MonsterStatus target)
    {
        PlayerAction action = new PlayerAction { user = players[currentPlayerIndex], command = command, targetEnemy = target };
        chosenActions.Add(action);

        currentPlayerIndex++;

        if (currentPlayerIndex < players.Count)
        {
            ChangePhase(BattlePhase.BattleMenuSelect);
        }
        else
        {
            ChangePhase(BattlePhase.EventProcessing);
        }
    }


    // --- 追加：敵を点滅させるコルーチン演出 ---
    private IEnumerator FlashEnemy(GameObject enemyGo)
    {
        if (enemyGo == null) yield break;

        Image img = enemyGo.GetComponent<Image>();
        if (img == null) yield break;

        Color originalColor = img.color;
        // 赤く点滅させたい場合は、flashColor = Color.red に変更してください
        Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // 透明に点滅

        // 3回チカチカと点滅させる（0.1秒ごとに入れ替え）
        for (int i = 0; i < 3; i++)
        {
            img.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            img.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ExecuteTurnRoutine()
    {
        // --- 1. 味方の行動処理 ---
        foreach (var action in chosenActions)
        {
            if (action.user.currentHp <= 0) continue;

            if (action.command == CommandType.Attack)
            {
                if (action.targetEnemy.currentHp <= 0)
                {
                    if (enemies.Count > 0) action.targetEnemy = enemies[0];
                    else break;
                }

                int damage = 0;
                logText.text = BattleLogic.ExecutePlayerAttack(action, out damage);

                // 🌟 追加：攻撃対象の敵オブジェクトを取得して、ダメージと同時に点滅させる！
                if (enemyGameObjects.TryGetValue(action.targetEnemy, out GameObject targetGo))
                {
                    StartCoroutine(FlashEnemy(targetGo));
                }

                yield return new WaitForSeconds(1.5f);

                if (action.targetEnemy.currentHp <= 0)
                {
                    logText.text = $"{action.targetEnemy.monsterName} をたおした！";

                    if (enemyGameObjects.ContainsKey(action.targetEnemy))
                    {
                        Destroy(enemyGameObjects[action.targetEnemy]);
                        enemyGameObjects.Remove(action.targetEnemy);
                    }

                    enemies.Remove(action.targetEnemy);
                    yield return new WaitForSeconds(1.2f);

                    if (enemies.Count == 0)
                    {
                        StartCoroutine(VictoryRoutine());
                        yield break;
                    }
                }
            }
            else if (action.command == CommandType.Defend)
            {
                logText.text = $"{action.user.monsterName} は 身をまもっている！";
                yield return new WaitForSeconds(1.5f);
            }
        }

        // --- 2. 敵の行動処理 ---
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var activeEnemy = enemies[i];
            if (activeEnemy.currentHp <= 0) continue;
            if (players.Count == 0) yield break;

            MonsterStatus targetPlayer = EnemyAI.DecideTarget(players);
            int targetPlayerIndex = players.IndexOf(targetPlayer);

            bool isTargetDefending = false;
            foreach (var act in chosenActions)
            {
                if (act.user == targetPlayer && act.command == CommandType.Defend) isTargetDefending = true;
            }

            if (isTargetDefending)
            {
                logText.text = $"【{targetPlayer.monsterName}のぼうぎょ成功！】\nダメージを軽減した！";
                yield return new WaitForSeconds(1.2f);
            }

            int enemyDamage = 0;
            logText.text = BattleLogic.ExecuteEnemyAttack(activeEnemy, targetPlayer, isTargetDefending, out enemyDamage);

            if (statusWindow != null && targetPlayerIndex >= 0)
            {
                // 🌟 追加：数値を更新する直前のタイミングで、ステータスパネルをチカチカと点滅させる！
                statusWindow.FlashPlayerPanel(targetPlayerIndex);

                statusWindow.UpdatePlayerUI(targetPlayerIndex, targetPlayer.currentHp, targetPlayer.hp, targetPlayer.currentMp, targetPlayer.mp);
            }
            yield return new WaitForSeconds(1.5f);

            if (targetPlayer.currentHp <= 0)
            {
                logText.text = $"{targetPlayer.monsterName} は倒れてしまった！";
                players.Remove(targetPlayer);

                if (statusWindow != null) statusWindow.CreateStatusWindow(players, 50f, 50f);

                yield return new WaitForSeconds(1.2f);

                if (players.Count == 0)
                {
                    logText.text = "パーティ は全滅してしまった……。\nGAME OVER";
                    yield break;
                }
            }
        }

        StartTurnSetup();
    }

    IEnumerator VictoryRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        logText.text = "敵をすべてたおした！\n戦闘に勝利した！";
    }
}