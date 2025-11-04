// 역할: 게임에 사용되는 모든 능력치의 종류(Enum)와 데이터 구조를 정의합니다.

// [핵심 1] 능력치 종류를 열거형(Enum)으로 정의합니다.
// 나중에 '요리 속도', '재료 손질' 등을 추가하고 싶으면 여기에 추가만 하면 됩니다.
public enum StatType
{
    // 요리 관련
    Cooking_Speed,      // 요리 속도
    Cooking_Quality,    // 요리 품질
    Ingredient_Preparation, // 재료 손질 (요리 품질에 보너스)
    Plating_Skill,      // 플레이팅 기술 (요리 등급에 최종 보정)

    // 서빙 관련
    Serving_Speed,      // 서빙 속도
    Movement_Speed,     // 이동 속도
    Cleaning_Speed,     // 테이블 정리 속도
    Order_Capacity,     // 처리 가능한 주문 개수

    // 아직 사용하지 않지만 미래를 위한 확장 예시
    Charisma,           // 매력 (팁 획득량 증가 등)
    Stamina,            // 체력

    // 보조 능력치 (다른 직원에게 영향)
    Leadership,         // 리더십 (주변 직원의 효율 증가)
    Teaching_Ability    // 교육 능력 (직원 성장 속도 증가)
}

// [핵심 2] 개별 능력치의 데이터 형식을 정의합니다.
// 이 구조체는 '능력치 종류'와 '기본값'을 한 쌍으로 묶어줍니다.
[System.Serializable]
public struct CharacterStat
{
    public StatType type;
    public int baseValue;
}

