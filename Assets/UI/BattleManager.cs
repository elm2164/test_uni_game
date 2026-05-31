using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    [Header("--- 共通パネル・システム ---")]
    [SerializeField] private GameObject panelLogWindow;
    [SerializeField] private TextAsset jsonFile; // 単体テストでのみ使用

    [Header("--- 使い回す万能メニューウインドウ ---")]
    [SerializeField] private MenuWindow generalMenuWindow;

    [Header("--- ステータス画面の配置システム ---")]
    [SerializeField] private BattleStatusWindow statusWindow; // 🌟 追加：ステータスUIウィンドウへの参照

    [Header("--- テキスト関連 ---")]
    [SerializeField] private TextMeshProUGUI logText;

    // 🌟 全て MonsterData から MonsterStatus に移行
    private List<MonsterStatus> players = new List<MonsterStatus>();
    private List<MonsterStatus> enemies = new List<MonsterStatus>();

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

    /// <summary>
    /// 🌟 外部（フィールド画面など）から直接 MonsterStatus の実体リストを貰って戦闘を開始する本番用メソッド
    /// </summary>
    public void InitializeBattle(List<MonsterStatus> inputPlayers, List<MonsterStatus> inputEnemies)
    {
        players = inputPlayers;
        enemies = inputEnemies;

        isInitializedExternally = true;

        // 🌟 画面左下にステータスUIを生成配置
        if (statusWindow != null)
        {
            statusWindow.CreateStatusWindow(players, 50f, 50f);
        }

        StartTurnSetup();
    }

    void Start()
    {
        if (!isInitializedExternally)
        {
            // 🌟 単体テスト時：JSONファイルの読み込みや初期レベルの適用は全て Tests/Battle 側に一任する
            var testPlayers = Battle.CreateTestPlayers(jsonFile);
            var testEnemies = Battle.CreateTestEnemies(jsonFile);

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

    IEnumerator ExecuteTurnRoutine()
    {
        // --- 1. 味方の行動処理 ---
        foreach (var action in chosenActions)
        {
            if (action.user.currentHp <= 0) continue; // currentHpを参照

            if (action.command == CommandType.Attack)
            {
                if (action.targetEnemy.currentHp <= 0)
                {
                    if (enemies.Count > 0) action.targetEnemy = enemies[0];
                    else break;
                }

                int damage = 0;
                logText.text = BattleLogic.ExecutePlayerAttack(action, out damage);
                yield return new WaitForSeconds(1.5f);

                if (action.targetEnemy.currentHp <= 0)
                {
                    logText.text = $"{action.targetEnemy.monsterName} をたおした！";
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

            // 🌟 HPの減少をリアルタイムでステータスUIに反映
            if (statusWindow != null && targetPlayerIndex >= 0)
            {
                statusWindow.UpdatePlayerUI(targetPlayerIndex, targetPlayer.currentHp, targetPlayer.hp, targetPlayer.currentMp, targetPlayer.mp);
            }
            yield return new WaitForSeconds(1.5f);

            if (targetPlayer.currentHp <= 0)
            {
                logText.text = $"{targetPlayer.monsterName} は倒れてしまった！";
                players.Remove(targetPlayer);

                // プレイヤー死亡時にUIを一度リフレッシュ（またはグレーアウト処理など。ここでは一括再描画クリア）
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