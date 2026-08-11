using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[Serializable]
public struct RebindActionEntry
{
    [Tooltip("Inspector'dan .inputactions asset'i içinden ilgili action'ı sürükle")]
    public InputActionReference actionReference;

    [Tooltip("Composite değilse 0 bırak")]
    public int bindingIndex;

    [Tooltip("Bu action için sahnede oluşturulacak/atanacak UI satırı")]
    public InputRebindRow row;

    public string ActionName => actionReference != null ? actionReference.action.name : string.Empty;
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action OnDropKeyPressed;
    public event Action OnInteractKeyPressed;
    public event Action OnPickupKeyPressed;
    public event Action<int> OnQuickSlotKeyPressed;
    public event Action<string> OnRebindStarted;
    public event Action<string> OnRebindCompleted;

    private PlayerInputActions actions;
    private InputActionRebindingExtensions.RebindingOperation activeRebindOperation;

    //Sprint
    private bool localSprint = false;
    private float sprintTimer = 0f;
    private const float sprintDelay = 0.15f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        actions = new PlayerInputActions();
        actions.Enable();

        SettingsManager.OnSettingsLoaded += HandleSettingsLoaded;
        if (SettingsManager.Instance != null && SettingsManager.Instance.CurrentSettings != null)
            ApplySavedBindingOverrides();
        
        BindInputEvents();
    }

    private void HandleSettingsLoaded(Settings obj)
    {
        ApplySavedBindingOverrides();
    }

    private void OnDestroy()
    {
        UnbindInputEvents();
        SettingsManager.OnSettingsLoaded -= HandleSettingsLoaded;
    }

    private void BindInputEvents()
    {
        actions.Player.Pause.performed    += Pause_Performed;
        actions.Player.Drop.performed     += Drop_Performed;
        actions.Interactions.Pickup.performed += Pickup_Performed;
        actions.Interactions.Interact.performed += Interact_Performed;
        actions.Player.QuickSlot.performed += QuickSlot_Performed;
    }

    private void UnbindInputEvents()
    {
        actions.Player.Pause.performed    -= Pause_Performed;
        actions.Player.Drop.performed     -= Drop_Performed;
        actions.Interactions.Pickup.performed -= Pickup_Performed;
        actions.Interactions.Interact.performed -= Interact_Performed;
        actions.Player.QuickSlot.performed -= QuickSlot_Performed;
    }

    private void Pickup_Performed(InputAction.CallbackContext context)
    {
        OnPickupKeyPressed?.Invoke();
    }

    private void QuickSlot_Performed(InputAction.CallbackContext context)
    {
        int slotIndex = int.Parse(context.control.name) - 1;
        OnQuickSlotKeyPressed?.Invoke(slotIndex);
    }

    private void Interact_Performed(InputAction.CallbackContext ctx)
    {
        OnInteractKeyPressed?.Invoke();
    }

    private void Drop_Performed(InputAction.CallbackContext ctx)
    {
        OnDropKeyPressed?.Invoke();
    }

    private void Pause_Performed(InputAction.CallbackContext context)
    {
        GameManager.Instance.TogglePauseGame();
    }

    private void ApplySavedBindingOverrides()
    {
        if (SettingsManager.Instance == null || SettingsManager.Instance.CurrentSettings == null) return;
        string overridesJson = SettingsManager.Instance.CurrentSettings.bindingOverridesJson;
        if (string.IsNullOrWhiteSpace(overridesJson)) return;

        try
        {
            actions.asset.LoadBindingOverridesFromJson(overridesJson);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load input binding overrides: {e.Message}");
        }
    }

    public bool StartInteractiveRebind(string actionName, int bindingIndex, Action<bool> onComplete = null)
    {
        if (actions == null || string.IsNullOrEmpty(actionName))
        {
            onComplete?.Invoke(false);
            return false;
        }

        var action = actions.asset.FindAction(actionName, true);
        if (action == null)
        {
            onComplete?.Invoke(false);
            return false;
        }

        CancelRebind();
        
        bool wasEnabled = action.enabled;
        if (wasEnabled) action.Disable();

        activeRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                activeRebindOperation = null;
                if (wasEnabled) action.Enable();
                SaveBindingOverrides();
                OnRebindCompleted?.Invoke(actionName);
                onComplete?.Invoke(true);
            })
            .OnCancel(op =>
            {
                op.Dispose();
                activeRebindOperation = null;
                if (wasEnabled) action.Enable();
                OnRebindCompleted?.Invoke(actionName);
                onComplete?.Invoke(false);
            });

        OnRebindStarted?.Invoke(actionName);
        activeRebindOperation.Start();
        return true;
    }

    public void CancelRebind()
    {
        if (activeRebindOperation == null) return;
        activeRebindOperation.Cancel();
        activeRebindOperation.Dispose();
        activeRebindOperation = null;
    }

    private void SaveBindingOverrides()
    {
        if (SettingsManager.Instance == null || SettingsManager.Instance.CurrentSettings == null) return;

        string overridesJson = actions.asset.SaveBindingOverridesAsJson();
        Settings settingsCopy = SettingsManager.Instance.CurrentSettings.Clone();
        settingsCopy.bindingOverridesJson = overridesJson;
        SettingsManager.Instance.UpdateSetting(settingsCopy, true);
    }

    public string GetPrimaryBindingEffectivePath(string actionName)
    {
        var action = actions.asset.FindAction(actionName, true);
        if (action == null) return string.Empty;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                return action.bindings[i].effectivePath;
        }
        return string.Empty;
    }

    public string GetBindingDisplayString(string actionName, int bindingIndex)
    {
        if (string.IsNullOrEmpty(actionName)) return string.Empty;
        var action = actions.asset.FindAction(actionName, true);
        if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) return string.Empty;
        return action.bindings[bindingIndex].ToDisplayString();
    }
    
    /// <summary>
    /// Icon lookup için kullanılır. Layout'a göre değişen display string'in aksine
    /// effectivePath (örn. "<Keyboard/>/e") sabittir, bu yüzden InputIconResolver
    /// bu değeri anahtar olarak kullanır.
    /// </summary>
    public string GetBindingEffectivePath(string actionName, int bindingIndex)
    {
        if (string.IsNullOrEmpty(actionName)) return string.Empty;
        var action = actions.asset.FindAction(actionName, true);
        if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) return string.Empty;
        return action.bindings[bindingIndex].effectivePath;
    }

    public string GetPrimaryBindingDisplayString(string actionName)
    {
        var action = actions.asset.FindAction(actionName, true);
        if (action == null) return string.Empty;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                return action.bindings[i].ToDisplayString();
        }
        return string.Empty;
    }

    private void Update()
    {
        SprintTimer();
    }

    private void SprintTimer()
    {
        if (actions.Player.Sprint.IsInProgress())
        {
            sprintTimer += Time.deltaTime;
            if (sprintTimer >= sprintDelay)
            {
                localSprint = true;
            }
        }
        else
        {
            sprintTimer = 0f;
            localSprint = false;
        }
    }

    public static void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !locked;
    }

    public static void ToggleCursorLock()
    {
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;
    }

    public Vector2 GetMovementInput() => actions.Player.Move.ReadValue<Vector2>();

    public Vector2 GetMouseDelta() => actions.Player.Look.ReadValue<Vector2>();

    public bool GetJumpInput() => actions.Player.Jump.WasPressedThisFrame();

    public bool GetCrouchInput() => actions.Player.Crouch.WasPressedThisFrame();

    public bool GetSprintInput() => localSprint;

    public bool GetUseInputDown()
        => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
}
