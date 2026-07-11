using System;

namespace SyntaxSultan.ShopSystem
{
    [Serializable]
    public class PendingOrder
    {
        public string OrderId { get; }
        public ShopItemDefinition Item { get; }
        public float RemainingSeconds { get; private set; }

        public bool IsReady => RemainingSeconds <= 0f;

        public PendingOrder(ShopItemDefinition item)
        {
            OrderId = Guid.NewGuid().ToString();
            Item = item;
            RemainingSeconds = item.deliveryTimeSeconds;
        }

        /// <summary>Her frame OrderDeliveryManager tarafından çağrılır.</summary>
        public void Tick(float deltaTime)
        {
            if (IsReady) return;
            RemainingSeconds = UnityEngine.Mathf.Max(0f, RemainingSeconds - deltaTime);
        }
    }
}