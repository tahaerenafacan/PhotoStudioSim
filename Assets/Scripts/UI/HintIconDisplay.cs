using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintIconDisplay : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI labelText;

    public void Setup(Sprite icon, string text)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        if (labelText != null)
        {
            labelText.text = text ?? string.Empty;
        }
    }
}