using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClosePopupInput : MonoBehaviour, IPointerClickHandler
{
    public GameObject RecipeBookPanel;
    public GameObject ShopPanel;
    public GameObject InventoryPanel;
    public GameObject RecipeSelection;
    public GameObject MenuPlanner;
    public GameObject RecipeIngredientsPanel;
    public GameObject centralUpgradePanel; // 중앙 업그레이드 패널

    public GameObject PanelBlocker;
    public GameObject PopupManager;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.UI.ClosePopup.performed += OnClosePopup;
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.UI.ClosePopup.performed -= OnClosePopup;
        inputActions.UI.Disable();
    }

    private void OnClosePopup(InputAction.CallbackContext context)
    {
        TryCloseTopPopup();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 마우스 우클릭일 경우
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            TryCloseTopPopup();
        }
    }

    public void TryCloseTopPopup()
    {
        if (RecipeSelection != null && RecipeSelection.activeSelf)
        {
            RecipeSelection.SetActive(false);
            PanelBlocker.SetActive(false);
            return;
        }

        if (centralUpgradePanel != null && centralUpgradePanel.activeSelf)
        {
            // 컨트롤러의 OnCancel 함수를 호출하여 패널과 블로커를 모두 닫음
            centralUpgradePanel.GetComponent<UpgradePanelController>().OnCancel();
            return;
        }

        if (MenuPlanner != null && MenuPlanner.activeSelf)
        {
            MenuPlanner.SetActive(false);
            return;
        }

        if (ShopPanel != null && ShopPanel.activeSelf)
        {
            ShopPanel.SetActive(false);
            PanelBlocker.SetActive(false);
            return;
        }

        if (InventoryPanel != null && InventoryPanel.activeSelf)
        {
            InventoryPanel.SetActive(false);
            PanelBlocker.SetActive(false);
            return;
        }

        if (RecipeBookPanel != null && RecipeBookPanel.activeSelf)
        {
            RecipeBookPanel.SetActive(false);
            PanelBlocker.SetActive(false);
            return;
        }
        
        if (RecipeIngredientsPanel != null && RecipeIngredientsPanel.activeSelf)
        {
            RecipeIngredientsPanel.SetActive(false);
            return;
        }

        if (PopupManager != null && PopupManager.activeSelf)
        {
            PopupManager.SetActive(false);
            return;
        }
    }
}

