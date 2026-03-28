using UnityEngine;

public class ItemPaperPrinter : MonoBehaviour, IInteractable
{
    public string InteractHint => "Interact to Print";

    public void Interact()
    {
        Debug.Log("Interacted with printer.");
    }
}
