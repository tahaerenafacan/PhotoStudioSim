using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SyntaxSultan.SavingSystem;
using UnityEngine;
using UnityEngine.Events;

namespace SyntaxSultan.TutorialSystem
{
    public class TutorialManager : MonoBehaviour, IJsonSaveable
    {
        public static TutorialManager Instance { get; private set; }

        [SerializeField] private TutorialSequenceSO sequence;

        // UI bu event'i dinleyip ekranı günceller — Manager UI'ı bilmez (decoupled)
        public UnityEvent<TutorialStepSO, Dictionary<TutorialObjectiveSO, int>> OnStepUpdated = new();
        public UnityEvent OnSequenceCompleted = new();

        private const string CurrentStepIndexKey = "currentStepIndex";
        private const string ProgressKey = "progress";

        private int currentStepIndex = -1;
        private Dictionary<TutorialObjectiveSO, int> progress = new();
        private readonly Dictionary<TutorialObjectiveSO, System.Action<int>> objectiveListeners = new();

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
                progress.Clear();
                ObjectiveListenersClear();
                OnSequenceCompleted.Invoke();
                return;
            }

            progress.Clear();
            objectiveListeners.Clear();

            var step = sequence.steps[currentStepIndex];
            foreach (var objective in step.objectives)
            {
                progress[objective] = 0;
                System.Action<int> listener = amount => OnObjectiveProgress(objective, amount);
                objectiveListeners[objective] = listener;
                TutorialEventBus.Subscribe(objective.eventKey, listener);
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
            if (sequence == null || currentStepIndex < 0 || currentStepIndex >= sequence.steps.Length) return;

            foreach (var objective in sequence.steps[currentStepIndex].objectives)
            {
                if (objectiveListeners.TryGetValue(objective, out var listener))
                {
                    TutorialEventBus.Unsubscribe(objective.eventKey, listener);
                }
            }

            objectiveListeners.Clear();
        }

        private void ObjectiveListenersClear()
        {
            if (sequence == null || currentStepIndex < 0 || currentStepIndex >= sequence.steps.Length) return;
            foreach (var objective in sequence.steps[currentStepIndex].objectives)
            {
                if (objectiveListeners.TryGetValue(objective, out var listener))
                {
                    TutorialEventBus.Unsubscribe(objective.eventKey, listener);
                }
            }

            objectiveListeners.Clear();
        }

        public JToken CaptureAsJToken()
        {
            JObject state = new JObject();
            state[CurrentStepIndexKey] = currentStepIndex;

            JArray progressArray = new JArray();
            foreach (var kvp in progress)
            {
                if (kvp.Key == null) continue;
                JObject objectiveState = new JObject();
                objectiveState["objectiveName"] = kvp.Key.name;
                objectiveState["progress"] = kvp.Value;
                progressArray.Add(objectiveState);
            }

            state[ProgressKey] = progressArray;
            return state;
        }

        public void RestoreFromJToken(JToken state)
        {
            if (state == null || state.Type != JTokenType.Object) return;

            JObject stateObject = state.ToObject<JObject>();
            currentStepIndex = stateObject[CurrentStepIndexKey]?.ToObject<int>() ?? -1;

            UnsubscribeCurrentStep();
            progress.Clear();
            objectiveListeners.Clear();

            if (sequence == null)
            {
                Debug.LogWarning("TutorialManager: no sequence assigned while restoring tutorial state.");
                return;
            }

            if (currentStepIndex < 0)
                return;

            if (currentStepIndex >= sequence.steps.Length)
            {
                progress.Clear();
                ObjectiveListenersClear();
                OnSequenceCompleted.Invoke();
                return;
            }

            var step = sequence.steps[currentStepIndex];
            JArray progressArray = stateObject[ProgressKey] as JArray;
            foreach (var objective in step.objectives)
            {
                int savedProgress = 0;
                if (progressArray != null)
                {
                    foreach (JToken objectiveToken in progressArray)
                    {
                        JObject objectiveObject = objectiveToken as JObject;
                        if (objectiveObject == null) continue;
                        string objectiveName = objectiveObject["objectiveName"]?.ToObject<string>();
                        if (objectiveName == objective.name)
                        {
                            savedProgress = objectiveObject["progress"]?.ToObject<int>() ?? 0;
                            break;
                        }
                    }
                }

                progress[objective] = Mathf.Min(savedProgress, objective.requiredAmount);

                System.Action<int> listener = amount => OnObjectiveProgress(objective, amount);
                objectiveListeners[objective] = listener;
                TutorialEventBus.Subscribe(objective.eventKey, listener);
            }

            OnStepUpdated.Invoke(step, progress);
        }
    }
}