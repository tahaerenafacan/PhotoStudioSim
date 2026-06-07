using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class ShopRatingManager : MonoBehaviour
{
    public static ShopRatingManager Instance { get; private set; }

    public event System.Action<int, ShopRatingContext> OnRatingCalculated;
    public int CurrentShopStarLevel => Mathf.Clamp(currentShopStarLevel, minRating, maxRating);
    public float AverageRating => reviews.Count > 0
    ? (float)reviews.Sum(r => r.Rating) / reviews.Count
    : minRating;

    
    [Header("Rating Rules")]
    [SerializeField] private int minRating = 1;
    [SerializeField] private int maxRating = 5;
    [SerializeField] private int currentShopStarLevel = 1;
    private int neutralRatingThreshold = 3;

    [SerializeReference] private List<RatingRule> ratingRules = new();
    private readonly List<CustomerReview> reviews = new();
    public IReadOnlyList<CustomerReview> Reviews => reviews.AsReadOnly();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (ratingRules.Count == 0)
        {
            ratingRules.Add(new WaitTimeRatingRule {  });
            ratingRules.Add(new OrderAccuracyRatingRule {  });
            ratingRules.Add(new MaterialQualityRatingRule {  });
        }
    }
    private void Reset()
    {
        ratingRules = new List<RatingRule>
        {
            new WaitTimeRatingRule {  },
            new OrderAccuracyRatingRule {  },
            new MaterialQualityRatingRule {  }
        };
    }

    public int CalculateRating(ShopRatingContext context)
    {
        if (context == null || ratingRules.Count == 0)
        {
            return minRating;
        }

        float totalWeight   = 0f;
        float weightedScore = 0f;
        var comments        = new List<LocalizedString>();

        Debug.Log($"=== Rating Calculation Start ===\nAccuracy: {context.OrderAccuracyScore:P0}, Quality: {context.MaterialQualityScore:P0}, WaitTime: {context.WaitTime}s");

        foreach (var rule in ratingRules)
        {
            float score = rule.Evaluate(context);
            totalWeight   += rule.Weight;
            weightedScore += score * rule.Weight;

            Debug.Log($"  {rule.GetType().Name}: Score={score:F2}, Weight={rule.Weight}, Weighted={score * rule.Weight:F2}");

            // Sadece IReviewCommentProvider uygulayan kurallardan yorum toplanır
            if (rule is IReviewCommentProvider commentProvider)
            {
                var comment = commentProvider.GetComment(score);
                if (comment != null && !comment.IsEmpty)
                    comments.Add(comment);
            }
        }

        if (totalWeight <= 0f)
        {
            return minRating;
        }

        float normalizedScore = Mathf.Clamp01(weightedScore / totalWeight);
        int rating = Mathf.RoundToInt(normalizedScore * (maxRating - minRating) + minRating);
        rating = Mathf.Clamp(rating, minRating, maxRating);

        Debug.Log($"Total Weight: {totalWeight}, Weighted Score: {weightedScore:F2}, Normalized: {normalizedScore:F2} → Rating: {rating} ⭐");

        var review = new CustomerReview(rating, DetermineSentiment(rating), comments);
        reviews.Add(review);
        UpdateShopStarLevel();

        OnRatingCalculated?.Invoke(rating, context);
        return rating;
    }

    private ReviewSentiment DetermineSentiment(int rating)
    {
        return rating > neutralRatingThreshold ? ReviewSentiment.Positive : ReviewSentiment.Negative;
    }

    public void AddRatingRule(RatingRule rule)
    {
        if (rule != null && !ratingRules.Contains(rule))
        {
            ratingRules.Add(rule);
        }
    }

    public void RemoveRatingRule(RatingRule rule)
    {
        if (rule != null && ratingRules.Contains(rule))
        {
            ratingRules.Remove(rule);
        }
    }

    private void UpdateShopStarLevel()
    {
        currentShopStarLevel = Mathf.Clamp(Mathf.RoundToInt(AverageRating),minRating,maxRating);
    }
}
