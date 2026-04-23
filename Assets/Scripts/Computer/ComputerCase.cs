using UnityEngine;
using UnityEngine.Localization;

public class ComputerCase : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString interactHint;
    public LocalizedString InteractHint => interactHint;
    public bool CanInteract => true;

    public void Interact()
    {
        ComputerState.Instance.TogglePower();
    }
}