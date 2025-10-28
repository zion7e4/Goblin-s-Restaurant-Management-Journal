using UnityEngine;
using System.Collections;
using System.Linq;
using System.Drawing;
using TMPro;

public class Customer : MonoBehaviour
{
    public enum CustomerState { MovingToTable, DecidingMenu, WaitingForFood, Eating, Leaving }
    public CustomerState currentState;
    public GameObject orderIconPrefab; 
    public Transform iconSpawnPoint;   // 아이콘이 표시될 머리 위 위치
    public GameObject RestaurantReviwe; // 만족도 표시 텍스트
    private GameObject currentOrderIcon; // 현재 떠 있는 아이콘을 저장할 변수

    private Transform targetTable;
    private Transform exitPoint; // 퇴장 시 이동할 목표 지점
    [SerializeField]
    private float speed = 3f;
    private PlayerRecipe myOrderedRecipe;
    private float foodWaitStartTime; // 음식을 기다리기 시작한 시간
    private int satisfactionScore;   // 최종 만족도 점수

    public void Initialize(Transform table, Transform exit)
    {
        targetTable = table;
        exitPoint = exit;
        currentState = CustomerState.MovingToTable;
    }

    void Update()
    {
        switch (currentState)
        {
            case CustomerState.MovingToTable:
                // 테이블로 이동
                transform.position = Vector2.MoveTowards(transform.position, targetTable.position, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetTable.position) < 0.1f)
                {
                    currentState = CustomerState.DecidingMenu;
                    targetTable.GetComponent<Table>().Occupy(gameObject); // 테이블 점유

                    StartCoroutine(DecideMenuCoroutine()); // 메뉴 결정 코루틴 시작
                }
                break;
            case CustomerState.Leaving:
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(0, -5), speed * Time.deltaTime);
                Destroy(gameObject, 2f);
                break;
        }
    }

    IEnumerator DecideMenuCoroutine()
    {
        Debug.Log("손님이 메뉴를 고르는 중...");
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        var availableMenu = MenuPlanner.instance.dailyMenu.Where(r => r != null).ToList();

        if (availableMenu.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMenu.Count);
            myOrderedRecipe = availableMenu[randomIndex];

            Debug.Log($"{myOrderedRecipe.data.recipeName} 결정! 주방에 주문을 넣습니다.");

            if (orderIconPrefab != null && iconSpawnPoint != null)
            {
                currentOrderIcon = Instantiate(orderIconPrefab, iconSpawnPoint.position, Quaternion.identity);
                currentOrderIcon.transform.SetParent(iconSpawnPoint);
                OrderIconUI iconUI = currentOrderIcon.GetComponent<OrderIconUI>();
                if (iconUI != null)
                {
                    iconUI.SetIcon(myOrderedRecipe.data.icon);
                }
            }

            KitchenOrder newOrder = new KitchenOrder(this, myOrderedRecipe, null); // foodObject는 나중에 추가
            RestaurantManager.instance.OrderQueue.Add(newOrder);

            currentState = CustomerState.WaitingForFood;
        }
        else
        {
            Debug.LogError("손님이 주문할 메뉴가 오늘의 메뉴에 하나도 편성되어 있지 않습니다!");
        }
    }

    public void ReceiveFood()
    {
        if (currentOrderIcon != null)
        {
            Destroy(currentOrderIcon);
        }

        currentState = CustomerState.Eating;
        StartCoroutine(EatAndLeave());
    }

    public void SetTable(Transform table)
    {
        targetTable = table;
        currentState = CustomerState.MovingToTable;
    }


    System.Collections.IEnumerator EatAndLeave()
    {
        Debug.Log("식사 시작");
        yield return new WaitForSeconds(2f); // 2초간 식사
        Debug.Log("식사 완료");
        CalculateSatisfaction();
        SatisfactionLevel level = GetSatisfactionLevel();

        int price = myOrderedRecipe.GetCurrentPrice();
        float tipRate = 0f;

        switch (level)
        {
            case SatisfactionLevel.VerySatisfied:
                tipRate = 0.20f; // 20% 팁
                break;
            case SatisfactionLevel.Satisfied:
                tipRate = 0.10f; // 10% 팁
                break;
            case SatisfactionLevel.Normal:
                tipRate = 0.05f; // 5% 팁
                break;
        }

        int tip = (int)(price * tipRate);
        int totalPayment = price + tip;
        GameManager.instance.AddGold(totalPayment);

        if(RestaurantReviwe != null && iconSpawnPoint != null)
        {
            GameObject textObj = Instantiate(RestaurantReviwe, iconSpawnPoint.position, Quaternion.identity);
            textObj.transform.SetParent(iconSpawnPoint);
            TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();
            if(textMesh != null)
            {
                textMesh.text = $"지불금액: {totalPayment}G\n만족도: {GetSatisfactionString(level)}";
            }
        }
        Debug.Log($"만족도: {satisfactionScore} ({level}) | 음식값: {price}G + 팁: {tip}G = 총 {totalPayment}G 지불");

        GameManager.instance.AddCustomerCount();
        targetTable.GetComponent<Table>().Vacate(); // 테이블 비우기
        targetTable.GetComponent<Table>().isDirty = true; // 테이블 더러운 상태로 변경
        currentState = CustomerState.Leaving;
        RestaurantManager.instance.customers.Remove(this);
    }

    string GetSatisfactionString(SatisfactionLevel level)
    {
        switch (level)
        {
            case SatisfactionLevel.VerySatisfied: return "<color=cyan>매우 만족!</color>";
            case SatisfactionLevel.Satisfied: return "<color=green>만족</color>";
            case SatisfactionLevel.Normal: return "보통";
            case SatisfactionLevel.Dissatisfied: return "<color=orange>불만</color>";
            case SatisfactionLevel.VeryDissatisfied: return "<color=red>매우 불만...</color>";
            default: return "";
        }
    }

    void CalculateSatisfaction()
    {
        satisfactionScore = 50; // 기본 점수 50점 

        float totalWaitTime = Time.time - foodWaitStartTime;
        if (totalWaitTime < 15f) satisfactionScore += 20;
        else if (totalWaitTime < 30f) satisfactionScore += 10;
        else if (totalWaitTime > 60f) satisfactionScore -= 20;
        else if (totalWaitTime > 45f) satisfactionScore -= 10;

        int dishGrade = myOrderedRecipe.GetCurrentGrade();
        if (dishGrade >= 5) satisfactionScore += 20;
        else if (dishGrade == 4) satisfactionScore += 15;
        else if (dishGrade == 3) satisfactionScore += 5;
        else if (dishGrade == 1) satisfactionScore -= 20;

        if (RestaurantManager.instance.cleanliness >= 90) satisfactionScore += 10;
        else if (RestaurantManager.instance.cleanliness < 50) satisfactionScore -= 10;

        satisfactionScore = Mathf.Clamp(satisfactionScore, 0, 100); // 최종 점수를 0~100 사이로 고정
    }

    SatisfactionLevel GetSatisfactionLevel()
    {
        if (satisfactionScore <= 20) return SatisfactionLevel.VeryDissatisfied;
        if (satisfactionScore <= 40) return SatisfactionLevel.Dissatisfied;
        if (satisfactionScore <= 60) return SatisfactionLevel.Normal; 
        if (satisfactionScore <= 80) return SatisfactionLevel.Satisfied;
        return SatisfactionLevel.VerySatisfied;
    }
}

