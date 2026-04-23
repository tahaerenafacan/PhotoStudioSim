using UnityEngine;
using UnityEngine.Localization;

public class EnvelopeStack : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString interactHint;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject envelopePrefab;

    public LocalizedString InteractHint => interactHint;

    public bool CanInteract => true;

    public void Interact()
    {
        Instantiate(envelopePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
