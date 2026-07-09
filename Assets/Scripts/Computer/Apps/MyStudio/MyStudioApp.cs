using TMPro;
using UnityEngine;

namespace SyntaxSultan.ComputerSystem.Apps
{
    public class MyStudioApp : AppWindow
    {
        [SerializeField] private TextMeshProUGUI studioNameText;
        [SerializeField] private Evo.UI.Button changeStudioNameButton;
        [SerializeField] private RectTransform studioStarsContainer;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private GameObject emptyStarPrefab;
        [SerializeField] private Sprite likeIcon;
        [SerializeField] private Sprite dislikeIcon;

        [Header("Reviews")]
        [SerializeField] private RectTransform reviewsContainer;
        [SerializeField] private ReviewItem reviewPrefab;

        [Header("Reputation")]
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private TextMeshProUGUI reputationLevelText;

        protected override void OnOpened()
        {
            base.OnOpened();
            UpdateStudioInfo();
            LoadReviews();

            ReputationManager.Instance.OnReputationChanged += OnReputationChanged;
            ReputationManager.Instance.OnReputationLevelUp += OnReputationLevelUp;
        }
        protected override void OnClosed()
        {
            base.OnClosed();
            ReputationManager.Instance.OnReputationChanged -= OnReputationChanged;
            ReputationManager.Instance.OnReputationLevelUp -= OnReputationLevelUp;
        }

        private void UpdateStudioInfo()
        {
            studioNameText.text = GameManager.Instance.StudioName;
            FunctionLibrary.SetStars(studioStarsContainer, ShopRatingManager.Instance.CurrentShopStarLevel, starPrefab, emptyStarPrefab);
            reputationText.text = "Reputation: " + ReputationManager.Instance.Reputation.ToString();
            reputationLevelText.text = ReputationManager.Instance.ReputationLevel.ToString();
        }
        private void OnReputationChanged(int newReputation)
        {
            reputationText.text = "Reputation: " + newReputation.ToString();
        }
        private void OnReputationLevelUp(int newLevel)
        {
            reputationLevelText.text = newLevel.ToString();
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
}


