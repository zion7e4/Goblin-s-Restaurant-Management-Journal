using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem instance;
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private RectTransform tooltipRect;

    void Awake()
    {
        instance = this;
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // 툴팁 패널의 위치를 현재 마우스 커서 위치로 설정
            tooltipRect.position = Mouse.current.position.ReadValue();
        }
    }

    // 툴팁을 보여주는 함수
    public void Show(string content)
    {
        tooltipText.text = content;
        tooltipPanel.SetActive(true);
    }

    // 툴팁을 숨기는 함수
    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}