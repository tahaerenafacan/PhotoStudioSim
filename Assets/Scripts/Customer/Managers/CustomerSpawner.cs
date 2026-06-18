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
    [SerializeField] private float spawnInterval = 5f;

    public event System.Action<CustomerController> OnCustomerSpawned;

    private float spawnTimer;

    public void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnCustomer();
            spawnTimer = 0f;
        }
    }

    public CustomerController SpawnCustomer()
    {
        if (customerPrefab == null || spawnPoint == null || entranceTarget == null || exitTarget == null)
        {
            Debug.LogError("CustomerSpawner is missing required references.", this);
            return null;
        }

        CustomerController instance = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
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
}
