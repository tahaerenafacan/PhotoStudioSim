using System.Collections.Generic;
using SyntaxSultan.InventoryModule;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace SyntaxSultan.DirtSystem
{
    public class BroomItem : BasePickableItem, IComplexUsable, IStorable
    {
        [SerializeField] private InputActionReference broomAction;
        [SerializeField] private LocalizedString broomHint;
        
        [Header("Sweep Animation")]
        [SerializeField] private float sweepSpeed = 6f;
        [SerializeField] private float sweepDistance = 0.08f;
        [SerializeField] private float returnLerpSpeed = 8f;

        private List<ItemInteraction> interactions;
        private ICleanable currentCleanTarget;
        private float sweepTimer;
        private bool isBroomHeld;

        public bool CanStore => true;

        public Sprite Icon => ItemData.icon;

        private void HandleBroomStarted(InputAction.CallbackContext ctx) => isBroomHeld = true;
        private void HandleBroomCanceled(InputAction.CallbackContext ctx) => isBroomHeld = false;

        protected override void Awake()
        {
            base.Awake();
            
            interactions = new List<ItemInteraction>();

            var broomInteract = new ItemInteraction(broomAction, broomHint);
            broomInteract.OnStarted += HandleBroomStarted;
            broomInteract.OnCanceled += HandleBroomCanceled;
            interactions.Add(broomInteract);
        }
        private void OnDestroy()
        {
            if (broomAction != null && broomAction.action != null)
            {
                broomAction.action.started -= HandleBroomStarted;
                broomAction.action.canceled -= HandleBroomCanceled;
            }
        }

        private void Update()
        {
            if (isBroomHeld)
            {
                Clean();
            }
            else if (currentCleanTarget != null)
            {
                currentCleanTarget.ResetProgress();
                currentCleanTarget = null;
                ReturnToRestPose();
            }
        }

        private void Clean()
        {
            ICleanable lookedAt = PlayerInteraction.Instance.DetectedCleanable;

            if (lookedAt != null)
            {
                if (lookedAt != currentCleanTarget)
                {
                    currentCleanTarget?.ResetProgress();
                    currentCleanTarget = lookedAt;
                }

                currentCleanTarget.ApplyCleanProgress(Time.deltaTime);
                AnimateSweep();
            }
            else
            {
                currentCleanTarget?.ResetProgress();
                currentCleanTarget = null;
                ReturnToRestPose();
            }
        }
        
        private void AnimateSweep()
        {
            sweepTimer += Time.deltaTime * sweepSpeed;
            float offset = Mathf.Sin(sweepTimer) * sweepDistance;

            Vector3 basePos = ItemData.holdPositionOffset;
            transform.localPosition = basePos + Vector3.forward * offset;
        }
        
        private void ReturnToRestPose()
        {
            sweepTimer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                ItemData.holdPositionOffset,
                Time.deltaTime * returnLerpSpeed);
        }

        protected override void OnDropped()
        {
            currentCleanTarget?.ResetProgress();
            currentCleanTarget = null;
            isBroomHeld = false;
        }

        public List<ItemInteraction> GetInteractions()
        {
            return interactions;
        }
    }
}