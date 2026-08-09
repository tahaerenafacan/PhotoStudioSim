using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SyntaxSultan.TutorialSystem
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [SerializeField] private TutorialSequenceSO sequence;

        // UI bu event'i dinleyip ekranı günceller — Manager UI'ı bilmez (decoupled)
        public UnityEvent<TutorialStepSO, Dictionary<TutorialObjectiveSO, int>> OnStepUpdated = new();
        public UnityEvent OnSequenceCompleted = new();

        private int currentStepIndex = -1;
        private Dictionary<TutorialObjectiveSO, int> progress = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartSequence()
        {
            currentStepIndex = -1;
            AdvanceToNextStep();
        }

        private void AdvanceToNextStep()
        {
            UnsubscribeCurrentStep();

            currentStepIndex++;
            if (sequence == null || currentStepIndex >= sequence.steps.Length)
            {
                OnSequenceCompleted.Invoke();
                return;
            }

            progress.Clear();
            var step = sequence.steps[currentStepIndex];
            foreach (var objective in step.objectives)
            {
                progress[objective] = 0;
                TutorialEventBus.Subscribe(objective.eventKey, amount => OnObjectiveProgress(objective, amount));
            }

            OnStepUpdated.Invoke(step, progress);
        }

        private void OnObjectiveProgress(TutorialObjectiveSO objective, int amount)
        {
            if (!progress.ContainsKey(objective)) return; // bu step'e ait değil, yok say

            progress[objective] = Mathf.Min(progress[objective] + amount, objective.requiredAmount);
            OnStepUpdated.Invoke(sequence.steps[currentStepIndex], progress);

            CheckStepCompletion();
        }

        /// <summary>
        /// eventKey'i olmayan objective'ler için (örn: "open the store") dışarıdan manuel tamamlama.
        /// </summary>
        public void CompleteObjectiveManually(TutorialObjectiveSO objective)
        {
            OnObjectiveProgress(objective, objective.requiredAmount);
        }

        private void CheckStepCompletion()
        {
            foreach (var kvp in progress)
                if (kvp.Value < kvp.Key.requiredAmount)
                    return; // hala tamamlanmamış objective var

            AdvanceToNextStep();
        }

        private void UnsubscribeCurrentStep()
        {
            if (currentStepIndex < 0 || sequence == null || currentStepIndex >= sequence.steps.Length) return;

            foreach (var objective in sequence.steps[currentStepIndex].objectives)
            {
                TutorialEventBus.Unsubscribe(objective.eventKey, amount => OnObjectiveProgress(objective, amount));
            }
        }
    }
}