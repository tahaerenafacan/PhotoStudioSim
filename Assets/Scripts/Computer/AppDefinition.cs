using Sirenix.OdinInspector;
using UnityEngine;

namespace SyntaxSultan.ComputerSystem
{
    [CreateAssetMenu(fileName = "NewApp", menuName = "PSSGame/Computer/App Definition")]
    public class AppDefinition : ScriptableObject
    {
        [Required] public string appName;
        [Required] public Sprite icon;
        [Required] public AppWindow windowPrefab;
    }
}