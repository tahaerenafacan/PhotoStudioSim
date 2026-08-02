using System;
using UnityEngine;

namespace SyntaxSultan.DirtSystem
{
        public class DirtItem : MonoBehaviour, ICleanable
        {
            [SerializeField] private float cleanDuration = 3f;

            private float currentProgress;
            private bool isCleaned;

            public float CleanDuration => cleanDuration;
            public float NormalizedProgress => Mathf.Clamp01(currentProgress /  cleanDuration);
            public bool IsCleaned => isCleaned;

            public event Action<float> OnProgressChanged;
            public event Action OnCleaned;

            public void ApplyCleanProgress(float deltaTime)
            {
                if (isCleaned) return;

                currentProgress += deltaTime;
                OnProgressChanged?.Invoke(NormalizedProgress);

                if (currentProgress >= cleanDuration)
                {
                    isCleaned = true;
                    OnCleaned?.Invoke();
                    Destroy(gameObject);
                }
            }

            public void ResetProgress()
            {
                if (currentProgress == 0f) return;

                currentProgress = 0f;
                OnProgressChanged?.Invoke(0f);
            }
        }
}