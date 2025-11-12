using UnityEngine;
using System.Collections;
using System.Linq;
using System.Drawing; // (참고: 이 using문은 UnityEngine.Color와 충돌할 수 있으니, 필요 없다면 지우는 것이 좋습니다)
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class SatisfactionIcon
{
    public SatisfactionLevel level; // (SatisfactionManager.cs에 정의됨)
    public Sprite icon;
}

public class Customer : MonoBehaviour
{
    public enum CustomerState { MovingToTable, DecidingMenu, WaitingForFood, Eating, Leaving }
    public CustomerState currentState;
    public GameObject orderIconPrefab;
    public Transform iconSpawnPoint;     // 아이콘이 표시될 머리 위 위치
    public GameObject RestaurantReviwe; // 만족도 표시 텍스트
    public List<SatisfactionIcon> satisfactionIcons; //
    public Transform leavingPoint; // 퇴장 지점
    private GameObject currentOrderIcon; // 현재 떠 있는 아이콘을 저장할 변수

    private Transform targetTable;
    [SerializeField]
    private float speed = 3f;
    private PlayerRecipe myOrderedRecipe;
    private float foodWaitStartTime; // 음식을 기다리기 시작한 시간
    private int satisfactionScore;   // 최종 만족도 점수
    private EmployeeInstance serverEmployee;

    public void Initialize(Transform table, Transform exit)
    {
        targetTable = table;
        leavingPoint = exit;
        currentState = CustomerState.MovingToTable;
    }

    void Update()
    {
        switch (currentState)
        {
            case CustomerState.MovingToTable:
                // ▼▼▼ [기능 1] 의자 위치로 이동 (Script 1) ▼▼▼
                Table tableComponent = targetTable.GetComponent<Table>();
                Vector3 targetPosition;

                if (tableComponent != null && tableComponent.seatPosition != null)
                {
                    targetPosition = tableComponent.seatPosition.position;
                }
                else
                {
                    Debug.LogWarning("테이블에 seatPosition이 할당되지 않았습니다. 테이블 중앙으로 이동합니다.");
                    targetPosition = targetTable.position;
                }

                transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                {
                    currentState = CustomerState.DecidingMenu;
                    targetTable.GetComponent<Table>().Occupy(gameObject);
                    StartCoroutine(DecideMenuCoroutine());
                }
                break;
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            case CustomerState.Leaving:
                // ▼▼▼ [기능 2] 퇴장 지점에서 파괴 (Script 1) ▼▼▼
                if (leavingPoint == null)
                {
                    Debug.LogError("leavingPoint가 Customer 스크립트에 할당되지 않았습니다! (Initialize 함수 확인)");
                    Destroy(gameObject);
                    break;
                }

                transform.position = Vector2.MoveTowards(transform.position, leavingPoint.position, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, leavingPoint.position) < 0.1f)
                {
                    Destroy(gameObject);
                }
                break;
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }
    }
    /// <summary>
    /// Employee가 이 함수를 호출하여 음식을 전달합니다.
    /// </summary>
    public void ReceiveFood(EmployeeInstance server)
    {
        this.serverEmployee = server;

        if (currentOrderIcon != null)
        {
            Destroy(currentOrderIcon);
        }

        currentState = CustomerState.Eating;
        StartCoroutine(EatAndLeave());
    }

    IEnumerator DecideMenuCoroutine()
    {
        Debug.Log("손님이 메뉴를 고르는 중...");
        // ▼▼▼ [기능 3] 모호성 해결 (Script 2) ▼▼▼
        yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

        var dailyMenu = MenuPlanner.instance.dailyMenu.Where(r => r != null);

        var availableMenuWithStock = dailyMenu
        .Where(r => MenuPlanner.instance.GetRemainingStock(r.data.id) > 0)
        .ToList();

        if (availableMenuWithStock.Count > 0)
        {
            // ▼▼▼ [기능 3] 모호성 해결 (Script 2) ▼▼▼
            int randomIndex = UnityEngine.Random.Range(0, availableMenuWithStock.Count);
            myOrderedRecipe = availableMenuWithStock[randomIndex];

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

            KitchenOrder newOrder = new KitchenOrder(this, myOrderedRecipe, null);
            RestaurantManager.instance.OrderQueue.Add(newOrder);

            MenuPlanner.instance.RecordSale(myOrderedRecipe.data.id);

            currentState = CustomerState.WaitingForFood;
        }
        else
        {
            Debug.LogError("손님이 주문할 메뉴가 오늘의 메뉴에 하나도 편성되어 있지 않습니다!");
            currentState = CustomerState.Leaving;
        }
    }


    public void SetTable(Transform table)
    {
        targetTable = table;
        currentState = CustomerState.MovingToTable;
    }


    System.Collections.IEnumerator EatAndLeave()
    {
        Debug.Log("식사 시작");
        yield return new WaitForSeconds(2f);
        Debug.Log("식사 완료");

        CalculateSatisfaction();
        SatisfactionLevel level = GetSatisfactionLevel();
        int price = myOrderedRecipe.GetCurrentPrice();

        int tip = 0;

        // 만족도에 따른 '명성' 변화 (공통)
        switch (level)
        {
            case SatisfactionLevel.VerySatisfied:
                FameManager.instance.AddFame(20f);
                break;
            case SatisfactionLevel.Satisfied:
                FameManager.instance.AddFame(10f);
                break;
            case SatisfactionLevel.Normal:
                FameManager.instance.AddFame(5f);
                break;
            case SatisfactionLevel.VeryDissatisfied:
                FameManager.instance.DecreaseFame(1f);
                break;
        }

        // ▼▼▼ [기능 4] 상세한 팁 계산 (Script 2) ▼▼▼
        // (참고: 이 로직이 작동하려면 EmployeeInstance.cs에 GetTrait... 함수들이 필요합니다)
        int baseCharmStat = 0;
        int bonusCharmStat_Synergy = 0;
        float traitTipBonus = 0f; // "매혹" 특성 보너스
        float allStatMultiplier_Trait = 0f; // "주인공" 특성 보너스

        if (serverEmployee != null)
        {
            baseCharmStat = serverEmployee.currentCharmStat;
            traitTipBonus = serverEmployee.GetTraitTipChanceBonus(); // "매혹"
            allStatMultiplier_Trait = serverEmployee.GetTraitAllStatMultiplier(); // "주인공"
        }

        if (SynergyManager.Instance != null && serverEmployee != null)
        {
            var (cook, serve, charm) = SynergyManager.Instance.GetStatBonuses(serverEmployee);
            bonusCharmStat_Synergy = charm;
        }

        int finalCharmStat = (int)((baseCharmStat + bonusCharmStat_Synergy) * (1.0f + allStatMultiplier_Trait));
        float baseTipChance = 5f;
        float finalTipChance = baseTipChance + (finalCharmStat * 0.3f) + traitTipBonus;
        finalTipChance = Mathf.Clamp(finalTipChance, 0f, 100f);

        if (UnityEngine.Random.Range(0f, 100f) < finalTipChance)
        {
            tip = Mathf.RoundToInt(price * 0.1f);
            Debug.Log($"[팁 발생!] (최종스탯: {finalCharmStat}, 특성: {traitTipBonus}%) 보너스로 {tip}G 팁 획득! (확률: {finalTipChance:F1}%)");
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        int totalPayment = price + tip;
        GameManager.instance.AddGold(totalPayment);

        if (RestaurantReviwe != null && iconSpawnPoint != null)
        {
            GameObject textObj = Instantiate(RestaurantReviwe, iconSpawnPoint.position, Quaternion.identity);
            textObj.transform.SetParent(iconSpawnPoint);

            TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();

            // ▼▼▼ [기능 5] 만족도 아이콘 표시 (Script 1) ▼▼▼
            Image iconImage = textObj.GetComponentInChildren<Image>();

            Sprite spriteToUse = null;
            if (satisfactionIcons != null)
            {
                spriteToUse = satisfactionIcons.FirstOrDefault(s => s.level == level)?.icon;
            }

            if (iconImage != null && spriteToUse != null)
            {
                iconImage.sprite = spriteToUse;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            if (textMesh != null)
            {
                string tipText = (tip > 0) ? $"\n(팁: {tip}G)" : "";
                textMesh.text = $"지불금액: {totalPayment}G{tipText}\n만족도: {GetSatisfactionString(level)}";
            }
        }
        Debug.Log($"만족도: {satisfactionScore} ({level}) | 음식값: {price}G + 팁: {tip}G = 총 {totalPayment}G 지불");

        GameManager.instance.AddCustomerCount();
        targetTable.GetComponent<Table>().Vacate();
        targetTable.GetComponent<Table>().isDirty = true;
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
        if (dishGrade >= 1) satisfactionScore += 20;
        else if (dishGrade == 2) satisfactionScore += 15;
        else if (dishGrade == 3) satisfactionScore += 10;
        else if (dishGrade == 4) satisfactionScore += 5;
        else if (dishGrade == 5) satisfactionScore += 0;

        if (RestaurantManager.instance.cleanliness >= 90) satisfactionScore += 10;
        else if (RestaurantManager.instance.cleanliness < 50) satisfactionScore -= 10;

        if (SynergyManager.Instance != null)
        {
            int serviceBonus = SynergyManager.Instance.GetServiceScoreBonus();
            satisfactionScore += serviceBonus;
            if (serviceBonus != 0)
            {
                Debug.Log($"[시너지] 서비스 점수 보너스 {serviceBonus}점 적용!");
            }
        }

        if (serverEmployee != null)
        {
            int traitBonus = serverEmployee.GetTraitServiceScoreBonus();
            satisfactionScore += traitBonus;
            if (traitBonus != 0)
            {
                Debug.Log($"[특성] {serverEmployee.firstName}의 특성으로 서비스 점수 {traitBonus}점 적용!");
            }
        }

        satisfactionScore = Mathf.Clamp(satisfactionScore, 0, 100);
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