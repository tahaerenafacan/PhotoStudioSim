using System;
using UniStorm;
using UnityEngine;
using UnityEngine.Localization;

public class Bed : MonoBehaviour, IInteractable
{
    public event Action OnSleep;
    [SerializeField] private LocalizedString interactHint;
    [SerializeField] private LocalizedString itemName;
    
    public LocalizedString InteractHint => interactHint;
    public LocalizedString InteractName => itemName;
    public bool CanInteract => UniStormSystem.Instance.TimeFlow == UniStormSystem.EnableFeature.Disabled; 
    public void Interact()
    {
        OnSleep?.Invoke();
    }
}
