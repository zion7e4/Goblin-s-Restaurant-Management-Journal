using UnityEngine;

// ��� (Page 8 ��� ǥ�ÿ�)
public enum Rarity { Common, Uncommon, Rare, Legendary }

[CreateAssetMenu(fileName = "Ingredient", menuName = "Game Data/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("��� ���� ����")]
    public string id;
    public string ingredientName;
    public Sprite icon;

    [Header("��� ��� �� ����")]
    public Rarity rarity; // ����� ��͵� (�Ϲ�, ����, ���, ����)
    public int buyPrice; // �������� ������ ���� ����

    [TextArea(3, 5)] // �ν����Ϳ��� ���� �ٷ� ���� ���� TextArea �Ӽ� ���
    public string description;
}