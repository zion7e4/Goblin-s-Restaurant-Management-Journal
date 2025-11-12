using UnityEngine;
using System.Collections.Generic;

public class FameManager : MonoBehaviour
{
    public static FameManager instance;

    [SerializeField]
    private float currentFamePoints;

    public float CurrentFamePoints
    {
        get { return currentFamePoints; }
        private set { currentFamePoints = value; }
    }

    [SerializeField]
    private int currentFameLevel;

    public int CurrentFameLevel
    {
        get { return currentFameLevel; }
        private set { currentFameLevel = value; }
    }

    private readonly List<int> fameLevelThresholds = new List<int> { 10, 25, 50, 100, 200, 400 };

    public event System.Action OnFameChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateFameLevel();
    }

    public void AddFame(float amount)
    {
        CurrentFamePoints += amount;
        if (CurrentFamePoints > fameLevelThresholds[fameLevelThresholds.Count - 1])
        {
            CurrentFamePoints = fameLevelThresholds[fameLevelThresholds.Count - 1];
        }

        UpdateFameLevel();
        OnFameChanged?.Invoke();
        Debug.Log($"명성도 {amount} 증가. 현재 점수: {CurrentFamePoints}");
    }

    public void DecreaseFame(float amount)
    {
        CurrentFamePoints -= amount;
        if (CurrentFamePoints < 0) CurrentFamePoints = 0;

        UpdateFameLevel();
        OnFameChanged?.Invoke();
        Debug.Log($"명성도 {amount} 감소. 현재 점수: {CurrentFamePoints}");
    }

    private void UpdateFameLevel()
    {
        CurrentFameLevel = 0;
        for (int i = 0; i < fameLevelThresholds.Count; i++)
        {
            if (CurrentFamePoints > fameLevelThresholds[i])
            {
                CurrentFameLevel = i + 1;
            }
            else
            {
                break;
            }
        }
        if (CurrentFameLevel > 5) CurrentFameLevel = 5;
    }

    public float GetProgressToNextLevel()
    {
        if (CurrentFameLevel >= 5) return 1f;

        int prevThreshold = (CurrentFameLevel == 0) ? 0 : fameLevelThresholds[CurrentFameLevel - 1];

        int nextThreshold = fameLevelThresholds[CurrentFameLevel];

        float pointsInThisLevel = CurrentFamePoints - prevThreshold;

        float pointsNeededForThisLevel = nextThreshold - prevThreshold;

        return pointsInThisLevel / pointsNeededForThisLevel;
    }
}
