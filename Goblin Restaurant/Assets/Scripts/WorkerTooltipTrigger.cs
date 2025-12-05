using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마우스를 올렸을 때(Hover) 툴팁을 보여주기 위한 트리거 스크립트입니다.
/// </summary>
public class WorkerTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 툴팁에 표시할 직원 데이터
    public EmployeeInstance employeeData;

    // 마우스가 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EmployeeUI_Controller.Instance != null && employeeData != null)
        {
            //  인수를 1개만 전달합니다. 
            // (위치는 EmployeeUI_Controller의 Update문에서 마우스를 따라가도록 처리되어 있습니다.)
            EmployeeUI_Controller.Instance.ShowWorkerTooltip(employeeData);
        }
    }

    // 마우스가 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (EmployeeUI_Controller.Instance != null)
        {
            // 컨트롤러에게 툴팁을 끄라고 요청
            EmployeeUI_Controller.Instance.HideWorkerTooltip();
        }
    }
}
