using System;
using UnityEngine;
using UnityEngine.Localization;

public class PrintedPaper : BasePickableItem, IInteractable
{
    [SerializeField] private MeshRenderer paperRenderer;
    /// <summary>Birden fazla materyal varsa resmin uygulanacağı materyal indexi.</summary>
    [SerializeField] private int materialIndex = 0;

    private PrintSettings settings;
    private Texture2D printedTexture;

    public LocalizedString InteractHint => Definition.interactHint;
    public bool CanInteract => PlayerItemHolder.Instance != null
        && PlayerItemHolder.Instance.IsHoldingItem
        && PlayerItemHolder.Instance.CurrentItem is ItemEnvelope;

    public Texture2D PrintedTexture => printedTexture;
    public PrintSettings Settings => settings;

    public void Setup(Texture2D printedImage, PrintSettings settings)
    {
        this.settings = settings;
        ApplyImage(printedImage);
    }

    /// <summary>
    /// Texture'ı settings'e göre işler ve renderer'a uygular.
    /// Pipeline sırası: Resize (kalite) → Rotate (yön) → Grayscale (renk modu) → UV fit (material).
    /// </summary>
    private void ApplyImage(Texture2D source)
    {
        if (paperRenderer == null || source == null) return;

        Texture2D processed = ProcessTexture(source);
        printedTexture = processed;

        // Diğer kağıt prefablarını etkilememek için yeni materyal instance'ı
        Material[] mats = paperRenderer.materials;
        Material mat = new Material(mats[materialIndex]);
        mat.mainTexture = processed;
        ApplyUVFit(mat, processed);
        mats[materialIndex] = mat;
        paperRenderer.materials = mats;
    }

    // ── Texture Pipeline ──────────────────────────────────────

    private Texture2D ProcessTexture(Texture2D source)
    {
        // 1. Kaliteye göre çözünürlüğü düşür (baskı süresi/bellek optimizasyonu)
        Texture2D result = ResizeByQuality(source, settings.quality);

        // 2. Landscape ise 90° döndür
        if (settings.paperOrientation == PrintPaperOrientation.Landscape)
            result = RotateTexture90(result);

        // 3. Siyah-beyaz modunda renk kanallarını grayscale'e çevir
        if (!settings.isColored)
            ConvertToGrayscale(result);

        return result;
    }

    /// <summary>
    /// Kalite seviyesine göre texture'ı orantılı küçültür.
    /// RenderTexture kullandığı için GPU-side; büyük dokular için tercih edilir.
    /// </summary>
    private Texture2D ResizeByQuality(Texture2D source, PrintQuality quality)
    {
        float scale = quality switch
        {
            PrintQuality.Low      => 0.25f,
            PrintQuality.Average  => 0.5f,
            PrintQuality.High     => 0.75f,
            PrintQuality.UltraHigh => 1.0f,
            _                     => 1.0f
        };

        // UltraHigh'da gereksiz kopyadan kaçın
        if (Math.Abs(scale - 1f) < 0.001f) return source;

        int w = Mathf.Max(1, Mathf.RoundToInt(source.width  * scale));
        int h = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));

        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        result.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    /// <summary>
    /// Texture'ı saat yönünde 90° döndürür.
    /// Landscape ayarında portrait prefab kullanıldığında görüntüyü düzeltmek için çağrılır.
    /// </summary>
    private Texture2D RotateTexture90(Texture2D source)
    {
        int srcW = source.width;
        int srcH = source.height;
        Texture2D result = new Texture2D(srcH, srcW, source.format, false);

        Color[] src = source.GetPixels();
        Color[] dst = new Color[srcW * srcH];

        for (int y = 0; y < srcH; y++)
            for (int x = 0; x < srcW; x++)
                dst[x * srcH + (srcH - 1 - y)] = src[y * srcW + x];

        result.SetPixels(dst);
        result.Apply();
        return result;
    }

    /// <summary>
    /// Tüm pikselleri luminance değerine indirgeyerek siyah-beyaz görünüm sağlar.
    /// Color.grayscale; insan gözünün renk ağırlıklarını (ITU-R BT.601) kullanır.
    /// </summary>
    private void ConvertToGrayscale(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float lum = pixels[i].grayscale;
            pixels[i] = new Color(lum, lum, lum, pixels[i].a);
        }
        tex.SetPixels(pixels);
        tex.Apply();
    }

    /// <summary>
    /// Baskı fit moduna göre material UV tiling ve offset'ini ayarlar.
    /// ScaleToFit: görüntüyü tüm yüzeye yay.
    /// ActualSize: görüntünün orijinal en-boy oranını koru, yüzeyi tam doldurmaya çalışma.
    /// </summary>
    private void ApplyUVFit(Material mat, Texture2D tex)
    {
        if (settings.paperFit == PrintPaperFit.ScaleToFit)
        {
            mat.mainTextureScale  = Vector2.one;
            mat.mainTextureOffset = Vector2.zero;
            return;
        }

        // ActualSize: mesh'in UV alanına göre aspect-ratio'yu koru.
        // Mesh'in fiziksel boyutunu bilmeden tam hesap yapılamaz;
        // bu yüzden texture aspect ratio'sunu referans alıyoruz.
        float texAspect  = (float)tex.width / tex.height;
        // Kağıt mesh'inin A4 standardına yakın oranı (~1:1.414)
        float paperAspect = settings.paperOrientation == PrintPaperOrientation.Landscape
            ? 1.414f : 1f / 1.414f;

        float ratio = texAspect / paperAspect;
        if (ratio > 1f) // Görüntü kağıttan geniş → yatayda sığdır, dikeyde boşluk bırak
        {
            mat.mainTextureScale  = new Vector2(1f, 1f / ratio);
            mat.mainTextureOffset = new Vector2(0f, (1f - 1f / ratio) * 0.5f);
        }
        else            // Görüntü kağıttan uzun → dikeyde sığdır, yatayda boşluk bırak
        {
            mat.mainTextureScale  = new Vector2(ratio, 1f);
            mat.mainTextureOffset = new Vector2((1f - ratio) * 0.5f, 0f);
        }
    }

    // ── Physics ───────────────────────────────────────────────

    public void DisablePhysics()
    {
        SetCollidersActive(false);
        Rb.isKinematic = true;
    }

    public void EnablePhysics()
    {
        SetCollidersActive(true);
        Rb.isKinematic = false;
    }

    public void Interact()
    {
        if (PlayerItemHolder.Instance == null)
            return;

        if (PlayerItemHolder.Instance.CurrentItem is ItemEnvelope envelope)
        {
            if (envelope.TryStorePrintedPaper(this))
                gameObject.SetActive(false);
        }
    }
}