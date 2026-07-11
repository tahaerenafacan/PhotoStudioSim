using UnityEngine;

public class PlaceableObject : MonoBehaviour, IPlaceable
{
    [SerializeField] private Collider[] placementColliders;
    [SerializeField] private Renderer[] placementRenderers;

    [Tooltip("False ise bu obje duvar gibi dikey yüzeylere yerleştirilemez.")]
    [SerializeField] private bool allowVerticalPlacement = false;

    public Transform PlacementTransform => transform;
    public Collider[] PlacementColliders => placementColliders;
    public Renderer[] PlacementRenderers => placementRenderers;
    public bool AllowVerticalPlacement => allowVerticalPlacement;

    public void SetPlacementCollidersEnabled(bool isEnabled)
    {
        foreach (var col in placementColliders)
        {
            if (col != null) col.enabled = isEnabled;
        }
    }

    public void OnPlacementConfirmed() { }
    public void OnPlacementCancelled() { }
}