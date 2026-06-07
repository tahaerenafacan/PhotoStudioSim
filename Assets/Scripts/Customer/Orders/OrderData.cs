using System;

[Serializable]
public class OrderData
{
    public string OrderId = Guid.NewGuid().ToString();
    public OrderType OrderType;
    public PhotoOrderVariant PhotoVariant;
    public string Description;
    public PrintPaperSize PaperSize;
    public PrintPaperOrientation PaperOrientation;
    public PrintPaperFit PaperFit;
    public bool IsColored;
    public float RequestedAt;

    public static OrderData CreatePhotoOrder(PhotoOrderVariant variant)
    {
        return new OrderData
        {
            OrderType = OrderType.PhotoShooting,
            PhotoVariant = variant,
            Description = $"Photo shoot request: {variant}",
            PaperSize = GetRandomPaperSize(),
            PaperOrientation = GetRandomPaperOrientation(),
            PaperFit = GetRandomPaperFit(),
            IsColored = GetRandomIsColored(),
            RequestedAt = UnityEngine.Time.time
        };
    }

    public static OrderData CreateUsbExtractionOrder()
    {
        return new OrderData
        {
            OrderType = OrderType.UsbExtraction,
            Description = "USB extraction request",
            PaperSize = GetRandomPaperSize(),
            PaperOrientation = GetRandomPaperOrientation(),
            PaperFit = GetRandomPaperFit(),
            IsColored = GetRandomIsColored(),
            RequestedAt = UnityEngine.Time.time
        };
    }

    public static OrderData CreatePhotoCopyOrder()
    {
        return new OrderData
        {
            OrderType = OrderType.PhotoCopy,
            Description = "Photo copy request",
            PaperSize = GetRandomPaperSize(),
            PaperOrientation = GetRandomPaperOrientation(),
            PaperFit = GetRandomPaperFit(),
            IsColored = GetRandomIsColored(),
            RequestedAt = UnityEngine.Time.time
        };
    }

    private static PrintQuality GetRandomQuality()
    {
        PrintQuality[] qualities = (PrintQuality[])Enum.GetValues(typeof(PrintQuality));
        return qualities[UnityEngine.Random.Range(0, qualities.Length)];
    }

    private static PrintPaperSize GetRandomPaperSize()
    {
        PrintPaperSize[] sizes = (PrintPaperSize[])Enum.GetValues(typeof(PrintPaperSize));
        return sizes[UnityEngine.Random.Range(0, sizes.Length)];
    }

    private static PrintPaperOrientation GetRandomPaperOrientation()
    {
        PrintPaperOrientation[] orientations = (PrintPaperOrientation[])Enum.GetValues(typeof(PrintPaperOrientation));
        return orientations[UnityEngine.Random.Range(0, orientations.Length)];
    }

    private static PrintPaperFit GetRandomPaperFit()
    {
        PrintPaperFit[] fits = (PrintPaperFit[])Enum.GetValues(typeof(PrintPaperFit));
        return fits[UnityEngine.Random.Range(0, fits.Length)];
    }

    private static bool GetRandomIsColored()
    {
        return UnityEngine.Random.value > 0.5f;
    }
}
