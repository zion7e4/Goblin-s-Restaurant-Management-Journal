using UnityEngine;

public class ClosePopupInput : MonoBehaviour
{
    [Header("Game UI Panels")]
    public GameObject RecipeBookPanel;
    public GameObject ShopPanel;
    public GameObject InventoryPanel;
    public GameObject MenuPlanner;
    public GameObject RecipeIngredientsPanel;
    public GameObject centralUpgradePanel; 
    public GameObject QuantityPopupPanel;
    public GameObject recipeSubMenuPanel; // 추가

    [Header("Blockers")]
    public GameObject PanelBlocker;
    public GameObject PopupManager; 

    // 반환값: 닫은게 있으면 true
    public bool TryCloseTopPopup()
    {
        if (QuantityPopupPanel != null && QuantityPopupPanel.activeSelf)
        {
            QuantityPopupPanel.SetActive(false);
            return true;
        }
        if (recipeSubMenuPanel != null && recipeSubMenuPanel.activeSelf)
        {
            recipeSubMenuPanel.SetActive(false);
            if(PopupManager) PopupManager.SetActive(false);
            return true;
        }
        if (centralUpgradePanel != null && centralUpgradePanel.activeSelf)
        {
            var ctrl = centralUpgradePanel.GetComponent<UpgradePanelController>();
            if(ctrl) ctrl.OnCancel(); else centralUpgradePanel.SetActive(false);
            return true;
        }
        
        // 메인 패널들
        if (ClosePanelIfActive(MenuPlanner)) return true;
        if (ClosePanelIfActive(RecipeBookPanel)) return true;
        if (ClosePanelIfActive(ShopPanel)) return true;
        if (ClosePanelIfActive(InventoryPanel)) return true;
        if (ClosePanelIfActive(RecipeIngredientsPanel)) return true;
        
        // 팝업 매니저 (블로커)
        if (PopupManager != null && PopupManager.activeSelf)
        {
            PopupManager.SetActive(false);
            return true;
        }

        return false;
    }

    private bool ClosePanelIfActive(GameObject panel)
    {
        if (panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
            if (PanelBlocker != null) PanelBlocker.SetActive(false);
            return true;
        }
        return false;
    }
}