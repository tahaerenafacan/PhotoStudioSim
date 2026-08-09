using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewItem", menuName = "PSSGame/Item System/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Genel")] 
    [SerializeField, ReadOnly] private string itemId;
    public string ItemId => itemId;
        
    public LocalizedString itemName;
    public Sprite icon;
    public BasePickableItem itemPrefab;
    
    [Header("Elde Tutma Pozisyonu")]
    [Tooltip("Hold point'e göre lokal pozisyon ofseti.")]
    public Vector3 holdPositionOffset = Vector3.zero;

    [Tooltip("Hold point'e göre lokal rotasyon ofseti.")]
    public Vector3 holdRotationOffset = Vector3.zero;
    
    
#if UNITY_EDITOR
    
    private void Update() 
    {
        if (Application.IsPlaying(this)) return;

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty property = serializedObject.FindProperty("itemId");
            
        if (string.IsNullOrEmpty(property.stringValue))
        {
            property.stringValue = System.Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}