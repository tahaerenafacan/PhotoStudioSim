using DG.Tweening;
using UnityEngine;

public class ComputerConnectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform connectionPanel;
    private const float animationDuration = 0.8f;

    private void Start()
    {
        connectionPanel.gameObject.SetActive(false);
    }

    public void ToggleConnectionPanel()
    {
        if (connectionPanel.gameObject.activeSelf)
        {
            HideConnectionPanel();
        }
        else
        {
            ShowConnectionPanel();
        }
    }

    private void ShowConnectionPanel()
    {
        connectionPanel.gameObject.SetActive(true);
        DOTween.Sequence().Append(
            connectionPanel.DOAnchorPosY(50f, animationDuration).SetEase(Ease.OutBack)
            );
    }

    private void HideConnectionPanel()
    {
        DOTween.Sequence().Append(
            connectionPanel.DOAnchorPosY(-300f, animationDuration).SetEase(Ease.InBack)
            ).OnComplete(() => connectionPanel.gameObject.SetActive(false));
    }
}
