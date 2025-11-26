using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Button button;
    public GameObject selectionOutline; // 선택 시 표시될 테두리

    private IngredientData data;
    private RecipeBook_UI bookController;

    public void Setup(IngredientData ingredientData, int count, RecipeBook_UI controller)
    {
        data = ingredientData;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (countText != null) countText.text = $"x{count}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
       
            Debug.Log($"{data.ingredientName} 클릭됨 (연결된 UI 없음)");
        });
    }
    public void SetSelected(bool isSelected)
    {
        if (selectionOutline != null) selectionOutline.SetActive(isSelected);
    }
}