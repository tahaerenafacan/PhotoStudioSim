
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIMouseSway : MonoBehaviour
{
    [SerializeField] private float inputForce = 0.15f;      
    [SerializeField] private float springStiffness = 120f;  
    [SerializeField] private float springDamping = 14f;    

    private Vector2 _velocity; 

    private RectTransform _rect;
    private Vector2 _basePosition;
    private Vector2 _currentOffset;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _basePosition = _rect.anchoredPosition;
    }

    private void Update()
    {
        if (InputManager.Instance == null) return;

        Vector2 delta = InputManager.Instance.GetMouseDelta();
        _velocity += -delta * inputForce;

        _velocity += (-_currentOffset * springStiffness - _velocity * springDamping) * Time.deltaTime;
        _currentOffset += _velocity * Time.deltaTime;

        _rect.anchoredPosition = _basePosition + _currentOffset;
    }
}