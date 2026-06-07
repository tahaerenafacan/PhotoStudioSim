using UnityEngine;
using UnityEngine.Localization;

public class EnvelopeStack : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString interactHint;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] envelopeStackVisual;
    [SerializeField] private GameObject envelopePrefab;

    public LocalizedString InteractHint => interactHint;

    public bool CanInteract => true;

    private int currentStackSize = 20;

    public void Interact()
    {
        if (currentStackSize <= 0 || envelopeStackVisual == null || envelopeStackVisual.Length == 0) RefillStack();
        if (currentStackSize <= 0 || envelopeStackVisual == null || envelopeStackVisual.Length == 0) return;

        int index = envelopeStackVisual.Length - currentStackSize;
        if (index < 0 || index >= envelopeStackVisual.Length) return;

        envelopeStackVisual[index].SetActive(false);
        Instantiate(envelopePrefab, spawnPoint.position, spawnPoint.rotation);
        currentStackSize--;
    }

    public int RefillStack()
    {
        int added = 20 - currentStackSize;
        if (added <= 0) return 0;

        currentStackSize = 20;

        // Reactivate all visual envelopes
        if (envelopeStackVisual != null)
        {
            for (int i = 0; i < envelopeStackVisual.Length; i++)
            {
                envelopeStackVisual[i].SetActive(true);
            }
        }

        return added;
    }
}
