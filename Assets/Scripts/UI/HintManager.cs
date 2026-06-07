using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
    [SerializeField] private GameObject pickupHint;
    [SerializeField] private GameObject interactHint;

    public void OnItemChanged(string itemName, bool interact, bool pickup)
    {
        itemNameText.text = itemName;
        interactHint.SetActive(interact);
        pickupHint.SetActive(pickup);
    }
}
