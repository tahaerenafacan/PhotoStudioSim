using UnityEngine;

public static class FunctionLibrary
{
    public static void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }

    public static void SetStars(RectTransform starsContainer, int starCount, GameObject starPrefab, GameObject emptyStarPrefab)
    {
        DestroyChildren(starsContainer);

        foreach (Transform child in starsContainer.transform)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }
        for (int i = 0; i < starCount; i++)
        {
            MonoBehaviour.Instantiate(starPrefab, starsContainer.transform);
        }
        for (int i = 0; i < 5 - starCount; i++)
        {
            MonoBehaviour.Instantiate(emptyStarPrefab, starsContainer.transform);
        }
    }

    public static void SetCanvasGroupActive(ref CanvasGroup canvasGroup, bool isEnabled)
    {
        canvasGroup.interactable = isEnabled;
        canvasGroup.blocksRaycasts = isEnabled;
        canvasGroup.alpha = isEnabled ? 1 : 0;
    }
}
