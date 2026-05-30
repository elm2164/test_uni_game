using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;

public class MenuWindow : MonoBehaviour
{
  [SerializeField] private GameObject optionPrefab;
  [SerializeField] private GameObject cursorPrefab;

  [Header("--- カーソルの配置設定 ---")]
  [SerializeField] private float cursorDistanceX = 45f;
  [SerializeField] private float stepY = 50f;

  [Header("--- 余白の設定 ---")]
  [SerializeField] private float marginLeft = 20f;
  [SerializeField] private float marginTop = 20f;

  [Header("--- タイトルの設定 ---")]
  [SerializeField] private GameObject titlePrefab;
  [SerializeField] private float titleSpaceY = 40f;
  [SerializeField] private GameObject titlePanelPrefab;

  [Header("--- 背景パネルの設定 ---")]
  [SerializeField] private GameObject panelPrefab;

  private List<RectTransform> generatedOptions = new List<RectTransform>();
  private RectTransform instantiatedCursor;
  private int currentSelection = 0;

  private Action<int> onConfirmAction;
  private Action onCancelAction;

  private GameObject instantiatedPanel;

  public void CreateMenu(
      List<string> optionNames,
      float posX,
      float posY,
      Action<int> onConfirm,
      Action onCancel = null,
      string title = "")
  {
    Close();

    onConfirmAction = onConfirm;
    onCancelAction = onCancel;
    currentSelection = 0;

    Transform menuParent = this.transform;
    float baseWidth = 0f;

    // 1. 全体の背景パネルPrefabがあれば生成
    if (panelPrefab != null)
    {
      instantiatedPanel = Instantiate(panelPrefab, this.transform);
      RectTransform panelRect = instantiatedPanel.GetComponent<RectTransform>();

      if (panelRect != null)
      {
        panelRect.anchoredPosition = new Vector2(posX, posY);
        baseWidth = panelRect.rect.width; // ストレッチされた横幅を記憶

        posX = 0f;
        posY = 0f;
      }

      menuParent = instantiatedPanel.transform;
    }

    // 2. タイトル表示の処理
    if (!string.IsNullOrEmpty(title))
    {
      Transform titleParent = menuParent;

      // 2-a. タイトル背景パネルPrefabの生成とサイズ・位置の最適化
      if (titlePanelPrefab != null)
      {
        GameObject titlePanelGo = Instantiate(titlePanelPrefab, menuParent);
        RectTransform titlePanelRect = titlePanelGo.GetComponent<RectTransform>();

        if (titlePanelRect != null)
        {
          // アンカーとピボットを左上に完全に固定
          titlePanelRect.anchorMin = new Vector2(0f, 1f);
          titlePanelRect.anchorMax = new Vector2(0f, 1f);
          titlePanelRect.pivot = new Vector2(0f, 1f);

          // 高さをインスペクターで指定された titleSpaceY と完全に一致させる
          float panelW = baseWidth > 0f ? baseWidth : titlePanelRect.sizeDelta.x;
          float panelH = titleSpaceY;

          titlePanelRect.sizeDelta = new Vector2(panelW, panelH);

          // 開始Y座標を「選択肢パネルの最上端（posY）」から、
          // タイトルパネルの高さ分（titleSpaceY）だけちょうど真上に持ち上げる
          float titlePanelY = posY + titleSpaceY;
          titlePanelRect.anchoredPosition = new Vector2(posX, titlePanelY);
        }

        titleParent = titlePanelGo.transform;
      }

      // 2-b. タイトルテキストの生成と配置
      if (titlePrefab != null)
      {
        GameObject titleGo = Instantiate(titlePrefab, titleParent);
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();

        if (titleRect != null)
        {
          if (titlePanelPrefab == null)
          {
            float titleY = posY - marginTop + titleSpaceY;
            titleRect.anchoredPosition = new Vector2(posX + marginLeft, titleY);
          }
          else
          {
            // タイトルパネルの完全に「中央・左寄せ」にテキストを配置
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(1f, 0.5f);
            titleRect.pivot = new Vector2(0f, 0.5f);

            titleRect.anchoredPosition = new Vector2(marginLeft, 0f);
          }
        }

        if (titleTmp != null)
        {
          titleTmp.text = title;
        }
      }
    }

    // 3. カーソルの生成
    if (instantiatedCursor == null && cursorPrefab != null)
    {
      GameObject cursorGo = Instantiate(cursorPrefab, menuParent);
      instantiatedCursor = cursorGo.GetComponent<RectTransform>();
    }
    else if (instantiatedCursor != null)
    {
      instantiatedCursor.SetParent(menuParent, false);
    }

    // 4. 選択肢（オプション）の動的生成と配置
    for (int i = 0; i < optionNames.Count; i++)
    {
      GameObject go = Instantiate(optionPrefab, menuParent);
      RectTransform rect = go.GetComponent<RectTransform>();
      TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();

      float textSpaceX = posX + marginLeft + cursorDistanceX;
      float textSpaceY = posY - marginTop - (i * stepY);

      rect.anchoredPosition = new Vector2(textSpaceX, textSpaceY);
      tmp.text = optionNames[i];

      generatedOptions.Add(rect);
    }

    gameObject.SetActive(true);
    UpdateCursor();
  }

  public void Close()
  {
    if (instantiatedCursor != null)
    {
      instantiatedCursor.SetParent(this.transform, false);
      instantiatedCursor.gameObject.SetActive(false);
    }

    foreach (var opt in generatedOptions)
    {
      if (opt != null) Destroy(opt.gameObject);
    }
    generatedOptions.Clear();

    if (instantiatedPanel != null)
    {
      Destroy(instantiatedPanel);
      instantiatedPanel = null;
    }

    gameObject.SetActive(false);
  }

  public void HandleInput(Keyboard keyboard)
  {
    if (generatedOptions.Count == 0) return;

    if (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
    {
      onCancelAction?.Invoke();
      return;
    }

    int prevSelection = currentSelection;

    if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
      currentSelection = Mathf.Max(0, currentSelection - 1);
    else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
      currentSelection = Mathf.Min(generatedOptions.Count - 1, currentSelection + 1);

    if (prevSelection != currentSelection)
    {
      UpdateCursor();
    }

    if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
    {
      onConfirmAction?.Invoke(currentSelection);
    }
  }

  private void UpdateCursor()
  {
    if (instantiatedCursor == null) return;
    if (generatedOptions.Count == 0 || currentSelection >= generatedOptions.Count) return;

    instantiatedCursor.gameObject.SetActive(true);

    RectTransform targetRect = generatedOptions[currentSelection];
    instantiatedCursor.SetParent(targetRect, false);
    instantiatedCursor.anchoredPosition = new Vector2(-cursorDistanceX, 0f);
  }
}