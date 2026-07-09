using UnityEngine;

public interface IPlaceable
{
    Transform PlacementTransform { get; }
    Collider[] PlacementColliders { get; }

    Renderer[] PlacementRenderers { get; }

    bool AllowVerticalPlacement { get; }

    void SetPlacementCollidersEnabled(bool isEnabled);
    void OnPlacementConfirmed();
    void OnPlacementCancelled();
}