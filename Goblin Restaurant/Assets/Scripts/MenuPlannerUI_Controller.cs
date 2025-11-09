using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MenuPlannerUI_Controller : MonoBehaviour
{
    [Header("팝업 패널")]
    [Tooltip("수량 조절 팝업 패널 (QuantityPopupController.cs)")]
    public GameObject quantityPopupPanel;
    private QuantityPopupController quantityPopup;

    [Header("오늘의 메뉴 (Left Scroll)")]
    [Tooltip("'오늘의 메뉴' 스크롤뷰의 Content Transform")]
    public Transform todayMenuContentParent;
    [Tooltip("'오늘의 메뉴'에 표시될 아이템 프리팹 (DailyMenuSlotUI.cs)")]
    public GameObject todayMenuItemPrefab;

    [Header("보유 레시피 (Right Grid)")]
    [Tooltip("보유 레시피 리스트의 부모 (Grid Layout Group)")]
    public Transform recipeListContentParent;
    [Tooltip("보유 레시피 아이템 프리팹 (OwnedRecipeitemUI.cs)")]
    public GameObject recipeListItemPrefab;

    private PlayerRecipe currentSelectedRecipe;

    private bool isInitialized = false;

    void Awake()
    {
        if (quantityPopupPanel != null)
        {
            quantityPopup = quantityPopupPanel.GetComponent<QuantityPopupController>();
        }
    }

    void Start()
    {
        RefreshAllUI();
        isInitialized = true;
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            RefreshAllUI();
        }
    }

    private void RefreshAllUI()
    {
        if (MenuPlanner.instance == null || RecipeManager.instance == null)
        {
            Debug.LogWarning("MenuPlannerUI: Managers가 아직 준비되지 않았습니다.");
            return;
        }

        UpdateTodayMenuUI();
        PopulateRecipeList();

        if (quantityPopupPanel != null)
        {
            quantityPopupPanel.SetActive(false);
        }
    }

    void PopulateRecipeList()
    {
        if (recipeListContentParent == null || recipeListItemPrefab == null) return;

        foreach (Transform child in recipeListContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var recipeData in GameDataManager.instance.GetAllRecipeData())
        {
            GameObject itemGO = Instantiate(recipeListItemPrefab, recipeListContentParent);

            OwnedRecipeItemUI itemUI = itemGO.GetComponent<OwnedRecipeItemUI>();

            itemUI.Setup(recipeData, this);
        }
    }

    public void UpdateTodayMenuUI()
    {
        if (todayMenuContentParent == null || todayMenuItemPrefab == null) return;

        foreach (Transform child in todayMenuContentParent)
        {
            Destroy(child.gameObject);
        }

        var dailyMenu = MenuPlanner.instance.dailyMenu;

        bool isEmpty = true; 

        for (int i = 0; i < dailyMenu.Length; i++)
        {
            PlayerRecipe recipe = dailyMenu[i];
            GameObject itemGO = Instantiate(todayMenuItemPrefab, todayMenuContentParent);
            DailyMenuSlotUI slotUI = itemGO.GetComponent<DailyMenuSlotUI>();

            if (recipe != null)
            {
                isEmpty = false;

                int quantity = MenuPlanner.instance.GetQuantity(recipe.data.id);
                slotUI.SetData(recipe, quantity);
            }
            else
            {
                slotUI.ClearData();
            }
        }

        bool canStartBusiness = !isEmpty;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetStartButtonInteractable(canStartBusiness);
        }
    }

    public void OpenQuantityPopup(PlayerRecipe recipe)
    {
        currentSelectedRecipe = recipe;

        if (quantityPopupPanel != null && quantityPopup != null)
        {
            quantityPopupPanel.SetActive(true);
            quantityPopupPanel.transform.SetAsLastSibling();
            quantityPopup.Show(recipe, this);
        }
    }

    public void OnConfirmQuantity(PlayerRecipe recipe, int quantity)
    {
        int emptySlotIndex = -1;
        for (int i = 0; i < MenuPlanner.instance.dailyMenu.Length; i++)
        {
            if (MenuPlanner.instance.dailyMenu[i] == null)
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex != -1)
        {
            MenuPlanner.instance.SetDailyMenu(emptySlotIndex, recipe, quantity);
        }
        else
        {
            NotificationController.instance.ShowNotification("오늘의 메뉴가 꽉 찼습니다!");
        }

        RefreshAllUI();
        CloseQuantityPopup();
    }

    public void CloseQuantityPopup()
    {
        if (quantityPopupPanel != null)
        {
            quantityPopupPanel.SetActive(false);
        }
    }

    public void RemoveRecipeFromDailyMenu(PlayerRecipe recipe)
    {
        if (recipe == null) return;

        for (int i = 0; i < MenuPlanner.instance.dailyMenu.Length; i++)
        {
            if (MenuPlanner.instance.dailyMenu[i] == recipe)
            {
                MenuPlanner.instance.SetDailyMenu(i, null, 0);
                break;
            }
        }

        RefreshAllUI();
    }
}