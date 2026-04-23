using System;

[Serializable]
public class OrderResult
{
    public string OrderId;
    public bool CompletedSuccessfully;
    public float AccuracyScore;
    public float MaterialQualityScore;
    public float CompletedAt;
}
