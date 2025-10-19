using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubMenuController : MonoBehaviour
{
    [Header("메인 버튼들")]
    public Button buttonStore;
    public Button buttonRecipe;
    public Button buttonInterior;
    public Button buttonEmployee;

    [Header("서브 메뉴 패널들")]
    public GameObject subMenuStore;
    public GameObject subMenuRecipe;
    public GameObject subMenuInterior;
    public GameObject subMenuEmployee;

    [Header("서브 메뉴 닫기 버튼들 (각 패널 내부)")]
    public Button closeButtonStore;
    public Button closeButtonRecipe;
    public Button closeButtonInterior;
    public Button closeButtonEmployee;

    [Header("화면 어둡게 가릴 블로커")]
    public GameObject blocker;

    private GameObject currentActiveSubMenu = null;

    private void Start()
    {
        // 메인 버튼 클릭 시 각각의 메뉴 열기
        buttonStore.onClick.AddListener(() => ShowSubMenu(subMenuStore));
        buttonRecipe.onClick.AddListener(() => ShowSubMenu(subMenuRecipe));
        buttonInterior.onClick.AddListener(() => ShowSubMenu(subMenuInterior));
        buttonEmployee.onClick.AddListener(() => ShowSubMenu(subMenuEmployee));

        // 닫기 버튼 이벤트
        closeButtonStore.onClick.AddListener(HideAllSubMenus);
        closeButtonRecipe.onClick.AddListener(HideAllSubMenus);
        closeButtonInterior.onClick.AddListener(HideAllSubMenus);
        closeButtonEmployee.onClick.AddListener(HideAllSubMenus);

        // 블로커 클릭 시 닫기
        if (blocker.TryGetComponent<Button>(out var blockerBtn))
            blockerBtn.onClick.AddListener(HideAllSubMenus);

        // 초기 상태
        HideAllSubMenus();
    }

    private void Update()
    {
        // ESC 키로 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 메뉴가 열려있을 때만 닫기
            if (currentActiveSubMenu != null)
                HideAllSubMenus();
        }
    }

    private void ShowSubMenu(GameObject targetMenu)
    {
        bool isSameMenu = (currentActiveSubMenu == targetMenu);

        HideAllSubMenus();

        if (!isSameMenu)
        {
            targetMenu.SetActive(true);
            currentActiveSubMenu = targetMenu;
            blocker.SetActive(true); // 블로커 켜기
        }
    }

    public void HideAllSubMenus()
    {
        subMenuStore.SetActive(false);
        subMenuRecipe.SetActive(false);
        subMenuInterior.SetActive(false);
        subMenuEmployee.SetActive(false);

        blocker.SetActive(false);
        currentActiveSubMenu = null;
    }
}
