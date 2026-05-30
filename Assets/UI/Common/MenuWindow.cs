using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using TMPro;

public class MenuWindow : MonoBehaviour
{
  [SerializeField] private GameObject optionPrefab;
  [SerializeField] private GameObject cursorPrefab;

  [Header("--- カーソルの配置設定 ---")]
  [SerializeField] private float cursorDistanceX = 45f; // 🌟 プラスの数値（例: 60）で指定できるように変更！
  [SerializeField] private float stepY = 50f;

  [Header("--- 余白の設定 ---")]
  [SerializeField] private float marginLeft = 20f;
  [SerializeField] private float marginTop = 20f;

  private List<RectTransform> generatedOptions = new List<RectTransform>();
  private RectTransform instantiatedCursor;
  private int currentSelection = 0;

  private Action<int> onConfirmAction;
  private Action onCancelAction;

  public void CreateMenu(List<string> optionNames, float posX, float posY, Action<int> onConfirm, Action onCancel = null)
  {
    Close();

    onConfirmAction = onConfirm;
    onCancelAction = onCancel;
    currentSelection = 0;

    if (instantiatedCursor == null && cursorPrefab != null)
    {
      GameObject cursorGo = Instantiate(cursorPrefab, this.transform);
      instantiatedCursor = cursorGo.GetComponent<RectTransform>();
    }

    for (int i = 0; i < optionNames.Count; i++)
    {
      GameObject go = Instantiate(optionPrefab, this.transform);
      RectTransform rect = go.GetComponent<RectTransform>();
      TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();

      // 🌟 最終計算：カーソルの「距離」をそのまま足し算して空き地を作る
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
    // 🌟【超重要】古いテキストが消される前に、カーソルを親（MenuWindow）の直下へ避難させる！
    if (instantiatedCursor != null)
    {
      instantiatedCursor.SetParent(this.transform, false);
      instantiatedCursor.gameObject.SetActive(false); // 次のメニューが出来るまで一旦非表示に
    }

    foreach (var opt in generatedOptions)
    {
      if (opt != null) Destroy(opt.gameObject);
    }
    generatedOptions.Clear();

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

    // 🌟 最終計算：テキストの左端(0)から、指定された距離分「左（マイナス）」に配置する！
    instantiatedCursor.anchoredPosition = new Vector2(-cursorDistanceX, 0f);
  }
}