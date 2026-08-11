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

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 9f;
    [SerializeField] private float busyTablePenalty = 1.3f;

    public event System.Action<CustomerController> OnCustomerSpawned;

    private float spawnTimer;
    private float currentSpawnInterval;

    private void Awake()
    {
        currentSpawnInterval = Mathf.Clamp(spawnInterval, minSpawnInterval, maxSpawnInterval);
    }

    private void Update()
    {
        if (queueManager == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer < currentSpawnInterval)
            return;

        if (!queueManager.HasQueuePositions)
            return;

        SpawnCustomer();
        spawnTimer = 0f;
        currentSpawnInterval = CalculateSpawnInterval();
    }

    private float CalculateSpawnInterval()
    {
        if (queueManager == null)
            return Mathf.Clamp(spawnInterval, minSpawnInterval, maxSpawnInterval);

        float fillRate = queueManager.QueueFillRatio;
        float tableFactor = serviceTableManager != null && !serviceTableManager.HasAvailableTable ? busyTablePenalty : 1f;
        float interval = spawnInterval * (1f + fillRate) * tableFactor;

        return Mathf.Clamp(interval, minSpawnInterval, maxSpawnInterval);
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
