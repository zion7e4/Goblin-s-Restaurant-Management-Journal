using UnityEngine;
using UnityEngine.UI;

public class UsedRecipeSlotUI : MonoBehaviour
{
    public Image recipeIcon;
    private TooltipTrigger tooltipTrigger;

    private Color lockedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Color unlockedColor = Color.white;

    public void Setup(RecipeData recipe, bool isUnlocked)
    {
        if (recipeIcon != null)
        {
            recipeIcon.sprite = recipe.icon;
            // 레시피 미보유 시 어둡게 처리
            recipeIcon.color = isUnlocked ? unlockedColor : lockedColor;
            recipeIcon.preserveAspect = true;
        }

        // 툴팁 설정 (커서 올리면 메뉴명 표시)
        tooltipTrigger = GetComponent<TooltipTrigger>();
        if (tooltipTrigger == null)
        {
            tooltipTrigger = gameObject.AddComponent<TooltipTrigger>();
        }
        
        if (isUnlocked)
        {
            // 해금된 경우: 레시피 이름 표시
            tooltipTrigger.SetTooltipText(recipe.recipeName); 
        }
        else
        {
            // 미해금인 경우: 물음표 표시
            tooltipTrigger.SetTooltipText("?????"); 
        }
    }
}