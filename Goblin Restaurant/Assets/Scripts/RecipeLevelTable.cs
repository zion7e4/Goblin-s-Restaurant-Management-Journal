using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipeLevelTable", menuName = "Scriptable Objects/RecipeLevelTable")]
public class RecipeLevelTable : ScriptableObject
{
    public List<RecipeLevelEntry> levelEntries;

    [ContextMenu("40레벨 데이터 자동 생성")]
    void GenerateLevels()
    {
        // 리스트가 없으면 새로 만들고, 있으면 내용을 싹 지웁니다 (Reset)
        if (levelEntries == null) levelEntries = new List<RecipeLevelEntry>();
        else levelEntries.Clear();

        // 1레벨부터 40레벨까지 새로 채워 넣기
        for (int i = 1; i <= 40; i++)
        {
            levelEntries.Add(new RecipeLevelEntry
            {
                Level = i,

                // [비용 공식] 레벨 * 30 골드 (저렴해짐)
                Required_Gold = i * 30,

                // [재료 공식] 5레벨마다 재료 1개씩 증가
                Required_Item_Count = 1 + (i / 5),

                // [가격 공식] 판매 가격 10%씩 상승
                Price_Growth_Rate = 0.1f * i,

                // [확률 공식] 레벨당 2%씩 감소 (최소 30% 보장)
                SuccessRate = Mathf.Max(30, 100 - (i * 2))
            });
        }

        Debug.Log("기존 데이터를 지우고, 새로운 가격표로 40레벨까지 갱신했습니다!");
    }
}