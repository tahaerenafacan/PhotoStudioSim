using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString interactHint;
    [SerializeField] private LocalizedString interactName;
    public LocalizedString InteractHint => interactHint;
    public LocalizedString InteractName => interactName;
    public bool CanInteract => true;
    
    [SerializeField] private float openTime = 1f;
    [SerializeField] private Vector3 openedRotation;
    [SerializeField] private Vector3 closedRotation;
    
    private bool isOpen = false;
    private Sequence sequence;
    
    public void Interact()
    {
        sequence?.Kill();
        sequence = DOTween.Sequence();

        sequence.Append(isOpen
            ? transform.DOLocalRotate(closedRotation, openTime)
            : transform.DOLocalRotate(openedRotation, openTime));

        isOpen = !isOpen;
    }
}
