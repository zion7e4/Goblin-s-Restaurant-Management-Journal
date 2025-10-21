using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SubMenuController : MonoBehaviour
{
    // [Header("메인 버튼들")] - 주석은 유지합니다.
    [Header("Main Buttons")]
    public Button buttonStore;
    public Button buttonRecipe;
    public Button buttonInterior;
    public Button buttonEmployee;

    // [Header("서브 메뉴 패널들")]
    [Header("Sub Menu Panels")]
    public GameObject subMenuStore;
    public GameObject subMenuRecipe;
    public GameObject subMenuInterior;
    public GameObject subMenuEmployee;

    // [Header("서브 메뉴 닫기 버튼들 (각 패널 내부)")]
    [Header("Close Buttons (Inside Panels)")]
    public Button closeButtonStore;
    public Button closeButtonRecipe;
    public Button closeButtonInterior;
    public Button closeButtonEmployee;

    // [Header("화면 어둡게 가릴 블로커")]
    [Header("Screen Blocker")]
    public GameObject blocker;

    private GameObject currentActiveSubMenu = null;

    // Null 체크를 위한 버튼 목록 생성 (Start()에서만 사용)
    private List<Button> allCloseButtons = new List<Button>();
    private List<Button> allMainButtons = new List<Button>();


    private void Awake()
    {
        // 모든 닫기 버튼을 리스트에 추가합니다. (Null이 아닌 버튼만)
        if (closeButtonStore != null) allCloseButtons.Add(closeButtonStore);
        if (closeButtonRecipe != null) allCloseButtons.Add(closeButtonRecipe);
        if (closeButtonInterior != null) allCloseButtons.Add(closeButtonInterior);
        if (closeButtonEmployee != null) allCloseButtons.Add(closeButtonEmployee);

        // 모든 메인 버튼을 리스트에 추가합니다. (Null이 아닌 버튼만)
        if (buttonStore != null) allMainButtons.Add(buttonStore);
        if (buttonRecipe != null) allMainButtons.Add(buttonRecipe);
        if (buttonInterior != null) allMainButtons.Add(buttonInterior);
        if (buttonEmployee != null) allMainButtons.Add(buttonEmployee);
    }


    private void Start()
    {
        // --- 메인 버튼 클릭 이벤트 할당 ---

        // Null 체크를 통해 오류 방지
        if (buttonStore != null)
            buttonStore.onClick.AddListener(() => ShowSubMenu(subMenuStore));
        if (buttonRecipe != null)
            buttonRecipe.onClick.AddListener(() => ShowSubMenu(subMenuRecipe));
        if (buttonInterior != null)
            buttonInterior.onClick.AddListener(() => ShowSubMenu(subMenuInterior));
        if (buttonEmployee != null)
            buttonEmployee.onClick.AddListener(() => ShowSubMenu(subMenuEmployee));

        // --- 닫기 버튼 이벤트 할당 (NullReferenceException 해결) ---
        // Awake에서 생성한 리스트를 사용하여 안정적으로 이벤트 연결
        foreach (Button closeBtn in allCloseButtons)
        {
            closeBtn.onClick.AddListener(HideAllSubMenus);
        }

        // --- 블로커 클릭 시 닫기 (Null 체크 강화) ---
        // TryGetComponent<Button> 대신 GetComponent<Button>()을 사용해도 되지만, TryGetComponent가 더 안전합니다.
        if (blocker != null && blocker.TryGetComponent<Button>(out var blockerBtn))
            blockerBtn.onClick.AddListener(HideAllSubMenus);

        // 초기 상태 설정
        HideAllSubMenus();
    }

    private void Update()
    {
        // InvalidOperationException 해결을 위해 Input Settings를 'Old Input Manager'로 설정해야 합니다.
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
        // 타겟 메뉴가 Null이면 실행하지 않음 (안정성 강화)
        if (targetMenu == null) return;

        bool isSameMenu = (currentActiveSubMenu == targetMenu);

        HideAllSubMenus();

        if (!isSameMenu)
        {
            targetMenu.SetActive(true);
            currentActiveSubMenu = targetMenu;

            // 블로커가 Null이 아닌 경우에만 켜기 (안정성 강화)
            if (blocker != null)
                blocker.SetActive(true);
        }
    }

    public void HideAllSubMenus()
    {
        // 각 서브 메뉴 패널이 Null이 아닌 경우에만 비활성화 (안정성 강화)
        if (subMenuStore != null) subMenuStore.SetActive(false);
        if (subMenuRecipe != null) subMenuRecipe.SetActive(false);
        if (subMenuInterior != null) subMenuInterior.SetActive(false);
        if (subMenuEmployee != null) subMenuEmployee.SetActive(false);

        // 블로커가 Null이 아닌 경우에만 비활성화 (안정성 강화)
        if (blocker != null) blocker.SetActive(false);

        currentActiveSubMenu = null;
    }
}