using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionHUDContainer : MonoBehaviour
{
    [System.Serializable]
    public struct ActionIconMapping
    {
        public InputActionReference actionReference;
        public Sprite icon;
    }

    [Header("UI Spawn Settings")]
    [SerializeField] private GameObject interactionItemPrefab; 
    [SerializeField] private Transform containerParent;
    
    [Header("Icon Mapping")]
    [SerializeField] private List<ActionIconMapping> actionIcons = new List<ActionIconMapping>();

    private List<GameObject> spawnedUIElements = new List<GameObject>();

    private void Awake()
    {
        FunctionLibrary.DestroyChildren(containerParent);
    }

    private void Start()
    {
        if (PlayerItemHolder.Instance != null)
        {
            PlayerItemHolder.Instance.OnInteractionOptionsChanged += PlayerItemHolder_OnInteractionOptionsChanged;
        }
    }

    private void OnDestroy()
    {
        if (PlayerItemHolder.Instance != null)
        {
            PlayerItemHolder.Instance.OnInteractionOptionsChanged -= PlayerItemHolder_OnInteractionOptionsChanged;
        }
    }

    private void PlayerItemHolder_OnInteractionOptionsChanged(IComplexUsable complexUsable)
    {
        ClearUI();

        List<ItemInteraction> interactions = complexUsable?.GetInteractions();
        if (interactions == null) return;

        foreach (var interaction in interactions)
        {
            SpawnInteractionUI(interaction);
        }
    }

    private void SpawnInteractionUI(ItemInteraction interaction)
    {
        if (interactionItemPrefab == null || containerParent == null) return;

        GameObject uiObj = Instantiate(interactionItemPrefab, containerParent);
        spawnedUIElements.Add(uiObj);
        
        if (uiObj.TryGetComponent<ExtraInteractionItem>(out var uiItem))
        {
            // InputActionReference'a karşılık gelen ikonu sözlükten bul
            Sprite resolvedIcon = ResolveIconForAction(interaction.ActionReference);
            uiItem.Setup(resolvedIcon, interaction.Hint);
        }
    }

    private Sprite ResolveIconForAction(InputActionReference actionRef)
    {
        if (actionRef == null) return null;

        // Liste içinde eşleşen InputActionReference guid'lerini kontrol et
        var mapping = actionIcons.Find(m => m.actionReference != null && 
            m.actionReference.action.id == actionRef.action.id);

        return mapping.icon;
    }

    private void ClearUI()
    {
        foreach (var element in spawnedUIElements)
        {
            if (element != null) Destroy(element);
        }
        spawnedUIElements.Clear();
    }
}