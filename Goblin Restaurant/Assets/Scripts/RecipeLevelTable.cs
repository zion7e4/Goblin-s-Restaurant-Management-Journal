using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// '레시피 성장 테이블' 전체를 ScriptableObject 에셋으로 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "RecipeLevelTable", menuName = "Scriptable Objects/RecipeLevelTable")]
public class RecipeLevelTable : ScriptableObject
{
    // 1단계에서 만든 RecipeLevelEntry의 리스트를 가집니다.
    public List<RecipeLevelEntry> levelEntries;
}