using System;
using System.Collections.Generic;
using UnityEngine.Localization;

/// <summary>
/// Bir müşteri ziyaretinden doğan değerlendirme kaydı.
/// Immutable tutularak geçmiş verinin kazara değiştirilmesi engellenir.
/// </summary>
[Serializable]
public class CustomerReview
{
    public readonly int Rating;
    public readonly ReviewSentiment Sentiment;
    public readonly IReadOnlyList<LocalizedString> Comments;

    public CustomerReview(int rating, ReviewSentiment sentiment, List<LocalizedString> comments)
    {
        Rating     = rating;
        Sentiment  = sentiment;
        Comments   = comments.AsReadOnly();
    }
}

public enum ReviewSentiment
{
    Negative,
    Positive
}