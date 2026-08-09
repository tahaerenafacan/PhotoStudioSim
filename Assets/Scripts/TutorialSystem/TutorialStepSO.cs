using UnityEngine;

namespace SyntaxSultan.TutorialSystem
{
    /// <summary>
    /// "Cleaning the house" gibi bir başlık altında AYNI ANDA aktif olan objective grubu.
    /// Step, içindeki TÜM objective'ler tamamlanınca biter (senin örneğindeki gibi).
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "PSSGame/Tutorial/Step")]
    public class TutorialStepSO : ScriptableObject
    {
        public string title;
        public TutorialObjectiveSO[] objectives;
    }
}