using UnityEngine;

public class OrderGenerator : MonoBehaviour
{
    public OrderData GenerateOrder()
    {
        var orderTypePicker = Random.value;

        OrderData orderData;
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

        return orderData;
    }

    private PhotoOrderVariant PickRandomPhotoVariant()
    {
        var variants = (PhotoOrderVariant[])System.Enum.GetValues(typeof(PhotoOrderVariant));
        return variants[Random.Range(0, variants.Length)];
    }
}
