using UnityEngine;

public enum PrintPaperSize
{
    A3,
    A4,
    A5
}

public enum PrintPaperOrientation
{
    Portrait,
    Landscape
}

public enum PrintPaperFit
{
    ScaleToFit,
    ActualSize
}

public enum PrintQuality
{
    Low = 0,
    Average = 1,
    High = 2,
    UltraHigh = 3
}

public struct PrintSettings
{
    public INetworkDevice targetPrinter;
    public PrintPaperSize paperSize;
    public PrintPaperOrientation paperOrientation;
    public PrintPaperFit paperFit;
    public bool isColored;
    public PrintQuality quality;

    public static PrintSettings FromOrderData(OrderData orderData)
    {
        return new PrintSettings
        {
            targetPrinter = null,
            paperSize = orderData.PaperSize,
            paperOrientation = orderData.PaperOrientation,
            paperFit = orderData.PaperFit,
            isColored = orderData.IsColored,
        };
    }
}