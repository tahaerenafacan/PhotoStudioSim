using System.Collections.Generic;
using UnityEngine;

public class ShopRatingManager : MonoBehaviour
{
    [Header("Rating Rules")]
    [SerializeReference] private List<RatingRule> ratingRules = new();
    [SerializeField] private int minRating = 1;
    [SerializeField] private int maxRating = 5;
    [SerializeField] private int currentShopStarLevel = 3;

    public event System.Action<int, ShopRatingContext> OnRatingCalculated;

    public int CurrentShopStarLevel => Mathf.Clamp(currentShopStarLevel, minRating, maxRating);

    private void Awake()
    {
        if (ratingRules.Count == 0)
        {
            ratingRules.Add(new ShopLevelRatingRule { Weight = 1f });
            ratingRules.Add(new WaitTimeRatingRule { Weight = 1.5f });
            ratingRules.Add(new OrderAccuracyRatingRule { Weight = 2f });
            ratingRules.Add(new MaterialQualityRatingRule { Weight = 1f });
        }
    }

    public int CalculateRating(ShopRatingContext context)
    {
        if (context == null || ratingRules.Count == 0)
        {
            return minRating;
        }

        float totalWeight = 0f;
        float weightedScore = 0f;

        foreach (var rule in ratingRules)
        {
            totalWeight += rule.Weight;
            weightedScore += rule.Evaluate(context) * rule.Weight;
        }

        if (totalWeight <= 0f)
        {
            return minRating;
        }

        float normalizedScore = Mathf.Clamp01(weightedScore / totalWeight);
        int rating = Mathf.RoundToInt(normalizedScore * (maxRating - minRating) + minRating);
        rating = Mathf.Clamp(rating, minRating, maxRating);

        OnRatingCalculated?.Invoke(rating, context);
        return rating;
    }
}
