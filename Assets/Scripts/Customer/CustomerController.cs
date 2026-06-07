using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Localization;

public class CustomerController : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString interactHint;
    public LocalizedString InteractHint => interactHint;
    public bool CanInteract => PlayerItemHolder.Instance != null
        && PlayerItemHolder.Instance.IsHoldingItem
        && PlayerItemHolder.Instance.CurrentItem is ItemEnvelope
        && reservedServiceTable != null
        && !serviceCompleted;

    
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float destinationTolerance = 0.25f;
    [SerializeField] private Animator controller;

    private Transform entranceTarget;
    private Transform exitTarget;

    private QueueManager queueManager;
    private ServiceTableManager serviceTableManager;
    private OrderManager orderManager;
    private ShopRatingManager shopRatingManager;
    private OrderGenerator orderGenerator;

    private CustomerStateMachine stateMachine;
    private ServiceTable reservedServiceTable;
    private bool serviceCompleted;

    public CustomerData CustomerData { get; private set; }
    public QueueManager QueueManager => queueManager;
    public ServiceTableManager ServiceTableManager => serviceTableManager;
    public OrderManager OrderManager => orderManager;
    public ShopRatingManager RatingManager => shopRatingManager;
    public OrderGenerator OrderGenerator => orderGenerator;
    public Transform EntranceTarget => entranceTarget;
    public Transform ExitTarget => exitTarget;
    public bool HasReachedDestination => navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + destinationTolerance;
    public CustomerStateMachine StateMachine => stateMachine;

    public event Action<CustomerController> OnCustomerDespawning;
    public event Action<OrderData> OnOrderCreated;
    public event Action<CustomerController, OrderResult> OnServiceCompleted;
    public event Action<CustomerController, int> OnRatingSubmitted;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (navMeshAgent == null)
        {
            Debug.LogError("CustomerController requires a NavMeshAgent.", this);
        }
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    public void Initialize(
        Transform entranceTarget,
        Transform exitTarget,
        QueueManager queueManager,
        ServiceTableManager serviceTableManager,
        OrderManager orderManager,
        ShopRatingManager shopRatingManager,
        OrderGenerator orderGenerator)
    {
        this.entranceTarget = entranceTarget;
        this.exitTarget = exitTarget;
        this.queueManager = queueManager;
        this.serviceTableManager = serviceTableManager;
        this.orderManager = orderManager;
        this.shopRatingManager = shopRatingManager;
        this.orderGenerator = orderGenerator;

        CustomerData = new CustomerData();
        stateMachine = new CustomerStateMachine(this);
        stateMachine.SetState(new ApproachEntranceState(this));
    }

    public void SetDestination(Vector3 worldPosition)
    {
        if (navMeshAgent == null) return;
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("Customer NavMeshAgent is not on a valid NavMesh.", this);
            return;
        }

        navMeshAgent.isStopped = false;
        controller.SetBool("isWalking", true);
        navMeshAgent.SetDestination(worldPosition);
    }

    public void StopMovement()
    {
        if (navMeshAgent == null) return;
        controller.SetBool("isWalking", false);
        navMeshAgent.isStopped = true;
    }

    public void ReserveServiceTable(ServiceTable serviceTable)
    {
        reservedServiceTable = serviceTable;
        CustomerData.TableReservedAt = Time.time;
    }

    public ServiceTable GetReservedServiceTable() => reservedServiceTable;

    public void CreateOrder()
    {
        if (CustomerData.AssignedOrder != null) return;

        CustomerData.AssignedOrder = orderGenerator.GenerateOrder();
        CustomerData.OrderCreatedAt = Time.time;
        orderManager.RegisterOrder(CustomerData.AssignedOrder);
        OnOrderCreated?.Invoke(CustomerData.AssignedOrder);
    }

    public void NotifyServiceStarted()
    {
        CustomerData.ServiceStartedAt = Time.time;
    }

    public void NotifyServiceCompleted(OrderResult orderResult)
    {
        if (serviceCompleted)
        {
            return;
        }

        serviceCompleted = true;
        CustomerData.OrderResult = orderResult;
        CustomerData.ServiceCompletedAt = Time.time;
        orderManager?.CompleteOrder(orderResult);

        OnServiceCompleted?.Invoke(this, orderResult);

        if (shopRatingManager != null)
        {
            float actualWaitTime = CustomerData.ServiceCompletedAt - CustomerData.ServiceStartedAt;
                        
            var context = new ShopRatingContext
            {
                ShopStarLevel = shopRatingManager.CurrentShopStarLevel,
                WaitTime = actualWaitTime,
                OrderAccuracyScore = orderResult.AccuracyScore,
                MaterialQualityScore = orderResult.MaterialQualityScore
            };

            int rating = shopRatingManager.CalculateRating(context);
            OnRatingSubmitted?.Invoke(this, rating);
            Debug.Log($"CustomerController: Submitted rating {rating} for customer {name}", this);
        }

        PlayerItemHolder.Instance.ClearCurrentItem();
    }

    public void BeginExit()
    {
        stateMachine.SetState(new ExitShopState(this));
    }

    public void NotifyDespawning()
    {
        OnCustomerDespawning?.Invoke(this);
    }

    public void BeginDespawn()
    {
        stateMachine.SetState(new DespawnState(this));
    }

    public void Interact()
    {
        reservedServiceTable.CompleteService();
    }
}
