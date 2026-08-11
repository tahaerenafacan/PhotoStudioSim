using UnityEngine;

public static class OrderEconomyCalculator
{
    public static int CalculateEarnings(OrderData orderData, OrderResult orderResult, int shopStarLevel = 1)
    {
        if (orderData == null || orderResult == null)
        {
            return 0;
        }

        int deliveredCount = Mathf.Max(orderResult.DeliveredPhotoCount, 0);
        if (deliveredCount == 0)
        {
            return 0;
        }

        float qualityBasePrice = GetQualityPrice(orderResult.MaterialQualityScore);
        float colorMultiplier = orderData.IsColored ? 1.2f : 1f;
        float accuracyMultiplier = Mathf.Clamp01(orderResult.AccuracyScore);
        float reputationMultiplier = 1f + Mathf.Clamp(shopStarLevel - 1, 0, 10) * 0.05f;

        float earningsPerPhoto = qualityBasePrice * colorMultiplier * accuracyMultiplier * reputationMultiplier;
        float earnings = earningsPerPhoto * deliveredCount;

        return Mathf.Max(0, Mathf.RoundToInt(earnings));
    }

    private static float GetQualityPrice(float qualityScore)
    {
        if (qualityScore < 0.375f) return 5f;
        if (qualityScore < 0.625f) return 15f;
        if (qualityScore < 0.875f) return 22f;
        return 30f;
    }
}
