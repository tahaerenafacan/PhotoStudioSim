using System;

namespace SyntaxSultan.DirtSystem
{
    public interface ICleanable
    {
        float CleanDuration { get; }
        float NormalizedProgress { get; }
        bool IsCleaned { get; }

        void ApplyCleanProgress(float deltaTime);
        void ResetProgress();

        event Action<float> OnProgressChanged;
        event Action OnCleaned;
    }
}