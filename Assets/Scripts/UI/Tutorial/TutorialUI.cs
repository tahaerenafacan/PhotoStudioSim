using System.Collections.Generic;
using SyntaxSultan.TutorialSystem;
using TMPro;
using UnityEngine;

namespace SyntaxSultan.UI.Tutorial
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private CanvasGroup panelCG;
        [SerializeField] private ObjectiveItem objectiveItemPrefab;
        [SerializeField] private Transform objectivesContainer;
        
        private void OnEnable() 
        {
            TutorialManager.Instance.OnStepUpdated.AddListener(HandleStepUpdated);
            TutorialManager.Instance.OnSequenceCompleted.AddListener(HandleSequenceCompleted);
        }

        private void OnDisable()
        {
            if (TutorialManager.Instance == null) return;
            TutorialManager.Instance.OnStepUpdated.RemoveListener(HandleStepUpdated);
            TutorialManager.Instance.OnSequenceCompleted.RemoveListener(HandleSequenceCompleted);
        }

        private void Awake()
        {
            FunctionLibrary.DestroyChildren(objectivesContainer);
            FunctionLibrary.SetCanvasGroupActive(ref panelCG, false);
        }

        private void HandleStepUpdated(TutorialStepSO step, Dictionary<TutorialObjectiveSO, int> progress)
        {
            FunctionLibrary.SetCanvasGroupActive(ref panelCG, true);
            titleText.text = step.title;

            FunctionLibrary.DestroyChildren(objectivesContainer);

            foreach (var objective in step.objectives)
            {
                int current = progress[objective];

                ObjectiveItem item = Instantiate(objectiveItemPrefab, objectivesContainer);
                item.SetData(objective.description, current, objective.requiredAmount);
            }
        }
        
        private void HandleSequenceCompleted() //Tutorial ended
        {
            FunctionLibrary.DestroyChildren(objectivesContainer);
            FunctionLibrary.SetCanvasGroupActive(ref panelCG, false);
        }
    }
}