using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// LED panel maske dokularını (beyaz yazı / siyah zemin) tek tıkla üreten editor aracı.
// Kullanım senaryosu: LedSignController için gereken openMask/closedMask PNG'lerini
// elle Photoshop'ta hazırlamak yerine, dil listesine göre otomatik üretmek.
// Yan etki: Assets altında belirtilen klasöre PNG dosyaları yazar ve AssetDatabase'i günceller.
public class LedMaskGeneratorWindow : EditorWindow
{
    // Tek bir dil/metin girişini temsil eder (ör. "tr" -> AÇIK / KAPALI)
    [Serializable]
    private class LocalizedEntry
    {
        public string languageCode = "tr";
        public string openText = "AÇIK";
        public string closedText = "KAPALI";
    }

    private Font signFont;
    private int fontSize = 48;
    private int textureWidth = 128;
    private int textureHeight = 64;
    private string outputFolder = "Assets/LedSigns/Generated";

    private readonly List<LocalizedEntry> entries = new List<LocalizedEntry>
    {
        new LocalizedEntry { languageCode = "tr", openText = "AÇIK", closedText = "KAPALI" },
        new LocalizedEntry { languageCode = "en", openText = "OPEN", closedText = "CLOSED" },
    };

    private Vector2 scrollPos;

    [MenuItem("Tools/LED Sign/Mask Generator")]
    private static void ShowWindow()
    {
        GetWindow<LedMaskGeneratorWindow>("LED Mask Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Genel Ayarlar", EditorStyles.boldLabel);
        signFont = (Font)EditorGUILayout.ObjectField("Font", signFont, typeof(Font), false);
        fontSize = EditorGUILayout.IntField("Font Size", fontSize);
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Diller", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        for (int i = 0; i < entries.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            entries[i].languageCode = EditorGUILayout.TextField("Dil Kodu", entries[i].languageCode);
            entries[i].openText = EditorGUILayout.TextField("Açık Metni", entries[i].openText);
            entries[i].closedText = EditorGUILayout.TextField("Kapalı Metni", entries[i].closedText);

            if (GUILayout.Button("Bu Dili Sil"))
            {
                entries.RemoveAt(i);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ Dil Ekle"))
        {
            entries.Add(new LocalizedEntry());
        }

        EditorGUILayout.Space();
        GUI.enabled = signFont != null && entries.Count > 0;
        if (GUILayout.Button("Tüm Maskeleri Üret", GUILayout.Height(32)))
        {
            GenerateAllMasks();
        }
        GUI.enabled = true;
    }

    // Her dil girişi için openText ve closedText olmak üzere iki PNG üretir.
    // Dosya adlandırma: {dilKodu}_open.png / {dilKodu}_closed.png
    private void GenerateAllMasks()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        foreach (LocalizedEntry entry in entries)
        {
            RenderTextToPng(entry.openText, $"{entry.languageCode}_open");
            RenderTextToPng(entry.closedText, $"{entry.languageCode}_closed");
        }

        AssetDatabase.Refresh();
        Debug.Log($"LED maskeleri üretildi: {outputFolder}");
    }

    // Verilen metni geçici bir UI Canvas + Camera üzerinden bir RenderTexture'a çizip
    // siyah zemin üzerine beyaz yazı olacak şekilde PNG olarak diske kaydeder.
    private void RenderTextToPng(string text, string fileName)
    {
        // Sahneye kalıcı iz bırakmamak için tüm geçici nesneler işlem sonunda yok edilir
        GameObject tempRoot = new GameObject("LedMaskTempRoot", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = tempRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = tempRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(textureWidth, textureHeight);

        GameObject textGO = new GameObject("Label", typeof(Text));
        textGO.transform.SetParent(tempRoot.transform, false);
        Text textComp = textGO.GetComponent<Text>();
        textComp.font = signFont;
        textComp.fontSize = fontSize;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.text = text;
        textComp.rectTransform.sizeDelta = new Vector2(textureWidth * 0.9f, textureHeight * 0.9f);

        // Uzun kelimeler (ör. "CLOSED") sabit font boyutuyla taşıp kırpılabiliyordu.
        // Best Fit ile metin kutuya sığacak şekilde otomatik küçülüyor, sabit fontSize sadece üst sınır oluyor.
        textComp.resizeTextForBestFit = true;
        textComp.resizeTextMinSize = 8;
        textComp.resizeTextMaxSize = fontSize;
        textComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComp.verticalOverflow = VerticalWrapMode.Truncate;

        GameObject camGO = new GameObject("LedMaskTempCam", typeof(Camera));
        Camera cam = camGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.orthographicSize = textureHeight * 0.5f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 10f;
        cam.transform.position = new Vector3(0, 0, -5);
        cam.cullingMask = ~0;

        RenderTexture rt = new RenderTexture(textureWidth, textureHeight, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        canvas.worldCamera = cam;

        // Canvas'ı kameranın merkezine hizala (orthographic size piksel/unit varsayımıyla eşleşsin)
        canvas.transform.position = Vector3.zero;
        canvasRect.localScale = Vector3.one;

        cam.Render();

        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D output = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        output.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        output.Apply();

        RenderTexture.active = prevActive;

        byte[] pngBytes = output.EncodeToPNG();
        string path = Path.Combine(outputFolder, fileName + ".png");
        File.WriteAllBytes(path, pngBytes);

        // Geçici nesneleri ve kaynakları temizle - editor'de sahnede kalıntı bırakmamak önemli
        DestroyImmediate(tempRoot);
        DestroyImmediate(camGO);
        rt.Release();
        DestroyImmediate(rt);
        DestroyImmediate(output);

        // Import ayarlarını LED shader'ın beklediği formata uygun hale getir (Point filter, sıkıştırmasız)
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}