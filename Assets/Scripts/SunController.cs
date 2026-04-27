using UnityEngine;

public class SunController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 dayRotationAxis = new Vector3(1, 0, 0); 
    [SerializeField] private float sunriseRotation = -10f;
    
    private Light sunLight;

    private void Awake()
    {
        sunLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (TimeManager.Instance == null) return;

        UpdateSunRotation();
        UpdateSunIntensity();
    }

    private void UpdateSunRotation()
    {
        // 0-24 saatlik zamanı 0-360 derecelik rotasyona çeviriyoruz.
        // Formül: (Zaman / 24) * 360 - 90 (90 derece ofset öğle saatini tepeye alır)
        float rotationAngle = (TimeManager.Instance.CurrentTimeOfDay / 24f) * 360f - 90f;
        
        // Sadece X ekseninde döndür (Y veya Z'yi sabit tutabilirsin)
        transform.rotation = Quaternion.Euler(rotationAngle, -30f, 0f);
    }

    private void UpdateSunIntensity()
    {
        // TimeManager'dan gelen hesaplanmış yoğunluğu ışığa uygula
        if (sunLight != null)
        {
            sunLight.intensity = TimeManager.Instance.SunIntensity;
        }
    }
}
