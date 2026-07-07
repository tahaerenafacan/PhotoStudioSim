using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Kullanım Rehberi:
/// Bu bileşen, aynı GameObject üzerindeki bir UIDocument bileşenine bağlı StudioNamePopup.uxml
/// dosyasını yönetir. Popup'ı açmak için Show() çağrılmalıdır; Show() çağrılana kadar panel gizlidir.
/// Sonuç, doğrudan return değeri olarak değil, OnConfirmed / OnCancelled event'leri üzerinden dışarı verilir.
/// Bu sayede popup, sonucu kimin kullanacağını bilmek zorunda kalmaz (Dependency Inversion).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class StudioNamePopupController : MonoBehaviour
{
    // Diğer sistemlerin abone olabileceği tek doğruluk kaynağı (SSOT): onaylanan isim sadece burada yayılır.
    public event Action<string> OnConfirmed;
    public event Action OnCancelled;

    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 24;

    private VisualElement overlay;
    private TextField nameField;
    private Label validationLabel;
    private Button confirmButton;
    private Button cancelButton;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        overlay = root.Q<VisualElement>("Overlay");
        nameField = root.Q<TextField>("StudioNameField");
        validationLabel = root.Q<Label>("ValidationLabel");
        confirmButton = root.Q<Button>("ConfirmButton");
        cancelButton = root.Q<Button>("CancelButton");

        // Popup sahne yüklendiğinde otomatik açılmamalı; sadece Show() ile tetiklenmeli.
        SetVisible(false);
    }

    private void OnEnable()
    {
        confirmButton.clicked += HandleConfirmClicked;
        cancelButton.clicked += HandleCancelClicked;

        // Kullanıcı yazarken anlık doğrulama yapılır; Confirm butonu geçersiz girişte devre dışı kalır.
        nameField.RegisterValueChangedCallback(HandleNameChanged);
    }

    private void OnDisable()
    {
        confirmButton.clicked -= HandleConfirmClicked;
        cancelButton.clicked -= HandleCancelClicked;
        nameField.UnregisterValueChangedCallback(HandleNameChanged);
    }

    /// <summary>
    /// Popup'ı açar ve alanı sıfırlar. Stüdyo oluşturma akışının başında çağrılmalıdır.
    /// </summary>
    public void Show(string prefillValue = "")
    {
        nameField.SetValueWithoutNotify(prefillValue);
        UpdateValidationState(prefillValue);
        SetVisible(true);
        nameField.Focus();
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void HandleNameChanged(ChangeEvent<string> evt)
    {
        UpdateValidationState(evt.newValue);
    }

    private void HandleConfirmClicked()
    {
        string trimmedName = nameField.value.Trim();

        // Çift kontrol: buton devre dışı olsa da, doğrudan tıklama simülasyonlarına karşı güvenlik amaçlı tekrar doğrula.
        if (!IsValidName(trimmedName))
        {
            return;
        }

        OnConfirmed?.Invoke(trimmedName);
        Hide();
    }

    private void HandleCancelClicked()
    {
        OnCancelled?.Invoke();
        Hide();
    }

    private void UpdateValidationState(string currentValue)
    {
        string trimmed = currentValue?.Trim() ?? string.Empty;
        bool valid = IsValidName(trimmed);

        confirmButton.SetEnabled(valid);

        // Boş alanda hata mesajı gösterilmez; kullanıcı henüz yazmaya başlamamış olabilir.
        if (trimmed.Length == 0)
        {
            validationLabel.text = string.Empty;
        }
        else if (trimmed.Length < minNameLength)
        {
            validationLabel.text = $"İsim en az {minNameLength} karakter olmalı.";
        }
        else if (trimmed.Length > maxNameLength)
        {
            validationLabel.text = $"İsim en fazla {maxNameLength} karakter olabilir.";
        }
        else
        {
            validationLabel.text = string.Empty;
        }
    }

    private bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name)
               && name.Length >= minNameLength
               && name.Length <= maxNameLength;
    }

    private void SetVisible(bool isVisible)
    {
        overlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}