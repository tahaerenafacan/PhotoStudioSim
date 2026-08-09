using UnityEngine;

namespace SyntaxSultan.TutorialSystem
{

    /// <summary>
    /// Oyunun baştan sona tutorial akışı. TutorialManager bu listeyi sırayla işler.
    /// Farklı sahneler/senaryolar için birden fazla sequence oluşturulabilir (SSOT ihlali değil,
    /// her sequence kendi kapsamının tek doğru kaynağı).
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "PSSGame/Tutorial/Sequence")]
    public class TutorialSequenceSO : ScriptableObject
    {
        public TutorialStepSO[] steps;
    }
}