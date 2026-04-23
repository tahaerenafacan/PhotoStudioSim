using DG.Tweening;
using TMPro;
using UnityEngine;

public class QuestHUD : MonoBehaviour
{
    [SerializeField] private RectTransform questPanel;
    [SerializeField] private TextMeshProUGUI orderTypeText;
    [SerializeField] private TextMeshProUGUI orderDetailsText;
    [SerializeField] private TextMeshProUGUI reqQualityText;

    public void SetQuestDisplay(OrderData orderData)
    {
        questPanel.gameObject.SetActive(true);

        orderTypeText.text = $"Quest: {orderData.OrderType}";
        orderDetailsText.text = orderData.Description;
        reqQualityText.text = $"Required Quality: {orderData.RequestedQuality}";

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
