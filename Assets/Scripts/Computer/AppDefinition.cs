using UnityEngine;

[CreateAssetMenu(fileName = "NewApp", menuName = "PSSGame/Computer/App Definition")]
public class AppDefinition : ScriptableObject
{
    public string appName;
    public Sprite icon;
    [Tooltip("AppWindow component'i içeren prefab.")]
    public AppWindow windowPrefab;
    public bool installedByDefault = true;
}