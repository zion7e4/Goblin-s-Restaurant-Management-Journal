using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyMenuSlotUI : MonoBehaviour
{
    public int slotIndex;
    public GameObject dataGroup;
    public GameObject emptyGroup;
    public TextMeshProUGUI recipeNameText;
    public Image recipeIcon;
    public Button plusButton;
    public Button minusButton;
    public Button removeButton;
    public TextMeshProUGUI quantityText;

    private Button myButton;
    private PlayerRecipe myRecipe;
    private MenuPlannerUI_Controller controller;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnSlotClick);
        plusButton.onClick.AddListener(() => controller.ChangeRecipeQuantity(myRecipe, 1));
        minusButton.onClick.AddListener(() => controller.ChangeRecipeQuantity(myRecipe, -1));
        removeButton.onClick.AddListener(() => controller.RemoveRecipeFromDailyMenu(this));
    }

    public void SetData(PlayerRecipe recipe)
    {
        dataGroup.SetActive(true);
        emptyGroup.SetActive(false);
        recipeNameText.text = recipe.data.recipeName;
        recipeIcon.sprite = recipe.data.icon;
    }

    public void ClearData()
    {
        dataGroup.SetActive(false);
        emptyGroup.SetActive(true);
    }

    public void Initialize(MenuPlannerUI_Controller uiController)
    {
        controller = uiController;
        GetComponent<Button>().onClick.AddListener(OnSlotClick);
    }

    public void SetData(PlayerRecipe recipe, int quantity)
    {
        myRecipe = recipe;
        dataGroup.SetActive(true);
        emptyGroup.SetActive(false);
        recipeNameText.text = myRecipe.data.recipeName;
        recipeIcon.sprite = myRecipe.data.icon;
        quantityText.text = quantity.ToString();
    }

    void OnSlotClick()
    {
        if (controller != null)
        {
            controller.OpenRecipeSelectionPanel(this);
        }
    }
}
