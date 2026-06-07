using UnityEngine;

namespace SyntaxSultan.ComputerSystem
{
    [CreateAssetMenu(fileName = "NewApp", menuName = "PSSGame/Computer/App Definition")]
    public class AppDefinition : ScriptableObject
    {
        public string appName;
        public Sprite icon;
        public AppWindow windowPrefab;
        public bool installedByDefault = true;
    }
}