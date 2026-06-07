using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ComputerSplashScreen : MonoBehaviour
{
    [SerializeField] private Image loadingImage;
    [SerializeField] private float loadingImageRotationSpeed = 1f;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private LocalizedString localWelcomeMessage;

    private void OnEnable()
    {
        loadingText.text = localWelcomeMessage.GetLocalizedString();   
    }

    void Update()
    {
        loadingImage.transform.Rotate(0f, 0f, -loadingImageRotationSpeed * Time.deltaTime);
    }
}
