using UnityEngine;
using TMPro;

public class GameUIBinder : MonoBehaviour
{
    [Header("Main UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI totalGoldText;
    public GameObject preparePanel;
    public GameObject tooltipPanel; // 툴팁 패널도 여기서 재연결

    void Start()
    {
        // GameManager에게 새 UI들을 연결해줍니다.
        if (GameManager.instance != null)
        {
            GameManager.instance.timeText = timeText;
            GameManager.instance.dayText = dayText;
            GameManager.instance.totalGold = totalGoldText;
            GameManager.instance.PreparePanel = preparePanel;
            
            // UI 갱신 강제 호출 (값이 보이도록)
            GameManager.instance.AddGold(0); 
        }

        // TooltipSystem에게 새 패널을 연결해줍니다.
        if (TooltipSystem.instance != null)
        {
            TooltipSystem.instance.tooltipPanel = tooltipPanel;
            // 툴팁Rect도 다시 찾아야 할 수 있음
             // TooltipSystem.instance.tooltipRect = tooltipPanel.GetComponent<RectTransform>(); 
             // (TooltipSystem 변수가 private라면 public으로 바꾸거나 Set 함수 필요)
        }
    }
}