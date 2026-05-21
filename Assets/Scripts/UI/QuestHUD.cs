using DG.Tweening;
using TMPro;
using UnityEngine;

public class QuestHUD : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private RectTransform questPanel;
    [SerializeField] private TextMeshProUGUI orderTypeText;
    [SerializeField] private TextMeshProUGUI orderDetailsText;
    [SerializeField] private TextMeshProUGUI colorText;
    [SerializeField] private TextMeshProUGUI orientationText;
    [SerializeField] private TextMeshProUGUI fitText;
    [SerializeField] private TextMeshProUGUI paperSizeText;

    [SerializeField] private float startXPos = 0f;
    [SerializeField] private float endXPos = 350f;
    [SerializeField] private float animationTime = 0.5f;
    
    private void OnEnable()
    {
        orderManager.OnOrderRegistered += SetQuestDisplay;
        orderManager.OnOrderCompleted += CloseQuestDisplay;
    }

    private void OnDisable()
    {
        orderManager.OnOrderRegistered -= SetQuestDisplay;
        orderManager.OnOrderCompleted -= CloseQuestDisplay;
    }

    public void SetQuestDisplay(OrderData orderData)
    {
        questPanel.gameObject.SetActive(true);

        orderTypeText.text = $"Quest: {orderData.OrderType}";
        orderDetailsText.text = orderData.Description;
        colorText.text = $"Color: {(orderData.IsColored ? "Colored" : "Black & White")}";
        orientationText.text = $"Orientation: {orderData.PaperOrientation}";
        fitText.text = $"Fit: {orderData.PaperFit}";
        paperSizeText.text = $"Paper Size: {orderData.PaperSize}";

        questPanel.DOAnchorPosX(endXPos, animationTime).SetEase(Ease.OutBounce);
    }

    public void CloseQuestDisplay(OrderResult result)
    {
        questPanel.DOAnchorPosX(startXPos, animationTime).SetEase(Ease.InBack).OnComplete(() =>
        {
            questPanel.gameObject.SetActive(false);
        });
    }
}
