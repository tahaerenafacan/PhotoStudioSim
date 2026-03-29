using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class ScreenConfinement : MonoBehaviour
{
    [SerializeField] private RectTransform monitorPanel;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private RectTransform virtualCursor;
    [SerializeField] private InputSystemUIInputModule inputModule;
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float blendDuration = 0.5f;

    private Mouse virtualMouse;
    private Mouse physicalMouse;
    private bool isActive;
    private float sessionStartTime;
    private bool hasSessionHistory;

    private Vector2 localPos;
    private Rect panelRect;

    private Plane monitorPlane;

    private void Start()
    {
        ComputerState.Instance.OnPlayerSit += OnSessionStart;
        ComputerState.Instance.OnPlayerOut += OnSessionEnd;
        ComputerState.Instance.OnPowerOn += OnPowerOn;
        ComputerState.Instance.OnPowerOut += OnPowerOut;
        monitorPanel.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ComputerState.Instance.OnPlayerSit -= OnSessionStart;
        ComputerState.Instance.OnPlayerOut -= OnSessionEnd;
        ComputerState.Instance.OnPowerOn -= OnPowerOn;
        ComputerState.Instance.OnPowerOut -= OnPowerOut;
    }

    private void OnPowerOn()
    {
        monitorPanel.gameObject.SetActive(true);
    }

    private void OnPowerOut()
    {
        monitorPanel.gameObject.SetActive(false);
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


    private void Update()
    {
        if (!ComputerState.Instance.IsPlayerSit) return;

        if (!isActive)
        {
            if (Time.unscaledTime - sessionStartTime < blendDuration) return;
            Activate();
            if (!isActive) return;
        }

        UpdateVirtualMouse();
    }
    private void Activate()
    {
        if (!monitorPanel || !renderCamera) return;

        // Sınırlar: panelin kendi Rect'i — projeksiyon hatası yok
        panelRect = monitorPanel.rect;
        monitorPlane = new Plane(monitorPanel.forward, monitorPanel.position);

        // localPos önceki oturumdan korunur — ilk açılışta merkeze al
        if (!hasSessionHistory)
        {
            localPos = Vector2.zero;
            hasSessionHistory = true;
        }

        physicalMouse = Mouse.current;  // virtual device kurulmadan önce fiziksel mouse
        virtualMouse = InputSystem.AddDevice<Mouse>("VirtualMouse");

        if (inputModule == null)
            inputModule = FindAnyObjectByType<InputSystemUIInputModule>();

        if (inputModule != null)
        {
            //inputModule.cursor = virtualCursor;
            inputModule.pointerBehavior = UIPointerBehavior.SingleUnifiedPointer;
        }

        PushPositionToDevice();

        if (virtualCursor)
        {
            virtualCursor.gameObject.SetActive(true);
            SyncCursorGraphic();
        }

        isActive = true;
    }
    private void UpdateVirtualMouse()
    {
        // Delta'yı panel local space'e çevir:
        // ekrandaki piksel hareketi → dünya → panel local
        Vector2 rawDelta = physicalMouse.delta.ReadValue() * sensitivity;
        Vector2 localDelta = ScreenDeltaToLocalDelta(rawDelta);

        localPos.x = Mathf.Clamp(localPos.x + localDelta.x, panelRect.xMin, panelRect.xMax);
        localPos.y = Mathf.Clamp(localPos.y + localDelta.y, panelRect.yMin, panelRect.yMax);

        PushPositionToDevice();
        ForwardButtonEvents();
        SyncCursorGraphic();

        if (physicalMouse.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPos = monitorPanel.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
            Vector2 screenPos = renderCamera.WorldToScreenPoint(worldPos);
        }
    }


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
        Vector3 worldPos = monitorPanel.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
        virtualCursor.position = worldPos;
    }

    private void PushToDevice()
    {
        if (virtualMouse == null) return;

        Vector3 worldPos = monitorPanel.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
        Vector2 screenPos = renderCamera.WorldToScreenPoint(worldPos);

        InputState.Change(virtualMouse, new MouseState
        {
            position = screenPos,
            buttons = (ushort)((Mouse.current.leftButton.isPressed ? 1 << 0 : 0) |
                        (Mouse.current.rightButton.isPressed ? 1 << 1 : 0) |
                        (Mouse.current.middleButton.isPressed ? 1 << 2 : 0))
        });
    }

    private void PushPositionToDevice()
    {
        if (virtualMouse == null) return;

        Vector3 worldPos = monitorPanel.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
        Vector2 screenPos = renderCamera.WorldToScreenPoint(worldPos);

        InputState.Change(virtualMouse, new MouseState { position = screenPos });
    }

    private void ForwardButtonEvents()
    {
        if (virtualMouse == null) return;

        var p = physicalMouse;

        if (p.leftButton.wasPressedThisFrame) QueueButton(MouseButton.Left, true);
        if (p.leftButton.wasReleasedThisFrame) QueueButton(MouseButton.Left, false);
        if (p.rightButton.wasPressedThisFrame) QueueButton(MouseButton.Right, true);
        if (p.rightButton.wasReleasedThisFrame) QueueButton(MouseButton.Right, false);
    }

    private void QueueButton(MouseButton button, bool pressed)
    {
        var state = new MouseState { position = virtualMouse.position.ReadValue() };
        state.WithButton(button, pressed);
        InputSystem.QueueStateEvent(virtualMouse, state);
    }


    private void RemoveVirtualMouse()
    {
        if (virtualMouse != null)
        {
            InputSystem.RemoveDevice(virtualMouse);
            virtualMouse = null;
        }
    }

    /// <summary>Cursoru panel merkezine taşır.</summary>
    private void CenterCursor()
    {
        localPos = Vector2.zero;
        if (isActive)
        {
            SyncCursorGraphic();
            PushToDevice();
        }
    }

    private void OnDestroy() => RemoveVirtualMouse();
}