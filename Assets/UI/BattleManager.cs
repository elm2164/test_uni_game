using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem; // 🌟 既存の参照を完全に維持

public class BattleManager : MonoBehaviour
{
    [Header("--- 共通パネル・システム ---")]
    [SerializeField] private GameObject panelLogWindow;
    [SerializeField] private TextAsset jsonFile;

    [Header("--- 使い回す万能メニューウインドウ ---")]
    [SerializeField] private MenuWindow generalMenuWindow;

    [Header("--- テキスト関連 ---")]
    [SerializeField] private TextMeshProUGUI logText;

    private List<MonsterData> players = new List<MonsterData>();
    private List<MonsterData> enemies = new List<MonsterData>();
    private Dictionary<string, MonsterData> monsterDictionary;

    // 🌟 完全版の BattlePhase 列挙型と一致
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

    public void InitializeBattle(List<MonsterData> inputPlayers, List<MonsterData> inputEnemies)
    {
        monsterDictionary = BattleInitializer.LoadMonsterDictionary(jsonFile);
        players = BattleInitializer.SetupPlayers(inputPlayers);
        enemies = BattleInitializer.SetupEnemies(inputEnemies);

        isInitializedExternally = true;
        StartTurnSetup();
    }

    void Start()
    {
        if (!isInitializedExternally)
        {
            monsterDictionary = BattleInitializer.LoadMonsterDictionary(jsonFile);

            var testPlayers = Battle.CreateTestPlayers(monsterDictionary);
            var testEnemies = Battle.CreateTestEnemies(monsterDictionary);

            InitializeBattle(testPlayers, testEnemies);
        }
    }

    void StartTurnSetup()
    {
        chosenActions.Clear();
        currentPlayerIndex = 0;
        ChangePhase(BattlePhase.MainMenuSelect);
    }

    // 🌟 修正・完全版の ChangePhase メソッド
    void ChangePhase(BattlePhase nextPhase)
    {
        currentPhase = nextPhase;

        // フェーズ切り替え時は、常に一度メニューウィンドウを閉じる
        generalMenuWindow.Close();
        panelLogWindow.SetActive(false);

        switch (currentPhase)
        {
            case BattlePhase.MainMenuSelect:
                // 全体のコマンド選択なので、タイトル引数は無し（空文字）
                generalMenuWindow.CreateMenu(
                    new List<string> { "たたかう", "どうぐ", "スカウト", "にげる" },
                    0f, 0f,
                    OnMainMenuConfirmed
                );
                break;

            case BattlePhase.BattleMenuSelect:
                // 現在のプレイヤー名をタイトル引数としてMenuWindowに渡す
                string pName = (currentPlayerIndex < players.Count) ? players[currentPlayerIndex].monsterName : "";

                generalMenuWindow.CreateMenu(
                    new List<string> { "こうげき", "スキル", "ぼうぎょ", "もどる" },
                    0f, 0f,
                    OnBattleMenuConfirmed,
                    OnBattleMenuCanceled,
                    pName // 🌟 タイトル引数
                );
                break;

            case BattlePhase.TargetSelect:
                // ターゲット選択中もプレイヤー名を維持するため、同じ名前をタイトル引数に渡す
                string targetTitle = (currentPlayerIndex < players.Count) ? players[currentPlayerIndex].monsterName : "";

                List<string> targetNames = new List<string>();
                foreach (var enemy in enemies) targetNames.Add(enemy.monsterName);
                targetNames.Add("もどる");

                generalMenuWindow.CreateMenu(
                    targetNames,
                    0f, 0f,
                    OnTargetMenuConfirmed,
                    OnTargetMenuCanceled,
                    targetTitle // 🌟 タイトル引数
                );
                break;

            case BattlePhase.EventProcessing:
                // メニューウィンドウは閉じた状態で、ログウィンドウのみを有効化
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

        // 🌟 元の正常な入力渡しを完全に維持
        generalMenuWindow.HandleInput(keyboard);
    }

    void OnMainMenuConfirmed(int selection)
    {
        if (selection == 0)
        {
            ChangePhase(BattlePhase.BattleMenuSelect);
        }
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

    void SaveAction(CommandType command, MonsterData target)
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

    // 🌟 修正・完全版の ExecuteTurnRoutine
    IEnumerator ExecuteTurnRoutine()
    {
        // --- 1. 味方の行動処理 ---
        foreach (var action in chosenActions)
        {
            if (action.user.hp <= 0) continue;

            if (action.command == CommandType.Attack)
            {
                if (action.targetEnemy.hp <= 0)
                {
                    if (enemies.Count > 0) action.targetEnemy = enemies[0];
                    else break;
                }

                int damage = 0;
                logText.text = BattleLogic.ExecutePlayerAttack(action, out damage);
                yield return new WaitForSeconds(1.5f);

                if (action.targetEnemy.hp <= 0)
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
        foreach (var activeEnemy in enemies)
        {
            if (activeEnemy.hp <= 0) continue;
            if (players.Count == 0) yield break;

            MonsterData targetPlayer = EnemyAI.DecideTarget(players);

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
            yield return new WaitForSeconds(1.5f);

            if (targetPlayer.hp <= 0)
            {
                logText.text = $"{targetPlayer.monsterName} は倒れてしまった！";
                players.Remove(targetPlayer);
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