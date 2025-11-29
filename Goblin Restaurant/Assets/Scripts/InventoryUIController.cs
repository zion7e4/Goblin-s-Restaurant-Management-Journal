using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI 이동 설정")]
    public RectTransform listPanelRect;
    public Vector2 centerPosition = Vector2.zero;
    public Vector2 leftPosition = new Vector2(-350, 0);
    public float animationDuration = 0.3f;

    [Header("데이터 설정")]
    public List<RarityBackground> rarityBackgrounds;

    [Header("왼쪽: 재료 목록")]
    public GameObject inventoryItemPrefab; 
    public Transform inventoryContentParent; 

    [Header("오른쪽: 상세 정보 패널")]
    public GameObject detailPanel; 
    public Image detailIcon;
    public TextMeshProUGUI detailNameText;
    
    public TextMeshProUGUI detailRarityText;
    public Image detailRarityBackground;

    public TextMeshProUGUI detailDescriptionText; 
    public TextMeshProUGUI detailQuantityText;

    [Header("오른쪽: 사용되는 메뉴 목록")]
    public Transform usedRecipesContentParent; 
    public GameObject usedRecipeSlotPrefab;    

    private void OnEnable()
    {
        UpdateInventoryList();
        
        if(detailPanel != null) detailPanel.SetActive(false);
        if(listPanelRect != null) listPanelRect.anchoredPosition = centerPosition;
    }

    public void UpdateInventoryList()
    {
        foreach (Transform child in inventoryContentParent) Destroy(child.gameObject);

        var allIngredients = GameDataManager.instance.GetAllIngredientData(); 

        foreach (var data in allIngredients)
        {
            int count = 0;
            if (InventoryManager.instance.playerIngredients.ContainsKey(data.id))
            {
                count = InventoryManager.instance.playerIngredients[data.id];
            }

            bool isDiscovered = InventoryManager.instance.IsDiscovered(data.id);

            GameObject itemGO = Instantiate(inventoryItemPrefab, inventoryContentParent);
            InventoryItemUI itemUI = itemGO.GetComponent<InventoryItemUI>();
            
            Sprite bgSprite = GetRaritySprite(data.rarity);
            
            itemUI.Setup(data, count, this, bgSprite, isDiscovered);
        }
    }

    public void OnItemSelected(IngredientData data, int count, bool isUnlocked)
    {
        if (detailPanel != null) detailPanel.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(MovePanel(listPanelRect, leftPosition));

        if (detailIcon != null)
        {
            detailIcon.sprite = data.icon;
            detailIcon.color = isUnlocked ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f);

            detailIcon.preserveAspect = true;
        }

        if (detailNameText != null) detailNameText.text = isUnlocked ? data.ingredientName : "?????";
        
        if (detailRarityText != null) 
        {
            detailRarityText.text = isUnlocked ? data.rarity.ToKorean() : "???";
        }

        if (detailRarityBackground != null)
        {
            if (isUnlocked)
            {
                detailRarityBackground.gameObject.SetActive(true);
                detailRarityBackground.sprite = GetRaritySprite(data.rarity);
            }
            else
            {
                detailRarityBackground.gameObject.SetActive(false); 
            }
        }

        if (detailDescriptionText != null) 
            detailDescriptionText.text = isUnlocked ? data.description : "아직 발견하지 못한 재료입니다.";

        if (detailQuantityText != null) detailQuantityText.text = count.ToString();

        UpdateUsedRecipesList(data);
    }

    private Sprite GetRaritySprite(Rarity rarity)
    {
        if (rarityBackgrounds == null) return null;
        
        var item = rarityBackgrounds.FirstOrDefault(x => x.rarity == rarity);
        return item.backgroundSprite;
    }
    void UpdateUsedRecipesList(IngredientData currentIngredient)
    {
        if (usedRecipesContentParent == null || usedRecipeSlotPrefab == null) return;
        foreach (Transform child in usedRecipesContentParent) Destroy(child.gameObject);
        var allRecipes = GameDataManager.instance.GetAllRecipeData();
        foreach (var recipe in allRecipes)
        {
            bool isUsed = recipe.requiredIngredients.Any(req => req.ingredientID == currentIngredient.id);
            if (isUsed)
            {
                bool isRecipeUnlocked = RecipeManager.instance.playerRecipes.ContainsKey(recipe.id);
                GameObject slotObj = Instantiate(usedRecipeSlotPrefab, usedRecipesContentParent);
                UsedRecipeSlotUI slotUI = slotObj.GetComponent<UsedRecipeSlotUI>();
                slotUI.Setup(recipe, isRecipeUnlocked);
            }
        }
    }
    public void CloseDetailPanel()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(MovePanel(listPanelRect, centerPosition));
    }
    IEnumerator MovePanel(RectTransform target, Vector2 endPos)
    {
        if (target == null) yield break;
        float elapsedTime = 0;
        Vector2 startPos = target.anchoredPosition;
        while (elapsedTime < animationDuration)
        {
            target.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        target.anchoredPosition = endPos;
    }
    public void OpenInventory() { gameObject.SetActive(true); UpdateInventoryList(); }
    public void CloseInventory() { gameObject.SetActive(false); }
}