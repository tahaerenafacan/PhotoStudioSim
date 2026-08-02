using UnityEngine;
using UnityEngine.Localization;

// Dükkan tabelası gibi Açık/Kapalı durumunu tek noktadan yöneten kontrolcü (SSOT).
// Sahnede bir kez bu script'i panel objesine ekle, isOpen'ı değiştirdiğinde
// hem yazı maskesi hem de LED rengi otomatik güncellenir.
// Yazı maskeleri Unity Localization Package üzerinden yönetilir; oyunun dili
// LocalizationSettings.SelectedLocale ile değiştiğinde maskeler otomatik yenilenir.
[RequireComponent(typeof(Renderer))]
public class LedSignController : MonoBehaviour
{
    [Header("Durum")]
    [SerializeField] private bool isOpen = true;

    [Header("Yazı Maskeleri (Localization Table üzerinden, dile göre otomatik değişir)")]
    [Tooltip("Localization Tables penceresinde bir Asset Table oluşturup her dil için ilgili PNG'yi ata")]
    [SerializeField] private LocalizedTexture openMaskRef;
    [SerializeField] private LocalizedTexture closedMaskRef;

    [Header("Renkler")]
    [SerializeField] private Color openColor = new Color(0f, 3f, 0.2f, 1f);   // yeşil, HDR emission
    [SerializeField] private Color closedColor = new Color(3f, 0f, 0f, 1f);   // kırmızı, HDR emission

    private Renderer targetRenderer;
    private MaterialPropertyBlock propBlock;

    // Localization'dan gelen son yüklenmiş dokular - AssetChanged event'i ile güncel tutulur
    private Texture2D currentOpenMask;
    private Texture2D currentClosedMask;

    private static readonly int TextMaskId = Shader.PropertyToID("_TextMask");
    private static readonly int OnColorId = Shader.PropertyToID("_OnColor");

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        // Locale her değiştiğinde (veya ilk yüklemede) bu event tetiklenir,
        // dil değişimini elle takip etmemize gerek kalmaz
        openMaskRef.AssetChanged += OnOpenMaskChanged;
        closedMaskRef.AssetChanged += OnClosedMaskChanged;
    }

    private void OnDisable()
    {
        openMaskRef.AssetChanged -= OnOpenMaskChanged;
        closedMaskRef.AssetChanged -= OnClosedMaskChanged;
    }

    public void ToggleOpen()
    {
        isOpen = !isOpen;
        ApplyState();
    }
    
    public void SetOpen(bool open)
    {
        isOpen = open;
        ApplyState();
    }

    private void OnOpenMaskChanged(Texture newMask)
    {
        currentOpenMask = newMask as Texture2D;
        ApplyState();
    }

    private void OnClosedMaskChanged(Texture newMask)
    {
        currentClosedMask = newMask as Texture2D;
        ApplyState();
    }

    // Mevcut isOpen değerine ve son yüklenen lokalize dokuya göre shader'ı günceller.
    // Hem durum değişiminde hem de dil değişiminde (AssetChanged üzerinden) tetiklenir.
    private void ApplyState()
    {
        Texture2D maskToUse = isOpen ? currentOpenMask : currentClosedMask;
        Color emissionColor = isOpen ? openColor : closedColor;

        if (maskToUse == null)
        {
            // Localization tablosu henüz yüklenmemiş olabilir (örn. sahne yeni başladı),
            // bu durumda ilgili AssetChanged event'i geldiğinde ApplyState zaten tekrar çağrılacak
            return;
        }

        targetRenderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture(TextMaskId, maskToUse);
        propBlock.SetColor(OnColorId, emissionColor);
        targetRenderer.SetPropertyBlock(propBlock);
    }
}