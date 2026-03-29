using UnityEngine;

public static class FunctionLibrary
{
    public static void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }
}
