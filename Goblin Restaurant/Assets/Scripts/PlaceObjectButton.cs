using UnityEngine;
using UnityEngine.UI;

public class PlaceObjectButton : MonoBehaviour
{
    public int tablePrice = 100;


    private Button myButton;
    private bool isPurchased = false;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnButtonClick);
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

    public void OnButtonClick()
    {
        UpgradePanelController.instance.ShowPanel(this);
    }

    public void SetPurchased()
    {
        isPurchased = true;
        gameObject.SetActive(false); // 구매 후 버튼을 영구적으로 숨김
    }
}