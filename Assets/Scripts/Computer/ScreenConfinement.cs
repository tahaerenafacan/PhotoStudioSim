using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(ScreenConfinement))]
public class ScreenConfinement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform monitorPanel;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private RectTransform virtualCursor;
    [SerializeField] private InputSystemUIInputModule inputModule;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float blendDuration = 0.5f;

    // State Variables
    private Mouse virtualMouse;
    private Mouse physicalMouse;
    private bool isActive;
    private bool hasSessionHistory;
    private float sessionStartTime;
    
    private Vector2 localPos;
    private Rect panelRect;
    private Plane monitorPlane;

    #region Unity Lifecycle
    
    private void Awake()
    {
        if (inputModule == null)
        {
            inputModule = FindAnyObjectByType<InputSystemUIInputModule>();
        }
        if (inputModule == null) Debug.LogError($"{nameof(ScreenConfinement)}: No InputSystemUIInputModule found!");
    }

    private void Start()
    {
        SubscribeToComputerEvents();
        SetMonitorState(false);
    }

    private void Update()
    {
        if (!ComputerState.Instance.IsPlayerSit) return;

        HandleSessionActivation();

        if (isActive)
        {
            ProcessVirtualMouseInput();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromComputerEvents();
    }

    private void OnDestroy()
    {
        RemoveVirtualMouse();
    }

    #endregion

    #region Event Subscriptions

    private void SubscribeToComputerEvents()
    {
        if (ComputerState.Instance == null) return;

        ComputerState.Instance.OnPlayerSit += OnSessionStart;
        ComputerState.Instance.OnPlayerOut += OnSessionEnd;
        ComputerState.Instance.OnPowerOn += OnPowerOn;
        ComputerState.Instance.OnPowerOut += OnPowerOut;
    }

    private void UnsubscribeFromComputerEvents()
    {
        if (ComputerState.Instance == null) return;

        ComputerState.Instance.OnPlayerSit -= OnSessionStart;
        ComputerState.Instance.OnPlayerOut -= OnSessionEnd;
        ComputerState.Instance.OnPowerOn -= OnPowerOn;
        ComputerState.Instance.OnPowerOut -= OnPowerOut;
    }

    #endregion

    #region Computer State Handlers

    private void OnPowerOn() => SetMonitorState(true);
    private void OnPowerOut() => SetMonitorState(false);

    private void SetMonitorState(bool state)
    {
        if (monitorPanel != null)
        {
            monitorPanel.gameObject.SetActive(state);
        }
    }

    private void OnSessionStart()
    {
        sessionStartTime = Time.unscaledTime;
        isActive = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnSessionEnd()
    {
        isActive = false;
        RemoveVirtualMouse();
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion

    #region Core Logic

    private void HandleSessionActivation()
    {
        if (isActive || Time.unscaledTime - sessionStartTime < blendDuration) return;
        
        ActivateDevice();
    }

    private void ActivateDevice()
    {
        if (!monitorPanel || !renderCamera) return;

        InitializeMonitorBounds();
        InitializeVirtualMouse();
        
        isActive = true;
    }

    private void InitializeMonitorBounds()
    {
        panelRect = monitorPanel.rect;
        monitorPlane = new Plane(monitorPanel.forward, monitorPanel.position);

        if (!hasSessionHistory)
        {
            localPos = Vector2.zero;
            hasSessionHistory = true;
        }
    }

    private void InitializeVirtualMouse()
    {
        physicalMouse = Mouse.current;
        if (physicalMouse == null) return;

        virtualMouse = InputSystem.AddDevice<Mouse>("VirtualMouse");

        inputModule.pointerBehavior = UIPointerBehavior.AllPointersAsIs;

        UpdateDeviceState();

        if (virtualCursor)
        {
            virtualCursor.gameObject.SetActive(true);
            SyncCursorGraphic();
        }
    }

    private void ProcessVirtualMouseInput()
    {
        if (physicalMouse == null || virtualMouse == null) return;

        // 1. Pozisyonu Hesapla
        CalculateNewLocalPosition();

        // 2. Cihazın tüm durumunu (Pozisyon + Basılı Tutulan Butonlar) Güncelle
        UpdateDeviceState();
        
        // 3. Grafiksel imleci senkronize et
        SyncCursorGraphic();
    }

    private void CalculateNewLocalPosition()
    {
        Vector2 rawDelta = physicalMouse.delta.ReadValue() * sensitivity;
        Vector2 localDelta = ScreenDeltaToLocalDelta(rawDelta);

        localPos.x = Mathf.Clamp(localPos.x + localDelta.x, panelRect.xMin, panelRect.xMax);
        localPos.y = Mathf.Clamp(localPos.y + localDelta.y, panelRect.yMin, panelRect.yMax);
    }

    #endregion

    #region Input Mapping & Mathematics

    private Vector2 ScreenDeltaToLocalDelta(Vector2 screenDelta)
    {
        Vector2 screenCenter = renderCamera.WorldToScreenPoint(monitorPanel.position);
        Vector2 screenOffset = screenCenter + screenDelta;

        LocalFromScreen(screenCenter, out Vector2 localCenter);
        LocalFromScreen(screenOffset, out Vector2 localOffset);

        return localOffset - localCenter;
    }

    private bool LocalFromScreen(Vector2 screenPos, out Vector2 localResult)
    {
        Ray ray = renderCamera.ScreenPointToRay(screenPos);
        if (monitorPlane.Raycast(ray, out float dist))
        {
            Vector3 worldHit = ray.GetPoint(dist);
            localResult = monitorPanel.InverseTransformPoint(worldHit);
            return true;
        }
        
        localResult = Vector2.zero;
        return false;
    }

    private void SyncCursorGraphic()
    {
        if (!virtualCursor) return;
        virtualCursor.position = GetWorldPosition();
    }

    private void UpdateDeviceState()
    {
        if (virtualMouse == null) return;

        Vector2 screenPos = renderCamera.WorldToScreenPoint(GetWorldPosition());

        // Fiziksel farenin o anki tuş durumlarını alıyoruz (basılı tutuluyorsa 1, tutulmuyorsa 0)
        ushort buttonsState = 0;
        if (physicalMouse.leftButton.isPressed) buttonsState |= 1 << 0;
        if (physicalMouse.rightButton.isPressed) buttonsState |= 1 << 1;
        if (physicalMouse.middleButton.isPressed) buttonsState |= 1 << 2;

        // Hem yeni pozisyonu hem de tuşların basılı tutulma durumunu tek seferde gönderiyoruz.
        // Böylece sürükleme (dragging) işlemi kesintiye uğramaz.
        InputState.Change(virtualMouse, new MouseState 
        { 
            position = screenPos,
            buttons = buttonsState
        });
    }

    #endregion

    #region Utility

    private Vector3 GetWorldPosition()
    {
        return monitorPanel.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
    }

    private void RemoveVirtualMouse()
    {
        if (virtualMouse != null)
        {
            InputSystem.RemoveDevice(virtualMouse);
            virtualMouse = null;
        }
    }

    public void CenterCursor()
    {
        localPos = Vector2.zero;
        if (isActive)
        {
            SyncCursorGraphic();
            UpdateDeviceState();
        }
    }

    #endregion
}