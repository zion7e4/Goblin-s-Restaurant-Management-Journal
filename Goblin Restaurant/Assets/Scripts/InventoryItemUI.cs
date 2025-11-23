using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI rarityText;
    public Image rarityBackgroundImage; 
    public Button selectButton;

    private Color lockedColor = new Color(0.1f, 0.1f, 0.1f, 1f); 
    private Color unlockedColor = Color.white;

    private IngredientData myData;
    private int myCount;
    private InventoryUIController controller;
    private bool isUnlocked;

    public void Setup(IngredientData data, int count, InventoryUIController ctrl, Sprite rarityBgSprite, bool isDiscovered)
    {
        myData = data;
        myCount = count;
        controller = ctrl;
        
        isUnlocked = isDiscovered; 

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.color = isUnlocked ? unlockedColor : lockedColor;
        }

        if (nameText != null)
        {
            nameText.text = isUnlocked ? data.ingredientName : "?????";
        }

        if (quantityText != null)
        {
            quantityText.text = count.ToString();
        }

        // 등급 텍스트 및 배경 설정
        if (rarityText != null)
        {
            rarityText.text = data.rarity.ToKorean();
            rarityText.gameObject.SetActive(isUnlocked);
        }

        if (rarityBackgroundImage != null)
        {
            if (isUnlocked && rarityBgSprite != null)
            {
                rarityBackgroundImage.sprite = rarityBgSprite;
                rarityBackgroundImage.gameObject.SetActive(true);
            }
            else
            {
                rarityBackgroundImage.gameObject.SetActive(false);
            }
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    void OnClicked()
    {
        if (controller != null)
        {
            controller.OnItemSelected(myData, myCount, isUnlocked);
        }
    }
}