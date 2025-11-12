using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MenuIngredientUI : MonoBehaviour
{
    public Image ingredientIcon;
    public TextMeshProUGUI ingredientNameText;
    public TextMeshProUGUI stockText;
    public TextMeshProUGUI gradeText;

    public void UpdateData(IngredientData data, int owned, int required)
    {
        if (data != null)
        {
            ingredientIcon.sprite = data.icon;
            ingredientNameText.text = data.ingredientName;
            gradeText.text = data.rarity.ToString();
        }

        stockText.text = $"{owned} / {required}";

        if (owned < required)
        {
            stockText.color = Color.red;
        }
        else
        {
            stockText.color = Color.black;
        }
    }
}