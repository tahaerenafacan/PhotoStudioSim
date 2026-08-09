using UnityEngine;

namespace SyntaxSultan.TutorialSystem
{
    /// <summary>
    /// "1/5 clean the dirts" gibi TEK bir objective'in Inspector'dan tanımlanabilir verisi.
    /// RequiredAmount = 1 ve eventKey boşsa, bu objective manuel CompleteManually() ile de kapatılabilir
    /// (örn: "open the store" gibi tek seferlik aksiyonlar için).
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialObjective", menuName = "PSSGame/Tutorial/Objective")]
    public class TutorialObjectiveSO : ScriptableObject
    {
        [Tooltip("UI'da gösterilecek metin, örn: 'clean the dirts'")]
        public string description;

        [Tooltip("TutorialEventBus üzerinden dinlenecek anahtar, örn: 'CleanDirt'")]
        public TutorialObjectiveKeys eventKey;

        [Min(1)]
        public int requiredAmount = 1;
    }
}