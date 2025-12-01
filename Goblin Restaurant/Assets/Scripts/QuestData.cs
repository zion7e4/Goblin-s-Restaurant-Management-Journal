using System;
using System.Collections.Generic;
using System.Linq;

public enum QuestType { Main, Daily, VIP }
public enum QuestTargetType { Collect, Count, Level, None }

[Serializable]
public class QuestData
{
    // --- CSV 데이터 ---
    public int id;
    public QuestType type;
    public string title;
    public string description;
    public QuestTargetType targetType;
    public string target;           
    public int targetFigure;        
    public string triggerCondition;
    public string reward;

    // --- 런타임 데이터 ---
    public bool isUnlocked = false;
    public bool isCompleted = false;
    public bool isRewardClaimed = false;
    
    // 현재 진행도 (Key: ID, Value: 현재 값)
    public Dictionary<string, int> progressDict = new Dictionary<string, int>();
    
    // ▼▼▼ [신규] 목표치 개별 관리 딕셔너리 (Key: ID, Value: 목표 값) ▼▼▼
    public Dictionary<string, int> targetDict = new Dictionary<string, int>();
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    public QuestData(string[] values)
    {
        id = int.Parse(values[0]);
        type = ParseQuestType(values[1]);
        title = CleanString(values[2]);
        description = CleanString(values[3]);
        targetType = ParseTargetType(values[4]);
        target = CleanString(values[5]);
        targetFigure = int.Parse(values[6]);
        triggerCondition = CleanString(values[7]);
        reward = CleanString(values[8]);

        InitializeProgressDict();
    }

    private string CleanString(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return raw.Trim().Trim('"');
    }

    // ▼▼▼ [수정] 목표 문자열 파싱 로직 (수량 분리) ▼▼▼
    public void InitializeProgressDict()
    {
        progressDict.Clear();
        targetDict.Clear();

        // 콤마로 구분 (예: "ING01:2, ING02:1" 또는 "ING01, ING02")
        string[] targets = target.Split(',');
        
        foreach (var t in targets)
        {
            string key = t.Trim();
            int requiredAmount = targetFigure; // 기본값은 CSV의 target_figure

            // 콜론(:)이 있으면 분리 (예: "ING01:2" -> key="ING01", required=2)
            if (key.Contains(':'))
            {
                string[] parts = key.Split(':');
                if (parts.Length == 2)
                {
                    key = parts[0].Trim();
                    int.TryParse(parts[1].Trim(), out requiredAmount);
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                if (!progressDict.ContainsKey(key))
                {
                    progressDict.Add(key, 0);
                    targetDict.Add(key, requiredAmount); // 개별 목표치 저장
                }
            }
        }
    }

    // ▼▼▼ [수정] 완료 체크 로직 (개별 목표치 비교) ▼▼▼
    public bool CheckCompletion()
    {
        if (progressDict.Count == 0) return false;

        foreach (var kvp in progressDict)
        {
            // 해당 키의 목표치 가져오기 (없으면 기본값)
            int required = targetDict.ContainsKey(kvp.Key) ? targetDict[kvp.Key] : targetFigure;

            if (kvp.Value < required) return false;
        }
        return true;
    }

    public string GetProgressString()
    {
        // 1. 단순 카운트/레벨형 (Count, Level)
        if (targetType == QuestTargetType.Count || targetType == QuestTargetType.Level)
        {
            int current = progressDict.Values.Sum();
            // 현재값과 목표값 중 작은 것을 표시 (초과 방지)
            int displayValue = (current > targetFigure) ? targetFigure : current;
            
            return $"{displayValue} / {targetFigure}";
        }
            
        // 2. 수집형/복합형 (Collect - 재료 등)
        List<string> status = new List<string>();
        foreach(var kvp in progressDict)
        {
            string displayName = kvp.Key;
            
            // ID -> 이름 변환
            if (GameDataManager.instance != null)
            {
                IngredientData ingData = GameDataManager.instance.GetIngredientDataById(kvp.Key);
                if (ingData != null) displayName = ingData.ingredientName;
            }

            // 개별 목표치 가져오기
            int required = targetDict.ContainsKey(kvp.Key) ? targetDict[kvp.Key] : targetFigure;
            int current = kvp.Value;

            // 표시용 값 계산 (현재값이 목표보다 크면 목표값으로 고정)
            int displayValue = (current > required) ? required : current;

            status.Add($"{displayName}: {displayValue}/{required}");
        }
        return string.Join("\n", status);
    }

    QuestType ParseQuestType(string s)
    {
        s = s.Trim().ToLower();
        if (s.Contains("main")) return QuestType.Main;
        if (s.Contains("daily")) return QuestType.Daily;
        if (s.Contains("vip")) return QuestType.VIP;
        return QuestType.Main;
    }

    QuestTargetType ParseTargetType(string s)
    {
        s = s.Trim().ToLower();
        if (s.Contains("collect")) return QuestTargetType.Collect;
        if (s.Contains("count")) return QuestTargetType.Count;
        if (s.Contains("level")) return QuestTargetType.Level;
        return QuestTargetType.None;
    }
}