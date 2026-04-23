using System;
using UnityEngine;

[Serializable]
public abstract class RatingRule : IRatingRule
{
    [SerializeField] private float weight = 1f;

    public float Weight
    {
        get => Mathf.Max(0f, weight);
        set => weight = value;
    }

    public abstract float Evaluate(ShopRatingContext context);
}

[Serializable]
public class ShopLevelRatingRule : RatingRule
{
    [SerializeField] private int maxStarLevel = 5;

    public override float Evaluate(ShopRatingContext context)
    {
        if (context == null || maxStarLevel <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((float)context.ShopStarLevel / maxStarLevel);
    }
}

[Serializable]
public class WaitTimeRatingRule : RatingRule
{
    [SerializeField] private float optimalWaitSeconds = 3f;
    [SerializeField] private float maxPenaltyWaitSeconds = 15f;

    public override float Evaluate(ShopRatingContext context)
    {
        if (context == null)
        {
            return 0f;
        }

        var normalized = Mathf.Clamp01(1f - (context.WaitTime - optimalWaitSeconds) / maxPenaltyWaitSeconds);
        return Mathf.Max(0f, normalized);
    }
}

[Serializable]
public class OrderAccuracyRatingRule : RatingRule
{
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
public class MaterialQualityRatingRule : RatingRule
{
    public override float Evaluate(ShopRatingContext context)
    {
        if (context == null)
        {
            return 0f;
        }

        return Mathf.Clamp01(context.MaterialQualityScore);
    }
}
