using System.Collections.Generic;
using UnityEngine;

public class InputIconResolver : MonoBehaviour
{
    public static InputIconResolver Instance { get; private set; }

    [System.Serializable]
    public struct ControlIconMapping
    {
        [Tooltip("Binding effectivePath, e.g. <Keyboard>/e")]
        public string bindingPath;
        public Sprite icon;
    }
    
    [Header("Control Icons")]
    [SerializeField] private List<ControlIconMapping> controlIcons = new();
    private Dictionary<string, Sprite> controlIconLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildLookup();
    }
#endif

    private void BuildLookup()
    {
        if (controlIconLookup == null) controlIconLookup = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);
        else controlIconLookup.Clear();

        foreach (var mapping in controlIcons)
        {
            if (string.IsNullOrWhiteSpace(mapping.bindingPath) || mapping.icon == null) continue;
            controlIconLookup[mapping.bindingPath.Trim()] = mapping.icon;
        }
    }

    public Sprite GetControlIcon(string bindingPath)
    {
        if (string.IsNullOrWhiteSpace(bindingPath) || controlIconLookup == null) return null;
        controlIconLookup.TryGetValue(bindingPath.Trim(), out var icon);        
        return icon;
    }
}
