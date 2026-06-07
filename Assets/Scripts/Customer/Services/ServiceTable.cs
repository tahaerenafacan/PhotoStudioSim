using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

public class ServiceTable : MonoBehaviour, IServiceTable
{
    [SerializeField] private Transform servicePoint;

    private CustomerController reservedCustomer;
    private bool isBusy;
    private OrderData currentOrder;
    private Action<OrderResult> onServiceCompleteCallback;

    public bool IsAvailable => reservedCustomer == null && !isBusy;
    public Transform ServicePoint => servicePoint != null ? servicePoint : transform;

    public bool TryReserve(CustomerController customer)
    {
        if (!IsAvailable)
        {
            return false;
        }

        reservedCustomer = customer;
        return true;
    }

    public void Release()
    {
        reservedCustomer = null;
        isBusy = false;
        currentOrder = null;
        onServiceCompleteCallback = null;
    }

    public void BeginService(OrderData order, Action<OrderResult> onServiceComplete)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        if (reservedCustomer == null)
        {
            throw new InvalidOperationException("Service table must be reserved before beginning service.");
        }

        if (isBusy)
        {
            return;
        }

        isBusy = true;
        currentOrder = order;
        onServiceCompleteCallback = onServiceComplete;
    }

    public void CompleteService()
    {
        if (currentOrder == null || onServiceCompleteCallback == null)
        {
            return;
        }

        // Get envelope from player and verify papers
        float accuracyScore;
        float materialQualityScore;

        if (PlayerItemHolder.Instance?.CurrentItem is ItemEnvelope envelope)
        {
            var expectedSettings = PrintSettings.FromOrderData(currentOrder);
            int totalPapers = envelope.StoredPhotos.Count;

            if (totalPapers > 0)
            {
                float totalAccuracy = 0f;

                foreach (var storedPhoto in envelope.StoredPhotos)
                {
                    totalAccuracy += ComparePrintSettingsNormalized(storedPhoto.settings, expectedSettings);
                }

                accuracyScore = totalAccuracy / totalPapers;

                materialQualityScore = GetQualityScore(expectedSettings.quality);
            }
            else
            {
                accuracyScore = 0f;
                materialQualityScore = 0f;
            }
        }
        else
        {
            accuracyScore = 0f;
            materialQualityScore = 0f;
        }

        var result = new OrderResult
        {
            OrderId = currentOrder.OrderId,
            AccuracyScore = accuracyScore,
            MaterialQualityScore = materialQualityScore,
            CompletedAt = Time.time
        };

        Debug.Log($"ServiceTable: Completed order {currentOrder.OrderId} - Accuracy: {accuracyScore}, Quality: {materialQualityScore}");

        onServiceCompleteCallback?.Invoke(result);
        Release();
    }

    private float ComparePrintSettingsNormalized(PrintSettings actual, PrintSettings expected)
    {
        float score = 0f;
        if (actual.paperSize        == expected.paperSize)          score += 1f;
        if (actual.paperOrientation == expected.paperOrientation)   score += 1f;
        if (actual.paperFit         == expected.paperFit)           score += 1f;
        if (actual.isColored        == expected.isColored)          score += 1f;
        return score / 4f;
    }

    private float GetQualityScore(PrintQuality quality)
    {
        return quality switch
        {
            PrintQuality.Low => 0.25f,
            PrintQuality.Average => 0.50f,
            PrintQuality.High => 0.75f,
            PrintQuality.UltraHigh => 1f,
            _ => -1f
        };
    }
}
