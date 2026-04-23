using UnityEngine;

public class OrderGenerator : MonoBehaviour, IOrderGenerator
{
    [SerializeField] private QuestHUD questHUD;

    public OrderData GenerateOrder()
    {
        var orderTypePicker = Random.value;

        OrderData orderData = null;

        switch (orderTypePicker)
        {
            case < 0.4f:
                orderData = OrderData.CreatePhotoOrder(PickRandomPhotoVariant());
                break;
            case < 0.7f:
                orderData = OrderData.CreateUsbExtractionOrder();
                break;
            default:
                orderData = OrderData.CreatePhotoCopyOrder();
                break;
        }

        questHUD.SetQuestDisplay(orderData);
        Debug.Log($"OrderGenerator: Generated order {orderData.OrderType} with quality {orderData.RequestedQuality}");
        return orderData;
    }

    private PhotoOrderVariant PickRandomPhotoVariant()
    {
        var variants = (PhotoOrderVariant[])System.Enum.GetValues(typeof(PhotoOrderVariant));
        return variants[Random.Range(0, variants.Length)];
    }
}
