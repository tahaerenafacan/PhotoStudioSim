using UnityEngine;
using UnityEngine.InputSystem;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform entranceTarget;
    [SerializeField] private Transform exitTarget;
    [SerializeField] private QueueManager queueManager;
    [SerializeField] private ServiceTableManager serviceTableManager;
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private ShopRatingManager shopRatingManager;
    [SerializeField] private OrderGenerator orderGenerator;

    public event System.Action<CustomerController> OnCustomerSpawned;

    public CustomerController SpawnCustomer()
    {
        if (customerPrefab == null || spawnPoint == null || entranceTarget == null || exitTarget == null)
        {
            Debug.LogError("CustomerSpawner is missing required references.", this);
            return null;
        }

        var instance = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"CustomerSpawner: Spawned customer {instance.name} at {spawnPoint.position}", this);
        instance.Initialize(
            entranceTarget,
            exitTarget,
            queueManager,
            serviceTableManager,
            orderManager,
            shopRatingManager,
            orderGenerator);

        OnCustomerSpawned?.Invoke(instance);
        return instance;
    }

    public void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            SpawnCustomer();
        }
    }
}
