using UnityEngine;
using UnityEngine.Localization;

public class LedSign : MonoBehaviour, IInteractable
{
    [SerializeField] private LedSignController ledSignController;
    
    [SerializeField] private LocalizedString interactHint;
    [SerializeField] private LocalizedString interactName;
    public LocalizedString InteractHint => interactHint;
    public LocalizedString InteractName => interactName;
    public bool CanInteract => true;
    
    public void Interact()
    {
        ledSignController.ToggleOpen();
    }
}
