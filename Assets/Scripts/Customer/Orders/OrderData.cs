using System;

[Serializable]
public class OrderData
{
    public string OrderId = Guid.NewGuid().ToString();
    public OrderType OrderType;
    public PhotoOrderVariant PhotoVariant;
    public string Description;
    public PrintQuality RequestedQuality;
    public float RequestedAt;

    public static OrderData CreatePhotoOrder(PhotoOrderVariant variant)
    {
        return new OrderData
        {
            OrderType = OrderType.PhotoShooting,
            PhotoVariant = variant,
            Description = $"Photo shoot request: {variant}",
            RequestedQuality = GetRandomQuality(),
            RequestedAt = UnityEngine.Time.time
        };
    }

    public static OrderData CreateUsbExtractionOrder()
    {
        return new OrderData
        {
            OrderType = OrderType.UsbExtraction,
            Description = "USB extraction request",
            RequestedQuality = GetRandomQuality(),
            RequestedAt = UnityEngine.Time.time
        };
    }

    public static OrderData CreatePhotoCopyOrder()
    {
        return new OrderData
        {
            OrderType = OrderType.PhotoCopy,
            Description = "Photo copy request",
            RequestedQuality = GetRandomQuality(),
            RequestedAt = UnityEngine.Time.time
        };
    }

    private static PrintQuality GetRandomQuality()
    {
        PrintQuality[] qualities = (PrintQuality[])Enum.GetValues(typeof(PrintQuality));
        return qualities[UnityEngine.Random.Range(0, qualities.Length)];
    }
}
