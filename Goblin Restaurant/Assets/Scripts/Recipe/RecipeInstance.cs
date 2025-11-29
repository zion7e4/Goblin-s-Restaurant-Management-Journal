// 역할: 플레이어가 실제로 소유하고 성장시키는 개별 레시피의 '현재 상태'를 저장하는 데이터 클래스입니다.

[System.Serializable]
public class RecipeInstance
{
    /// <summary>
    /// 이 레시피의 원본 데이터(ScriptableObject)입니다.
    /// </summary>
    public RecipeData BaseData { get; private set; }

    /// <summary>
    /// 플레이어가 강화한 현재 레벨입니다.
    /// </summary>
    public int currentLevel;

    // --- 생성자 ---
    public RecipeInstance(RecipeData baseData)
    {
        BaseData = baseData;
        currentLevel = 1; // 모든 레시피는 1레벨부터 시작합니다.
    }

    /// <summary>
    /// 레시피를 강화하여 레벨을 1 올립니다.
    /// </summary>
    public void Upgrade()
    {
        // TODO: 최대 레벨 제한 로직 추가 (예: 10레벨)
        currentLevel++;
        UnityEngine.Debug.Log($"{BaseData.recipeName} 레시피가 {currentLevel}레벨로 강화되었습니다!");
    }
}
