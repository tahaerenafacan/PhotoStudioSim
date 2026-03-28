using UnityEngine;

public class ItemSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.1f;
    [SerializeField] private float maxSwayAmount = 1f; 
    [SerializeField] private float swaySmoothness = 5f; 

    [Header("Movement Bobbing")]
    [SerializeField] private float movementSwayMagnitudeLerpSpeed = 10f;
    [SerializeField] private float movementSwayFrequencyLerpSpeed = 10f;
    [Space]
    [SerializeField] private Vector2 walkMovementSwayMagnitude = new Vector2(1f, 1f); 
    [SerializeField] private Vector2 walkMovementSwayFrequency = new Vector2(10f, 10f); 
    [Space]
    [SerializeField] private Vector2 sprintMovementSwayMagnitude = new Vector2(1f, 1f); 
    [SerializeField] private Vector2 sprintMovementSwayFrequency = new Vector2(10f, 10f); 
    
    private float currentMovementSwayFrequencyY;
    private float currentMovementSwayFrequencyX;
    private float currentMovementSwayMagnitudeY;
    private float currentMovementSwayMagnitudeX;
    
    private float positionTimerX = 0f;
    private float positionTimerY = 0f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        currentMovementSwayFrequencyX = walkMovementSwayFrequency.x;
        currentMovementSwayMagnitudeX = walkMovementSwayMagnitude.x;
        currentMovementSwayFrequencyY = walkMovementSwayFrequency.y;
        currentMovementSwayMagnitudeY = walkMovementSwayMagnitude.y;
    }

    private void Update()
    {
        if (InputManager.Instance == null || PlayerState.Instance == null || !PlayerItemHolder.Instance.IsHoldingItem) return;

        bool isSprinting = PlayerState.Instance.IsSprinting;
        bool isCrouching = PlayerState.Instance.IsCrouching;
        bool isMoving = PlayerState.Instance.IsMoving;

        UpdateSway(isSprinting, isCrouching, isMoving);
    }

    public void UpdateSway(bool isSprinting, bool isCrouching, bool isMoving)
    {
        float deltaTime = Time.deltaTime;

        Vector2 lookInput = InputManager.Instance.GetMouseDelta();
        float swayX = Mathf.Clamp(lookInput.x * swayAmount, -maxSwayAmount, maxSwayAmount);
        float swayY = Mathf.Clamp(lookInput.y * swayAmount, -maxSwayAmount, maxSwayAmount);
        
        Quaternion targetRotation = Quaternion.Euler(new Vector3(-swayY * 10f, swayX * 10f, swayX * -5f));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * targetRotation, deltaTime * swaySmoothness);
        
        if (isMoving && !isCrouching)
        {
            float targetMovementSwayMagnitudeX = isSprinting ? sprintMovementSwayMagnitude.x : walkMovementSwayMagnitude.x;
            float targetMovementSwayFrequencyX = isSprinting ? sprintMovementSwayFrequency.x : walkMovementSwayFrequency.x;
            float targetMovementSwayMagnitudeY = isSprinting ? sprintMovementSwayMagnitude.y : walkMovementSwayMagnitude.y;
            float targetMovementSwayFrequencyY = isSprinting ? sprintMovementSwayFrequency.y : walkMovementSwayFrequency.y;
            
            currentMovementSwayMagnitudeX = Mathf.Lerp(currentMovementSwayMagnitudeX, targetMovementSwayMagnitudeX, deltaTime * movementSwayMagnitudeLerpSpeed);
            currentMovementSwayFrequencyX = Mathf.Lerp(currentMovementSwayFrequencyX, targetMovementSwayFrequencyX, deltaTime * movementSwayFrequencyLerpSpeed);
            currentMovementSwayMagnitudeY = Mathf.Lerp(currentMovementSwayMagnitudeY, targetMovementSwayMagnitudeY, deltaTime * movementSwayMagnitudeLerpSpeed);
            currentMovementSwayFrequencyY = Mathf.Lerp(currentMovementSwayFrequencyY, targetMovementSwayFrequencyY, deltaTime * movementSwayFrequencyLerpSpeed);
            
            positionTimerY += deltaTime * currentMovementSwayFrequencyY;
            positionTimerX += deltaTime * currentMovementSwayFrequencyX;
        
            transform.localPosition = new Vector3(
                initialPosition.x + Mathf.Sin(positionTimerX) * currentMovementSwayMagnitudeX, 
                initialPosition.y + Mathf.Sin(positionTimerY) * currentMovementSwayMagnitudeY, 
                transform.localPosition.z);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, 2f * deltaTime);
            positionTimerY = 0f;
            positionTimerX = 0f;
        }
    }
}