using DG.Tweening;
using TMPro;
using UnityEngine;

public class QuestHUD : MonoBehaviour
{
    [SerializeField] private RectTransform questPanel;
    [SerializeField] private TextMeshProUGUI orderTypeText;
    [SerializeField] private TextMeshProUGUI orderDetailsText;
    [SerializeField] private TextMeshProUGUI colorText;
    [SerializeField] private TextMeshProUGUI orientationText;
    [SerializeField] private TextMeshProUGUI fitText;
    [SerializeField] private TextMeshProUGUI paperSizeText;

    public void SetQuestDisplay(OrderData orderData)
    {
        questPanel.gameObject.SetActive(true);

        orderTypeText.text = $"Quest: {orderData.OrderType}";
        orderDetailsText.text = orderData.Description;
        colorText.text = $"Color: {(orderData.IsColored ? "Colored" : "Black & White")}";
        orientationText.text = $"Orientation: {orderData.PaperOrientation}";
        fitText.text = $"Fit: {orderData.PaperFit}";
        paperSizeText.text = $"Paper Size: {orderData.PaperSize}";

        questPanel.DOAnchorPosX(-350f, 0.5f).SetEase(Ease.OutBounce);
    }

    public void CloseQuestDisplay()
    {
        questPanel.DOAnchorPosX(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            questPanel.gameObject.SetActive(false);
        });
    }
}
