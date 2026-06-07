using UnityEngine.Localization;

/// <summary>
/// Kural bazlı lokalize yorum üretmek isteyen rating kuralları bu arayüzü uygular.
/// Her kuralın yorum üretmesi gerekmez; sadece ihtiyaç duyanlar implemente eder (ISP).
/// </summary>
public interface IReviewCommentProvider
{
    /// <param name="evaluatedScore">0–1 arası normalize edilmiş kural skoru</param>
    /// <returns>Lokalize yorum; skor için yorum üretilmeyecekse null</returns>
    LocalizedString GetComment(float evaluatedScore);
}