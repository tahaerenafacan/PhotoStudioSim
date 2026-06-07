using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SyntaxSultan.InventoryModule
{
    public class InventoryInputHandler : MonoBehaviour
    {
        public event Action<int> OnSlotKeyPressed;
        
        private static readonly Key[] NumberKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
            Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8
        };

        private int maxSlots;
        private float scrollTimer;

        public void Initialize(int slotCount) => SetMaxSlots(slotCount);

        /// <summary>UpgradeSlotCount sonrası InventorySystem event'i ile çağrılır.</summary>
        public void SetMaxSlots(int count) => maxSlots = Mathf.Clamp(count, 2, 8);

        private void Update()
        {
            HandleNumberKeys();
        }

        private void HandleNumberKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            for (int i = 0; i < maxSlots; i++)
            {
                if (keyboard[NumberKeys[i]].wasPressedThisFrame)
                {
                    OnSlotKeyPressed?.Invoke(i);
                    return;
                }
            }
        }
    }
}