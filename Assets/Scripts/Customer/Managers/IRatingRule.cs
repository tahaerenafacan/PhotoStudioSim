public interface IRatingRule
{
    float Weight { get; }
    float Evaluate(ShopRatingContext context);
}
