using System;
using UnityEngine;

namespace SyntaxSultan.ComputerSystem
{
    public class Computer : MonoBehaviour
    {
        public bool IsPoweredOn { get; private set; }
        public bool IsPlayerSitting { get; private set; }

        public event Action OnBootStart;
        public event Action OnBootComplete;
        public event Action OnShutdownStart;
        public event Action OnShutdownComplete;
    
        public event Action OnPlayerSatDown;
        public event Action OnPlayerStoodUp;

        private void Start()
        {
            IsPoweredOn = false;
        }

        public void TogglePower()
        {
            if (IsPoweredOn) Shutdown();
            else Boot();
        }

        public void Boot()
        {
            if (IsPoweredOn) return;
            IsPoweredOn = true;
            OnBootStart?.Invoke();
        }

        public void CompleteBoot()
        {
            OnBootComplete?.Invoke();
        }

        public void Shutdown()
        {
            if (!IsPoweredOn) return;
            OnShutdownStart?.Invoke();
        }

        public void CompleteShutdown()
        {
            IsPoweredOn = false;
            OnShutdownComplete?.Invoke();
        }

        public void SetPlayerSitting(bool sitting)
        {
            if (IsPlayerSitting == sitting) return;
            IsPlayerSitting = sitting;
            if (IsPlayerSitting) OnPlayerSatDown?.Invoke();
            else OnPlayerStoodUp?.Invoke();
        }
    }
}