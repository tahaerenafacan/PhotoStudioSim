using UnityEngine;
using UnityEngine.Localization;

namespace SyntaxSultan.ComputerSystem
{
    public class ComputerCase : MonoBehaviour, IInteractable
    {
        [SerializeField] private Computer computer;
        [SerializeField] private LocalizedString interactHint;
        public LocalizedString InteractHint => interactHint;
        public bool CanInteract => computer != null;

        public void Interact()
        {
            computer.TogglePower();
        }
    }
}