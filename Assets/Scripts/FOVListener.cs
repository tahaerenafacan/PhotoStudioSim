using Unity.Cinemachine;
using UnityEngine;

public class FOVListener : MonoBehaviour
{
    private CinemachineCamera targetCamera;

    private void Awake()
    {
        targetCamera = GetComponent<CinemachineCamera>();
    }
    
    private void Start()
    {
        if (SettingsManager.Instance != null && SettingsManager.Instance.CurrentSettings != null)
        {
            UpdateFOV(SettingsManager.Instance.CurrentSettings.fov);
            SettingsManager.OnFOVChanged += UpdateFOV;
        }
    }

    private void OnDestroy()
    {
        SettingsManager.OnFOVChanged -= UpdateFOV;
    }

    private void UpdateFOV(float newFov)
    {
        if (targetCamera != null)
        {
            targetCamera.Lens.FieldOfView = newFov;
        }
    }
}
