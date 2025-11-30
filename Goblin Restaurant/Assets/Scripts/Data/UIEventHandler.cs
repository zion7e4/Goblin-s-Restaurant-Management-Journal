using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // SerializeField 제거 (자동으로 찾을 거니까)
    private RecipeBook_UI mainUI;

    void Start()
    {
        // 게임 시작할 때 내 부모들 중에 RecipeBook_UI가 있는지 찾아서 연결함
        mainUI = GetComponentInParent<RecipeBook_UI>();

        if (mainUI == null)
            Debug.LogError("UIEventHandler: 부모 오브젝트에서 RecipeBook_UI를 찾을 수 없습니다!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("마우스 들어옴!"); 
        if (mainUI != null) mainUI.ShowTooltip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (mainUI != null) mainUI.ShowTooltip(false);
    }
}