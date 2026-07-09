using System;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance { get; private set; }
    public event Action<int> OnReputationChanged;
    public event Action<int> OnReputationLevelUp;

    public int Reputation { get; private set; }
    public int ReputationLevel { get; private set; }

    [SerializeField] private int[] minRepToLevelUp;

    private void Awake()
    {
        Instance = this;
    }

    public void AddReputation(int amount)
    {
        Reputation += amount;
        OnReputationChanged?.Invoke(Reputation);
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (ReputationLevel < minRepToLevelUp.Length && Reputation >= minRepToLevelUp[ReputationLevel])
        {
            ReputationLevel++;
            OnReputationLevelUp?.Invoke(ReputationLevel);
        }
    }
}