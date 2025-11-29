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

    [Header("Pause Menu")]
    public PauseMenuController pauseMenu;

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
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            TryCloseTopPopup();
        }
    }

    public void TryCloseTopPopup()
    {
        if (pauseMenu != null && pauseMenu.gameObject.activeSelf)
        {
            pauseMenu.ClosePauseMenu();
            PanelBlocker.SetActive(false);
            return;
        }

        if (QuantityPopupPanel != null && QuantityPopupPanel.activeSelf)
        {
            QuantityPopupPanel.SetActive(false);
            return;
        }

        if (centralUpgradePanel != null && centralUpgradePanel.activeSelf)
        {
            centralUpgradePanel.GetComponent<UpgradePanelController>().OnCancel();
            return;
        }

        if (MenuPlanner != null && MenuPlanner.activeSelf)
        {
            MenuPlanner.SetActive(false);
            if(PanelBlocker != null) PanelBlocker.SetActive(false);
            return;
        }

        if (RecipeBookPanel != null && RecipeBookPanel.activeSelf)
        {
            RecipeBookPanel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return;
        }

        if (ShopPanel != null && ShopPanel.activeSelf)
        {
            ShopPanel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return;
        }

        if (InventoryPanel != null && InventoryPanel.activeSelf)
        {
            InventoryPanel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
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

        if (pauseMenu != null)
        {
            pauseMenu.OpenPauseMenu();
            PanelBlocker.SetActive(true);
            return;
        }
    }
}