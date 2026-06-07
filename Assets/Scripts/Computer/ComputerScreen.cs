using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace SyntaxSultan.ComputerSystem
{
    public class ComputerScreen : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private Computer computer;
        [SerializeField] private CinemachineCamera seatCamera;
    
        [Header("Input (New Input System)")]
        [SerializeField] private InputActionReference exitAction;
    
        [Header("Localization")]
        [SerializeField] private LocalizedString interactHint;
    
        public LocalizedString InteractHint => interactHint;
        public bool CanInteract => computer != null && !computer.IsPlayerSitting;

        private void OnEnable()
        {
            if (exitAction != null)
            {
                exitAction.action.performed += HandleStandUpInput;
            }
        }

        private void OnDisable()
        {
            if (exitAction != null)
            {
                exitAction.action.performed -= HandleStandUpInput;
            }
        }

        public void Interact()
        {
            if (computer.IsPlayerSitting) return;
            SitDown();
        }

        private void SitDown()
        {
            computer.SetPlayerSitting(true);
            seatCamera.Priority = 2;
        
            // Karakter kontrolcünü ve Interaction sistemini dışarıdan bir event ile kapatmak daha SOLID'dir
            InputManager.SetCursorLock(false);
            PlayerInteraction.Instance.DisableInteraction();
        
            if (exitAction != null) exitAction.action.Enable();
        }

        private void HandleStandUpInput(InputAction.CallbackContext context)
        {
            if (computer.IsPlayerSitting) StandUp();
        }

        private void StandUp()
        {
            computer.SetPlayerSitting(false);
            seatCamera.Priority = -1;
            
            InputManager.SetCursorLock(true);
            PlayerInteraction.Instance.EnableInteraction();
        
            if (exitAction != null) exitAction.action.Disable();
        }
    }
}