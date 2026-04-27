using TMPro;
using UnityEngine;

public class MyStudioApp : AppWindow
{
    [SerializeField] private TextMeshProUGUI studioNameText;
    [SerializeField] private RectTransform studioStarsContainer;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject emptyStarPrefab;
    [SerializeField] private Sprite likeIcon;
    [SerializeField] private Sprite dislikeIcon;

    [Header("Reviews")]
    [SerializeField] private RectTransform reviewsContainer;
    [SerializeField] private ReviewItem reviewPrefab;

    protected override void OnOpened()
    {
        base.OnOpened();
        UpdateStudioInfo();
        LoadReviews();
    }

    private void UpdateStudioInfo()
    {
        studioNameText.text = "My Studio Placeholder";
        FunctionLibrary.SetStars(studioStarsContainer, ShopRatingManager.Instance.CurrentShopStarLevel, starPrefab, emptyStarPrefab);
    }

    private void LoadReviews()
    {
        foreach (var review in ShopRatingManager.Instance.Reviews)
        {
            var reviewItem = Instantiate(reviewPrefab, reviewsContainer);
            var icon = review.Sentiment == ReviewSentiment.Positive ? likeIcon : dislikeIcon;
            var comment = review.Comments[Random.Range(0, review.Comments.Count)];

            reviewItem.Initialize(review.Rating, icon, comment.GetLocalizedString(), starPrefab, emptyStarPrefab);
        }
    }
}
