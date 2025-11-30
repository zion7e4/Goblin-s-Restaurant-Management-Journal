using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string tooltipContent;
    
    public void SetTooltipText(string content)
    {
        tooltipContent = content;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.instance.Show(tooltipContent);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.instance.Hide();
    }
}