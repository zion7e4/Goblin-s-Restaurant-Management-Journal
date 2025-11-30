using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanelController : MonoBehaviour
{
    public static UpgradePanelController instance;

    public GameObject panel;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    private PlaceObjectButton currentButton;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);

        panel.SetActive(false);
    }

    public void ShowPanel(PlaceObjectButton button)
    {
        currentButton = button; 

        messageText.text = $"테이블을 구매하시겠습니까?\n(비용: {currentButton.tablePrice} G)";

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        GameManager.instance.panelBlocker.SetActive(true);
    }

    private void OnConfirm()
    {
        if (GameManager.instance.totalGoldAmount >= currentButton.tablePrice)
        {
            GameManager.instance.AddTable(currentButton.transform);
            
            currentButton.SetPurchased();
            HidePanel();
        }
        else
        {
            Debug.Log("골드가 부족합니다!");
            HidePanel();
            NotificationController.instance.ShowNotification("골드가 부족합니다!");
        }
    }

    public void OnCancel()
    {
        HidePanel();
    }

    private void HidePanel()
    {
        panel.SetActive(false);
        GameManager.instance.panelBlocker.SetActive(false);
    }
}