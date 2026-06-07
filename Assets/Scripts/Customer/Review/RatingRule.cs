using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public abstract class RatingRule : IRatingRule
{
    [SerializeField] private float weight = 1f;

    public float Weight => weight;

    public abstract float Evaluate(ShopRatingContext context);
}

[Serializable]
public class WaitTimeRatingRule : RatingRule, IReviewCommentProvider
{
    [SerializeField] private float optimalWaitSeconds = 3f;
    [SerializeField] private float maxPenaltyWaitSeconds = 15f;
    [SerializeField] private LocalizedString longWaitComment;
    [SerializeField] private LocalizedString fastServiceComment;
    [SerializeField] private float commentThreshold = 0.5f;

    public LocalizedString GetComment(float evaluatedScore)
    {
        var comment = evaluatedScore < commentThreshold ? longWaitComment : fastServiceComment;
        return comment.IsEmpty ? null : comment;
    } 

    public override float Evaluate(ShopRatingContext context)
    {
        if (context == null)
        {
            return 0f;
        }
        float timeDifference = context.WaitTime - optimalWaitSeconds;
        var normalized = Mathf.Clamp01(1f - (timeDifference / maxPenaltyWaitSeconds));
        return normalized;
    }
}

[Serializable]
public class OrderAccuracyRatingRule : RatingRule, IReviewCommentProvider
{
    [SerializeField] private LocalizedString lowAccuracyComment;
    [SerializeField] private LocalizedString correctOrderComment;
    [SerializeField] private float commentThreshold = 0.7f;

    public LocalizedString GetComment(float evaluatedScore)
    {
        var comment = evaluatedScore < commentThreshold ? lowAccuracyComment : correctOrderComment;
        return comment.IsEmpty ? null : comment;
    }

    public override float Evaluate(ShopRatingContext context)
    {
        if (context == null)
        {
            return 0f;
        }

        return Mathf.Clamp01(context.OrderAccuracyScore);
    }
}

[Serializable]
public class MaterialQualityRatingRule : RatingRule, IReviewCommentProvider
{
    [SerializeField] private LocalizedString lowQualityComment;
    [SerializeField] private LocalizedString highQualityComment;
    [SerializeField] private float commentThreshold = 0.8f;

    public LocalizedString GetComment(float evaluatedScore)
    {
        var comment = evaluatedScore < commentThreshold ? lowQualityComment : highQualityComment;
        return comment.IsEmpty ? null : comment;
    }

    public override float Evaluate(ShopRatingContext context)
    {
        if (context == null)
        {
            return 0f;
        }

        return Mathf.Clamp01(context.MaterialQualityScore);
    }
}
