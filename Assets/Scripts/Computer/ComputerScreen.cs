using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace SyntaxSultan.ComputerSystem
{
    public class ComputerScreen : MonoBehaviour, IInteractable, IComplexUsable
    {
        [Header("References")]
        [SerializeField] private Computer computer;
        [SerializeField] private CinemachineCamera seatCamera;
    
        [Header("Input (New Input System)")]
        [SerializeField] private InputActionReference exitAction;
        [SerializeField] private LocalizedString exitHint;
    
        [Header("Localization")]
        [SerializeField] private LocalizedString interactHint;
        [SerializeField] private LocalizedString interactName;
    
        public LocalizedString InteractHint => interactHint;
        public LocalizedString InteractName => interactName;
        public bool CanInteract => computer != null && !computer.IsPlayerSitting;
        
        private List<ItemInteraction> interactions;
        
        protected void Awake()
        {
            InitializeInteractions();
        }
    
        private void InitializeInteractions()
        {
            interactions = new List<ItemInteraction>();

            var shootInteract = new ItemInteraction(exitAction, exitHint);
            shootInteract.OnPerformed += ctx => StandUp();
            interactions.Add(shootInteract);
        }

        private void OnEnable()
        {
            if (exitAction != null)
            {
                //exitAction.action.performed += HandleStandUpInput;
            }
        }

        private void OnDisable()
        {
            if (exitAction != null)
            {
                //exitAction.action.performed -= HandleStandUpInput;
            }
        }

        public void Interact()
        {
            if (computer.IsPlayerSitting) return;
            SitDown();
        }

        private void SitDown()
        {
            PlayerItemHolder.Instance.BindExternalInteraction(this);
            
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
            PlayerItemHolder.Instance.UnbindExternalInteraction();
            
            computer.SetPlayerSitting(false);
            seatCamera.Priority = -1;
            
            InputManager.SetCursorLock(true);
            PlayerInteraction.Instance.EnableInteraction();
        
            if (exitAction != null) exitAction.action.Disable();
        }

        public List<ItemInteraction> GetInteractions()
        {
            return interactions;
        }
    }
}