using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputRebindRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionLabel;
    [SerializeField] private Image keyIcon;
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Image listeningOverlay;

    private string actionName;
    private int bindingIndex;
    private Action<string, int> onRequestRebind;

    public void Setup(string actionName, string displayText, string currentKeyText, Sprite keySprite, int bindingIndex, Action<string, int> onRequestRebind)
    {
        this.actionName = actionName;
        this.bindingIndex = bindingIndex;
        this.onRequestRebind = onRequestRebind;

        actionLabel.text = displayText;
        if (keyLabel != null && !string.IsNullOrWhiteSpace(currentKeyText)) keyLabel.text = currentKeyText;
        if (keySprite != null)
        {
            keyIcon.sprite = keySprite;
            keyIcon.gameObject.SetActive(true);
        }
        else
        {
            keyIcon.gameObject.SetActive(false);
        }

        if (rebindButton != null)
        {
            rebindButton.onClick.RemoveAllListeners();
            rebindButton.onClick.AddListener(OnRebindButtonClicked);
        }

        SetListening(false);
    }

    public void SetListening(bool isListening)
    {
        if (listeningOverlay != null)
            listeningOverlay.gameObject.SetActive(isListening);

        if (rebindButton != null)
            rebindButton.interactable = !isListening;
    }

    private void OnRebindButtonClicked()
    {
        onRequestRebind?.Invoke(actionName, bindingIndex);
        SetListening(true);
    }

    public void UpdateBinding(string currentKeyText, Sprite keySprite)
    {
        keyLabel.text = currentKeyText;
        if (keySprite != null)
        {
            keyIcon.sprite = keySprite;
            keyIcon.gameObject.SetActive(true);
        }
        else
        {
            keyIcon.gameObject.SetActive(false);
        }
        SetListening(false);
    }
}
