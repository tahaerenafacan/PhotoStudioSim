using UnityEngine;

public enum SurfaceType
{
    Default,
    Rock,
    Grass
}

public class SurfaceIdentifier : MonoBehaviour
{
    [SerializeField] private SurfaceType surfaceType;

    public SurfaceType GetSurfaceType()
    {
        return surfaceType;
    }
}
