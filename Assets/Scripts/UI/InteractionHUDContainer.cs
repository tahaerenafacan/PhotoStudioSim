using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionHUDContainer : MonoBehaviour
{
    [Header("UI Spawn Settings")]
    [SerializeField] private GameObject interactionItemPrefab; 
    [SerializeField] private Transform containerParent;
    
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
    
    /// <summary>
    /// InputActionReference üzerinden action'ın ilk composite-olmayan binding'inin
    /// effectivePath'ini bulup InputIconResolver'a geçirir.
    /// </summary>
    private Sprite ResolveIconForAction(InputActionReference actionReference)
    {
        if (actionReference == null || actionReference.action == null) return null;
        if (InputIconResolver.Instance == null) return null;

        var action = actionReference.action;
        string effectivePath = string.Empty;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
            {
                effectivePath = action.bindings[i].effectivePath;
                break;
            }
        }

        return InputIconResolver.Instance.GetControlIcon(effectivePath);
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