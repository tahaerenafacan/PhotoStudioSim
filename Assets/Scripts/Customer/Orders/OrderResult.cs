using System;

[Serializable]
public class OrderResult
{
    public string OrderId;
    public float AccuracyScore;
    public float MaterialQualityScore;
    public float CompletedAt;
    public int Earnings;
    public int DeliveredPhotoCount;
}
