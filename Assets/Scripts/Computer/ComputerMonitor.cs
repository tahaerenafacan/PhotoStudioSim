using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class ComputerMonitor : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString interactHint;
    [SerializeField] private CinemachineCamera seatCamera;
    
    private void Start()
    {
        ComputerState.Instance.OnPlayerOut += Stand;
    }
    
    private void OnDestroy()
    {
        if (ComputerState.Instance != null)
            ComputerState.Instance.OnPlayerOut -= Stand;
    }

    private void Update()
    {
        if (ComputerState.Instance.IsPlayerSit && Keyboard.current.tabKey.wasPressedThisFrame)
            ComputerState.Instance.PlayerStand();
    }
    

    // ── IInteractable ────────────────────────────────────────────
    public LocalizedString InteractHint =>  interactHint;
    public bool CanInteract => !ComputerState.Instance.IsPlayerSit;

    public void Interact()
    {
        if (ComputerState.Instance.IsPlayerSit) return;
        Sit();
    }

    private void Sit()
    {
        seatCamera.Priority = 2;

        ComputerState.Instance.PlayerSit();
    }

    private void Stand()
    {
        seatCamera.Priority = -1;
    }
}