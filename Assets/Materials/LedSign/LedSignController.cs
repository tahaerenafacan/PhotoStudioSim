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
    [Header("State"), NaughtyAttributes.ReadOnly]
    [SerializeField] private bool isOpen = true;

    [Header("Yazı Maskeleri")]
    [Tooltip("Localization Tables penceresinde bir Asset Table oluşturup her dil için ilgili PNG'yi ata")]
    [SerializeField] private LocalizedAsset<Texture2D> openMaskRef;
    [SerializeField] private LocalizedAsset<Texture2D> closedMaskRef;

    [Header("Renkler")]
    [SerializeField] private Color openColor = new Color(0f, 3f, 0.2f, 1f);   // yeşil, HDR emission
    [SerializeField] private Color closedColor = new Color(3f, 0f, 0f, 1f);   // kırmızı, HDR emission

    private Renderer targetRenderer;
    private MaterialPropertyBlock propBlock;

    private Texture2D currentOpenMask;
    private Texture2D currentClosedMask;

    private static readonly int TextMaskId = Shader.PropertyToID("_TextMask");
    private static readonly int OnColorId = Shader.PropertyToID("_OnColor");
    const int LedMaterialIndex = 1;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }
    
    private void Start()
    {
        if (openMaskRef != null && !openMaskRef.IsEmpty)
        {
            openMaskRef.AssetChanged += OnOpenMaskChanged;
        }
        else
        {
            Debug.LogWarning("[LedSignController] openMaskRef Inspector üzerinde atanmamış veya tablosu boş!", this);
        }

        if (closedMaskRef != null && !closedMaskRef.IsEmpty)
        {
            closedMaskRef.AssetChanged += OnClosedMaskChanged;
        }
        else
        {
            Debug.LogWarning("[LedSignController] closedMaskRef Inspector üzerinde atanmamış veya tablosu boş!", this);
        }
    }
    /*
    private void OnDisable()
    {
        if (openMaskRef != null && !openMaskRef.IsEmpty)
        {
            openMaskRef.AssetChanged -= OnOpenMaskChanged;
        }

        if (closedMaskRef != null && !closedMaskRef.IsEmpty)
        {
            closedMaskRef.AssetChanged -= OnClosedMaskChanged;
        }
    }*/

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

    public void OnOpenMaskChanged(Texture2D newMask)
    {
        currentOpenMask = newMask;
        ApplyState();
    }

    public void OnClosedMaskChanged(Texture2D newMask)
    {
        currentClosedMask = newMask;
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
            Debug.LogWarning($"[LedSign] Mask yüklenemedi, isOpen={isOpen}. Addressables/Localization asenkron yükleme tamamlanmamış olabilir.");
            return;
        }

        targetRenderer.GetPropertyBlock(propBlock, LedMaterialIndex);
        propBlock.Clear();
        propBlock.SetTexture(TextMaskId, maskToUse);
        propBlock.SetColor(OnColorId, emissionColor);
        targetRenderer.SetPropertyBlock(propBlock, LedMaterialIndex);
    }
}