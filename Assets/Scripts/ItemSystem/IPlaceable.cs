using System;
using UnityEngine;

public interface IPlaceable
{
    Transform PlacementTransform { get; }
    Collider[] PlacementColliders { get; }

    Renderer[] PlacementRenderers { get; }

    PlacementType PlacementAllowance { get; }

    void SetPlacementCollidersEnabled(bool isEnabled);
    void OnPlacementConfirmed();
    void OnPlacementCancelled();
    
    public enum PlacementType
    {
        OnlyHorizontal,
        OnlyVertical,
        Both
    }
}

