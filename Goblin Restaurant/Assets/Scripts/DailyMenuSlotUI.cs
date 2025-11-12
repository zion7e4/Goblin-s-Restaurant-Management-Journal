using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyMenuSlotUI : MonoBehaviour
{
    [Header("데이터 그룹")]
    public GameObject dataGroup;
    public GameObject emptyGroup;

    [Header("UI 요소 (이미지 기반)")]
    public Image recipeIcon;

    public TextMeshProUGUI RecipeGradeText;
    public Image quantityIcon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;

    private PlayerRecipe myRecipe;

    private void Awake()
    {
    }

    public void ClearData()
    {
        myRecipe = null;
        dataGroup.SetActive(false);
        if (emptyGroup != null) emptyGroup.SetActive(true);
    }

    public void SetData(PlayerRecipe recipe, int quantity)
    {
        myRecipe = recipe;
        dataGroup.SetActive(true);
        if (emptyGroup != null) emptyGroup.SetActive(false);

        recipeIcon.sprite = myRecipe.data.icon;

        if (RecipeGradeText != null)
        {
            int grade = myRecipe.GetCurrentGrade();
            RecipeGradeText.text = $"요리 등급: {grade}";
        }

        if (quantityIcon != null)
        {
            quantityIcon.gameObject.SetActive(true);
        }
        quantityText.text = $"X {quantity}";

        if (priceText != null)
        {
            priceText.text = recipe.GetCurrentPrice().ToString();
        }
    }
}