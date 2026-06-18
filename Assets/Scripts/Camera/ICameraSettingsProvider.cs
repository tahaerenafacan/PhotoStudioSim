using UnityEngine;

namespace SyntaxSultan.CameraSystem
{
    public interface ICameraSettingsProvider
    {
        CameraPhysicalSettings GetSettings();
        void SetFocalLength(float focalLength);
    }
}