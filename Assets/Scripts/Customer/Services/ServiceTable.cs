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
        float accuracyScore = 0f;
        float materialQualityScore = 0f;

        if (PlayerItemHolder.Instance?.CurrentItem is ItemEnvelope envelope)
        {
            var expectedSettings = PrintSettings.FromOrderData(currentOrder);
            int totalPapers = envelope.StoredPhotos.Count;

            if (totalPapers > 0)
            {
                int matchingPapers = 0;

                foreach (var storedPhoto in envelope.StoredPhotos)
                {
                    if (ComparePrintSettings(storedPhoto.settings, expectedSettings))
                    {
                        matchingPapers++;
                    }
                }

                // Accuracy: percentage of papers matching the order
                accuracyScore = (float)matchingPapers / totalPapers;

                // Material quality: based on print quality setting
                materialQualityScore = GetQualityScore(expectedSettings.quality);
            }
            else
            {
                // No papers in envelope = 0 score
                accuracyScore = 0f;
                materialQualityScore = 0f;
            }
        }
        else
        {
            // No envelope = 0 score
            accuracyScore = 0f;
            materialQualityScore = 0f;
        }

        var result = new OrderResult
        {
            OrderId = currentOrder.OrderId,
            CompletedSuccessfully = accuracyScore > 0f,
            AccuracyScore = accuracyScore,
            MaterialQualityScore = materialQualityScore,
            CompletedAt = Time.time
        };

        Debug.Log($"ServiceTable: Completed order {currentOrder.OrderId} - Accuracy: {accuracyScore:P0}, Quality: {materialQualityScore:P0}");

        onServiceCompleteCallback?.Invoke(result);
        Release();
    }

    private bool ComparePrintSettings(PrintSettings actual, PrintSettings expected)
    {
        return actual.paperSize == expected.paperSize
            && actual.paperOrientation == expected.paperOrientation
            && actual.paperFit == expected.paperFit
            && actual.isColored == expected.isColored
            && actual.quality == expected.quality;
    }

    private float GetQualityScore(PrintQuality quality)
    {
        return quality switch
        {
            PrintQuality.Low => 0.5f,
            PrintQuality.Average => 0.65f,
            PrintQuality.High => 0.8f,
            PrintQuality.UltraHigh => 1f,
            _ => 0.5f
        };
    }
}
