using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item System/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Genel")]
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public BasePickableItem itemPrefab;

    [NaughtyAttributes.EnableIf("type", ItemType.Static)]
    public string interactHint = "";

    [NaughtyAttributes.EnableIf("type", ItemType.Currency)]
    [Header("Para (sadece Currency için)")]
    [Tooltip("Pickup anında eklenecek para miktarı.")]
    public int currencyValue = 1;
    
    [NaughtyAttributes.EnableIf("type", ItemType.Pickable)]
    [Header("UI")]
    [Tooltip("Item elde tutulurken gösterilecek kullanım ipucu. Boş bırakılırsa gösterilmez.")]
    public string useHint;
    
    [NaughtyAttributes.EnableIf("type", ItemType.Pickable)]
    [Header("Kullanım (sadece Usable için)")]
    public UseEffect effect;
    
    [NaughtyAttributes.EnableIf("type", ItemType.Pickable)]
    [Header("Elde Tutma Pozisyonu")]
    [Tooltip("Hold point'e göre lokal pozisyon ofseti.")]
    [SerializeField] 
    public Vector3 holdPositionOffset = Vector3.zero;
    
    [Tooltip("Hold point'e göre lokal rotasyon ofseti.")]
    [SerializeField] 
    public Vector3 holdRotationOffset = Vector3.zero;
    
}

public enum ItemType
{
    Static,        // Bilgisayar, Yazıcı
    Pickable,      // Yiyecek, Kamera — Use() ile effect tetiklenir
    Currency      // Para — pickup anında CurrencyManager'a eklenir, elde tutulmaz
}