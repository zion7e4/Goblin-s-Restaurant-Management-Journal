using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public Image ingredientIcon;
    public TextMeshProUGUI ingredientNameText;
    public TextMeshProUGUI ingredientQuantityText;

    public void Setup(IngredientData data, int quantity)
    {
        ingredientIcon.sprite = data.icon;
        ingredientNameText.text = data.ingredientName;
        ingredientQuantityText.text = quantity.ToString();
    }
}
