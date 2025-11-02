using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MenuPlannerUI_Controller : MonoBehaviour
{
    public GameObject recipeSelectionPanel;      //레시피 선택 팝업 패널
    public GameObject selectableRecipePrefab;    //팝업 목록에 표시될 레시피 아이템 프리팹
    public Transform selectableRecipeContent; //팝업 스크롤 뷰의 Content 오브젝트

    public List<DailyMenuSlotUI> dailyMenuSlots; //5개의 일일 메뉴 슬롯 UI 리스트

    private DailyMenuSlotUI currentEditingSlot;

    void Awake()
    {
        foreach (var slot in dailyMenuSlots)
        {
            slot.Initialize(this);
        }
    }

    public void OpenRecipeSelectionPanel(DailyMenuSlotUI slot)
    {
        currentEditingSlot = slot; 
        recipeSelectionPanel.SetActive(true);
        recipeSelectionPanel.transform.SetAsLastSibling();
        UpdateSelectableRecipeList();
        GameManager.instance.panelBlocker.SetActive(true);
    }

    public void OnRecipeSelectedFromPopup(PlayerRecipe selectedRecipe)
    {
        if (currentEditingSlot != null)
        {
            MenuPlanner.instance.SetDailyMenu(currentEditingSlot.slotIndex, selectedRecipe, 1);
        }

        UpdateAllSlotsUI(); 
        recipeSelectionPanel.SetActive(false);
        GameManager.instance.panelBlocker.SetActive(false);
    }

    public void RemoveRecipeFromDailyMenu(DailyMenuSlotUI slot)
    {
        MenuPlanner.instance.SetDailyMenu(slot.slotIndex, null, 0);
        UpdateAllSlotsUI();
    }

    public void ChangeRecipeQuantity(PlayerRecipe recipe, int amount)
    {
        if (recipe == null) return;

        int currentQuantity = MenuPlanner.instance.GetQuantity(recipe.data.id);
        int maxQuantity = InventoryManager.instance.GetMaxCookableAmount(recipe);
        int newQuantity = currentQuantity + amount;

        if (newQuantity >= 1 && newQuantity <= maxQuantity)
        {
            MenuPlanner.instance.SetQuantity(recipe.data.id, newQuantity);
            UpdateAllSlotsUI(); 
        }
    }

    private void UpdateSelectableRecipeList()
    {
        foreach (Transform child in selectableRecipeContent)
        {
            Destroy(child.gameObject);
        }

        var alreadyAddedIDs = MenuPlanner.instance.dailyMenu.Where(r => r != null).Select(r => r.data.id);

        foreach (var playerRecipe in RecipeManager.instance.playerRecipes.Values)
        {
            if (!alreadyAddedIDs.Contains(playerRecipe.data.id))
            {
                bool canCook = InventoryManager.instance.CanCook(playerRecipe);

                GameObject itemGO = Instantiate(selectableRecipePrefab, selectableRecipeContent);
                itemGO.GetComponent<SelectableRecipeItemUI>().Setup(playerRecipe, canCook, this);
            }
        }
    }

    public void UpdateAllSlotsUI() // public으로 변경
    {
        for (int i = 0; i < dailyMenuSlots.Count; i++)
        {
            PlayerRecipe recipe = MenuPlanner.instance.dailyMenu[i];
            if (recipe != null)
            {
                int quantity = MenuPlanner.instance.GetQuantity(recipe.data.id);
                dailyMenuSlots[i].SetData(recipe, quantity);

                // int max = InventoryManager.instance.GetMaxCookableAmount(recipe); // InventoryManager 필요
                int max = 99; // 임시 최대 수량
                if (dailyMenuSlots[i].plusButton != null) dailyMenuSlots[i].plusButton.interactable = (quantity < max);
                if (dailyMenuSlots[i].minusButton != null) dailyMenuSlots[i].minusButton.interactable = (quantity > 1);
            }
            else
            {
                dailyMenuSlots[i].ClearData();
            }
        }

        bool canStartBusiness = MenuPlanner.instance.dailyMenu.Any(r => r != null);
        if (GameManager.instance != null)
        {
            // ★ '영업 시작' 버튼의 '상태'만 변경하도록 수정합니다.
            GameManager.instance.SetStartButtonInteractable(canStartBusiness);
        }
    }
}