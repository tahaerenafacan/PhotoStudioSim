#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using SyntaxSultan.ComputerSystem;

/// <summary>
/// Yeni bilgisayar uygulaması oluşturmak için Editor penceresi.
///
/// AÇILIŞ: Tools > PSSGame > App Creator
///
/// NE YAPAR:
///   1. Doldurduğun bilgilerle AppDefinition ScriptableObject oluşturur
///   2. İsteğe bağlı olarak AppWindow base prefab'ından varyant oluşturur
///   3. Oluşturulan SO'yu project'te seçili hale getirir (sürükle-bırak hazır)
/// </summary>
public class AppCreatorEditorWindow : EditorWindow
{
    private string      appName          = "New App";
    private Sprite      appIcon;
    private AppWindow   windowPrefab;
    private bool        createPrefabVariant = true;
    private string      soSavePath     = "Assets/Scripts/Computer/Apps";
    private string      prefabSavePath = "Assets/Scripts/Computer/Apps";

    private Vector2 scrollPos;

    [MenuItem("Tools/PSSGame/App Creator")]
    public static void ShowWindow()
    {
        var win = GetWindow<AppCreatorEditorWindow>("App Creator");
        win.minSize = new Vector2(360, 380);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawSectionHeader("App Data");
        appName = EditorGUILayout.TextField("App Name", appName);
        appIcon = (Sprite)EditorGUILayout.ObjectField(
            "Icon", appIcon, typeof(Sprite), false);

        EditorGUILayout.Space(8);
        DrawSectionHeader("Window Prefab");
        windowPrefab = (AppWindow)EditorGUILayout.ObjectField(
            "Base Window Prefab", windowPrefab, typeof(AppWindow), false);
        createPrefabVariant = EditorGUILayout.Toggle(
            "Prefab Varyant Oluştur", createPrefabVariant);

        EditorGUILayout.Space(8);
        DrawSectionHeader("Save Path");
        soSavePath     = EditorGUILayout.TextField("SO Path",     soSavePath);
        prefabSavePath = EditorGUILayout.TextField("Prefab Path", prefabSavePath);

        EditorGUILayout.Space(16);
        EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(appName));
        if (GUILayout.Button("Create", GUILayout.Height(40)))
            CreateApp();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(8);
        DrawHelp();
        EditorGUILayout.EndScrollView();
    }

    private void CreateApp()
    {
        EnsureDirectory(soSavePath);

        // 1. AppDefinition SO oluştur
        string safeName = appName.Trim();
        string soPath   = $"{soSavePath}/{safeName}App.asset";

        if (!ConfirmOverwrite(soPath)) return;

        var def = CreateInstance<AppDefinition>();
        def.appName           = appName;
        def.icon              = appIcon;

        // 2. İsteğe bağlı prefab varyant
        if (createPrefabVariant && windowPrefab != null)
        {
            EnsureDirectory(prefabSavePath);
            string prefabPath = $"{prefabSavePath}/{safeName}Window.prefab";

            string sourcePath = AssetDatabase.GetAssetPath(windowPrefab.gameObject);
            if (!string.IsNullOrEmpty(sourcePath))
            {
                AssetDatabase.CopyAsset(sourcePath, prefabPath);
                AssetDatabase.Refresh();

                var variantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var variantWindow = variantPrefab != null
                    ? variantPrefab.GetComponent<AppWindow>() : null;
                def.windowPrefab = variantWindow;

                Debug.Log($"[AppCreator] Prefab varyant: {prefabPath}");
            }
        }
        else if (windowPrefab != null)
        {
            def.windowPrefab = windowPrefab;
        }

        AssetDatabase.CreateAsset(def, soPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = def;

        Debug.Log($"[AppCreator] '{appName}' oluşturuldu: {soPath}");
    }

    private static void DrawSectionHeader(string title)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        EditorGUILayout.Space(4);
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    private static bool ConfirmOverwrite(string path) =>
        !File.Exists(path) ||
        EditorUtility.DisplayDialog("Üzerine Yaz?",
            $"'{path}' zaten mevcut. Devam et?", "Evet", "İptal");

    private static void DrawHelp()
    {
        EditorGUILayout.HelpBox(
            "KULLANIM ADIMLARI:\n" +
            "1. App adını ve ikonunu gir\n" +
            "2. Varsa base AppWindow prefab'ını seç\n" +
            "3. 'Prefab Varyant Oluştur' işaretliyse özgün prefab kopyası yapılır\n" +
            "4. 'Oluştur' butonuna bas → SO proje'de seçilir\n" +
            "5. SO'yu AppManager > Initial Apps listesine sürükle-bırak",
            MessageType.Info);
    }
}
#endif