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

    // TODO: 나중에 여기에 특성의 실제 효과를 정의하는 코드를 추가할 수 있습니다.
    // (예: public float cookingStatBonus;)
}
