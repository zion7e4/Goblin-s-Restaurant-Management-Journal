using UnityEngine;
using UnityEngine.UI;

public class PlaceObjectButton : MonoBehaviour
{
    public int tablePrice = 100;
    public GameObject upgradeConfirmPanel;
    private Button myButton;

    private bool isPurchased = false;

    void Awake()
    {
        myButton = GetComponent<Button>();
    }

    void Update()
    {
        bool isPreparing = (GameManager.instance.currentState == GameManager.GameState.Preparing);

        bool shouldBeVisible = isPreparing && !isPurchased;

        if (gameObject.activeSelf != shouldBeVisible)
        {
            gameObject.SetActive(shouldBeVisible);
        }

        if (shouldBeVisible && myButton != null)
        {
            myButton.interactable = (GameManager.instance.totalGoldAmount >= tablePrice);
        }
    }


    public void ShowUpgradePanel()
    {
        if (upgradeConfirmPanel != null)
        {
            upgradeConfirmPanel.SetActive(true);
            upgradeConfirmPanel.transform.SetAsLastSibling();
        }
    }

    public void OnConfirmPurchase()
    {
        if (GameManager.instance.totalGoldAmount >= tablePrice)
        {
            GameManager.instance.AddTable(this.transform);

            isPurchased = true;

            if (upgradeConfirmPanel != null)
            {
                upgradeConfirmPanel.SetActive(false);
            }
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            if (upgradeConfirmPanel != null)
            {
                upgradeConfirmPanel.SetActive(false);
            }
        }
    }

    public void OnCancel()
    {
        if (upgradeConfirmPanel != null)
        {
            upgradeConfirmPanel.SetActive(false);
        }
    }
}