using DG.Tweening;
using UnityEngine;

public class ComputerConnectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform connectionPanel;

    private void Start()
    {
        connectionPanel.gameObject.SetActive(false);
    }

    public void ShowConnectionPanel()
    {
        connectionPanel.gameObject.SetActive(true);
        DOTween.Sequence().Append(
            connectionPanel.DOAnchorPosY(0f, 250f).SetEase(Ease.OutBack)
            );
    }

    public void HideConnectionPanel()
    {
        DOTween.Sequence().Append(
            connectionPanel.DOAnchorPosY(-500f, 250f).SetEase(Ease.InBack)
            ).OnComplete(() => connectionPanel.gameObject.SetActive(false));
    }
}
