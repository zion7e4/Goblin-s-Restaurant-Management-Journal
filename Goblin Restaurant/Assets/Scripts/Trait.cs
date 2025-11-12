using UnityEngine;

// 특성 하나에 대한 정보를 담는 데이터 틀입니다.
[CreateAssetMenu(fileName = "New Trait", menuName = "GoblinChef/Trait Data")]
public class Trait : ScriptableObject
{
    [Tooltip("UI에 표시될 특성의 이름 (예: 성실함, 요리광)")]
    public string traitName;

    [TextArea(3, 5)] // 여러 줄로 설명을 편하게 입력할 수 있게 해줍니다.
    [Tooltip("이 특성에 대한 설명")]
    public string description;

    [Tooltip("이 특성이 긍정적인 효과(버프)이면 True, 부정적인 효과(디버프)이면 False")]
    public bool isPositive = true; // 기본값은 '긍정'으로 설정
    // TODO: 나중에 여기에 특성의 실제 효과를 정의하는 코드를 추가할 수 있습니다.
    // (예: public float cookingStatBonus;)

    [Header("Trait Effects")]
    [Tooltip("이 특성으로 인한 식재료 절약 확률 (예: 0.15 = 15%)")]
    public float ingredientSaveChance = 0f;

    [Tooltip("이 특성으로 인한 식재료 훔칠 확률 (예: 0.15 = 15%)")]
    public float ingredientStealChance = 0f;

    [Tooltip("이 특성으로 인한 요리 스탯 보너스 (예: 0.1 = +10%)")]
    public float cookingStatMultiplier = 0f;

    [Tooltip("이 특성으로 인한 이동 속도 배율 (예: 게으름 -0.1 = -10%)")]
    public float moveSpeedMultiplier = 0f;

    [Tooltip("이 특성으로 인한 작업 속도 배율 (예: 게으름 -0.1 = -10%, 작업 시간 10% 증가)")]
    public float workSpeedMultiplier = 0f;

    [Tooltip("이 특성으로 인한 서비스 점수 보너스/페널티 (예: -5)")]
    public int serviceScoreModifier = 0;

    [Tooltip("이 특성으로 인한 팁 획득 '확률' 보너스 (예: 10 = +10%)")]
    public float tipChanceBonus = 0f;

    [Tooltip("이 특성으로 인한 모든 스탯(요리,서빙,매력) 보너스 (예: 0.1 = +10%)")]
    public float allStatMultiplier = 0f;
}
