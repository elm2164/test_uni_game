using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    [Header("--- 共通パネル・システム ---")]
    [SerializeField] private GameObject panelLogWindow;
    [SerializeField] private TextAsset jsonFile; // ★cursorTextへの参照を削除！

    [Header("--- 使い回す万能メニューウインドウ ---")]
    [SerializeField] private MenuWindow generalMenuWindow;

    [Header("--- テキスト関連 ---")]
    [SerializeField] private TextMeshProUGUI logText;

    private List<MonsterData> players = new List<MonsterData>();
    private List<MonsterData> enemies = new List<MonsterData>();
    private Dictionary<string, MonsterData> monsterDictionary;

    enum BattlePhase
    {
        MainMenuSelect,
        BattleMenuSelect,
        TargetSelect,
        EventProcessing
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
        // 💡 外部から呼ばれていない（単体テストプレイ時）なら、隔離したTestクラスからデータを貰う
        if (!isInitializedExternally)
        {
            monsterDictionary = BattleInitializer.LoadMonsterDictionary(jsonFile);

            // ★ MyGame.Test. を消去し、スッキリしたグローバルなBattleクラスを呼び出す
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

    void ChangePhase(BattlePhase nextPhase)
    {
        currentPhase = nextPhase;

        generalMenuWindow.Close();
        panelLogWindow.SetActive(false);
        // ★「cursorText.gameObject.SetActive(true)」のようなカーソル制御を削除！

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
                generalMenuWindow.CreateMenu(
                    new List<string> { "こうげき", "スキル", "ぼうぎょ", "もどる" },
                    0f, 0f,
                    OnBattleMenuConfirmed,
                    OnBattleMenuCanceled
                );
                break;

            case BattlePhase.TargetSelect:
                List<string> targetNames = new List<string>();
                foreach (var enemy in enemies) targetNames.Add(enemy.monsterName);
                targetNames.Add("もどる");

                generalMenuWindow.CreateMenu(
                    targetNames,
                    0f, 0f,
                    OnTargetMenuConfirmed,
                    OnTargetMenuCanceled
                );
                break;

            case BattlePhase.EventProcessing:
                panelLogWindow.SetActive(true);
                break;
        }
    }

    void Update()
    {
        if (currentPhase == BattlePhase.EventProcessing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ★引数から「cursorText」を消し去り、ただ入力を丸投げするだけに！
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
            StartCoroutine(ExecuteTurnRoutine());
        }
    }

    IEnumerator ExecuteTurnRoutine()
    {
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

                int finalDamage = BattleLogic.CalculatePhysicalDamage(action.user, action.targetEnemy);

                logText.text = $"{action.user.monsterName} の攻撃！\n{action.targetEnemy.monsterName}に {finalDamage} のダメージ！";
                action.targetEnemy.hp -= finalDamage;
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

            int finalEnemyDamage = BattleLogic.CalculatePhysicalDamage(activeEnemy, targetPlayer);

            if (isTargetDefending)
            {
                finalEnemyDamage = Mathf.Max(1, finalEnemyDamage / 2);
                logText.text = $"【{targetPlayer.monsterName}のぼうぎょ成功！】\nダメージを軽減した！";
                yield return new WaitForSeconds(1.2f);
            }

            logText.text = $"敵の {activeEnemy.monsterName} の攻撃！\n{targetPlayer.monsterName}は {finalEnemyDamage} のダメージ！";
            targetPlayer.hp -= finalEnemyDamage;
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