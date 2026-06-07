using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewItem", menuName = "PSSGame/Item System/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Genel")]
    public LocalizedString itemName;
    public Sprite icon;
    public BasePickableItem itemPrefab;


    public bool isInteractable = false;
    [NaughtyAttributes.EnableIf("isInteractable")]
    public LocalizedString interactHint;
    
    [Header("Useable Item Settings")]
    public bool isUseable = false;
    
    [NaughtyAttributes.EnableIf("isUseable")]
    [Tooltip("Item elde tutulurken gösterilecek kullanım ipucu.")]
    public LocalizedString useHint;
    
    [Header("Elde Tutma Pozisyonu")]
    [Tooltip("Hold point'e göre lokal pozisyon ofseti.")]
    [SerializeField] 
    public Vector3 holdPositionOffset = Vector3.zero;

    [Tooltip("Hold point'e göre lokal rotasyon ofseti.")]
    [SerializeField] 
    public Vector3 holdRotationOffset = Vector3.zero;
    
}