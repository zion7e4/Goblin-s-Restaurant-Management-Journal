using UnityEngine;

// static 클래스로 만들어야 합니다.
public static class RarityExtensions
{
    // Rarity Enum을 한글 문자열로 변환하는 확장 메서드
    public static string ToKorean(this Rarity rarity)
    {
        switch (rarity)
        { 
            case Rarity.common:      return "일반";
            case Rarity.Uncommon:    return "고급";
            case Rarity.Rare:        return "희귀"; // (만약 있다면)
            case Rarity.Legendary:   return "전설";
            default:                 return rarity.ToString(); // 예외 시 영어 그대로
        }
    }
}