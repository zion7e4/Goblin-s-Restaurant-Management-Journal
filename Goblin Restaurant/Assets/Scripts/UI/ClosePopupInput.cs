using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClosePopupInput : MonoBehaviour, IPointerClickHandler
{
    [Header("Game UI Panels")]
    public GameObject RecipeBookPanel;
    public GameObject ShopPanel;
    public GameObject InventoryPanel;
    public GameObject MenuPlanner;
    public GameObject RecipeIngredientsPanel;
    public GameObject centralUpgradePanel; 
    public GameObject QuantityPopupPanel;

    [Header("Blockers & Managers")]
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
        if (GameManager.instance != null && GameManager.instance.IsPauseMenuOpen)
        {
            GameManager.instance.ClosePauseMenu();
            return;
        }

        if (TryCloseTopPopup())
        {
            return; 
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.OpenPauseMenu();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            TryCloseTopPopup();
        }
    }

    public bool TryCloseTopPopup()
    {
        if (QuantityPopupPanel != null && QuantityPopupPanel.activeSelf)
        {
            QuantityPopupPanel.SetActive(false);
            return true; 
        }

        if (centralUpgradePanel != null && centralUpgradePanel.activeSelf)
        {
            centralUpgradePanel.GetComponent<UpgradePanelController>().OnCancel();
            return true;
        }

        if (MenuPlanner != null && MenuPlanner.activeSelf)
        {
            MenuPlanner.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return true;
        }

        if (RecipeBookPanel != null && RecipeBookPanel.activeSelf)
        {
            RecipeBookPanel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return true;
        }

        if (ShopPanel != null && ShopPanel.activeSelf)
        {
            ShopPanel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return true;
        }

        if (InventoryPanel != null && InventoryPanel.activeSelf)
        {
            InventoryPanel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return true;
        }
        
        if (RecipeIngredientsPanel != null && RecipeIngredientsPanel.activeSelf)
        {
            RecipeIngredientsPanel.SetActive(false);
            return true;
        }

        if (PopupManager != null && PopupManager.activeSelf)
        {
            PopupManager.SetActive(false);
            return true;
        }

        return false; 
    }
}