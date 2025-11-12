using UnityEngine;
using System.Collections;
using System.Linq;
using System.Drawing; // V1
using TMPro;
using System.Collections.Generic; // V2
using UnityEngine.UI; // V2

// [V2] 만족도 아이콘을 위한 직렬화 클래스
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
    public Transform iconSpawnPoint;    // 아이콘이 표시될 머리 위 위치
    public GameObject RestaurantReviwe; // 만족도 표시 텍스트

    // [V2] 만족도 아이콘 리스트
    public List<SatisfactionIcon> satisfactionIcons;
    // [V2] 퇴장 지점 (V1의 exitPoint 대체)
    public Transform leavingPoint;

    private GameObject currentOrderIcon; // 현재 떠 있는 아이콘을 저장할 변수

    private Transform targetTable;
    [SerializeField]
    private float speed = 3f;
    private PlayerRecipe myOrderedRecipe;
    private float foodWaitStartTime; // 음식을 기다리기 시작한 시간
    private int satisfactionScore;   // 최종 만족도 점수
    private EmployeeInstance serverEmployee; // 나에게 서빙한 직원 (V1, V2 공통)

    // [V2] leavingPoint를 사용하도록 수정
    public void Initialize(Transform table, Transform exit)
    {
        targetTable = table;
        leavingPoint = exit; // V2
        currentState = CustomerState.MovingToTable;
    }

    void Update()
    {
        switch (currentState)
        {
            // [V2] 테이블의 'seatPosition'으로 이동하는 정교한 로직
            case CustomerState.MovingToTable:
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
                    targetTable.GetComponent<Table>().Occupy(gameObject); //
                    StartCoroutine(DecideMenuCoroutine());
                }
                break;

            // [V2] 하드코딩이 아닌 'leavingPoint'로 퇴장하는 로직
            case CustomerState.Leaving:
                if (leavingPoint == null)
                {
                    Debug.LogError("leavingPoint가 Customer 스크립트에 할당되지 않았습니다! (Initialize 함수 확인)");
                    Destroy(gameObject);
                    break;
                }

                transform.position = Vector2.MoveTowards(transform.position, leavingPoint.position, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, leavingPoint.position) < 0.1f)
                {
                    Destroy(gameObject); // V2 (도착 시 즉시 파괴)
                }
                break;
        }
    }

    /// <summary>
    /// Employee가 이 함수를 호출하여 음식을 전달합니다.
    /// </summary>
    /// <param name="server">음식을 가져다준 직원(Employee)의 데이터</param>
    public void ReceiveFood(EmployeeInstance server) // (V1, V2 공통)
    {
        // 서빙한 직원 정보 저장
        this.serverEmployee = server;

        if (currentOrderIcon != null)
        {
            Destroy(currentOrderIcon);
        }

        currentState = CustomerState.Eating;
        StartCoroutine(EatAndLeave());
    }

    // [V2] V1의 UnityEngine.Random에서 네임스페이스 제거 (기능 동일)
    IEnumerator DecideMenuCoroutine()
    {
        Debug.Log("손님이 메뉴를 고르는 중...");
        yield return new WaitForSeconds(Random.Range(2f, 5f)); // V2

        var dailyMenu = MenuPlanner.instance.dailyMenu.Where(r => r != null);

        var availableMenuWithStock = dailyMenu
        .Where(r => MenuPlanner.instance.GetRemainingStock(r.data.id) > 0)
        .ToList();

        if (availableMenuWithStock.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMenuWithStock.Count); // V2
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

            KitchenOrder newOrder = new KitchenOrder(this, myOrderedRecipe, null); // foodObject는 나중에 추가
            RestaurantManager.instance.OrderQueue.Add(newOrder);

            MenuPlanner.instance.RecordSale(myOrderedRecipe.data.id);

            currentState = CustomerState.WaitingForFood;

            // [V1] 음식 대기 시작 시간 기록 (V1, V2 모두 누락되어 있었으나 V1의 CalculateSatisfaction에서 사용되므로 추가)
            foodWaitStartTime = Time.time;
        }
        else
        {
            Debug.LogError("손님이 주문할 메뉴가 오늘의 메뉴에 하나도 편성되어 있지 않습니다!");
            currentState = CustomerState.Leaving;
        }
    }

    // (V1에서 '사용 안 함' 주석이 달린 ReceiveFood()는 제거됨)

    public void SetTable(Transform table)
    {
        targetTable = table;
        currentState = CustomerState.MovingToTable;
    }


    // [병합] V1의 팁 공식 + V2의 아이콘 UI
    System.Collections.IEnumerator EatAndLeave()
    {
        Debug.Log("식사 시작");
        yield return new WaitForSeconds(2f); // 2초간 식사
        Debug.Log("식사 완료");

        CalculateSatisfaction();
        SatisfactionLevel level = GetSatisfactionLevel();
        int price = myOrderedRecipe.GetCurrentPrice();

        // 팁은 0으로 초기화하고, 확률에 따라 별도 계산
        int tip = 0;

        // 만족도에 따른 '명성' 변화 (기존 로직 유지)
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

        // --- [V1] '매력' 기반 팁 공식 적용 (시너지, 특성 포함) --- 

        // 1. 나를 서빙한 직원의 스탯을 가져옵니다.
        int baseCharmStat = 0;
        int bonusCharmStat_Synergy = 0;
        float traitTipBonus = 0f; // "매혹" 특성 보너스
        float allStatMultiplier_Trait = 0f; // "주인공" 특성 보너스

        if (serverEmployee != null) // (ReceiveFood에서 이 변수가 설정되었어야 함)
        {
            baseCharmStat = serverEmployee.currentCharmStat;
            traitTipBonus = serverEmployee.GetTraitTipChanceBonus(); // "매혹"
            allStatMultiplier_Trait = serverEmployee.GetTraitAllStatMultiplier(); // "주인공"
        }

        // (시너지 보너스도 가져옴)
        if (SynergyManager.Instance != null && serverEmployee != null)
        {
            var (cook, serve, charm) = SynergyManager.Instance.GetStatBonuses(serverEmployee);
            bonusCharmStat_Synergy = charm;
        }

        // 2. 최종 매력 스탯 = (기본 + 시너지) * (1 + 주인공)
        int finalCharmStat = (int)((baseCharmStat + bonusCharmStat_Synergy) * (1.0f + allStatMultiplier_Trait));

        // 3. 기획서 공식으로 팁 확률 계산 
        float baseTipChance = 5f; // (기본 팁 확률 5%로 임시 설정)
        // (기본 + (최종 매력 스탯 * 0.3) + 매혹 특성)
        float finalTipChance = baseTipChance + (finalCharmStat * 0.3f) + traitTipBonus;
        finalTipChance = Mathf.Clamp(finalTipChance, 0f, 100f); // 0~100% 사이로 고정

        // 4. 팁 획득 시도
        if (UnityEngine.Random.Range(0f, 100f) < finalTipChance) // 모호성 해결
        {
            tip = Mathf.RoundToInt(price * 0.1f); // (예: 음식값의 10%)
            Debug.Log($"[팁 발생!] (최종스탯: {finalCharmStat}, 특성: {traitTipBonus}%) 보너스로 {tip}G 팁 획득! (확률: {finalTipChance:F1}%)");
        }
        // --- [V1 팁 공식 적용 완료] ---

        int totalPayment = price + tip;
        GameManager.instance.AddGold(totalPayment);

        // --- [V2] 만족도 아이콘 표시 로직 적용 ---
        if (RestaurantReviwe != null && iconSpawnPoint != null)
        {
            GameObject textObj = Instantiate(RestaurantReviwe, iconSpawnPoint.position, Quaternion.identity);
            textObj.transform.SetParent(iconSpawnPoint);

            TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();

            // V2: 아이콘 이미지 찾기
            Image iconImage = textObj.GetComponentInChildren<Image>();

            // V2: 만족도 레벨에 맞는 스프라이트 찾기
            Sprite spriteToUse = null;
            if (satisfactionIcons != null)
            {
                spriteToUse = satisfactionIcons.FirstOrDefault(s => s.level == level)?.icon;
            }

            // V2: 아이콘 이미지 적용
            if (iconImage != null && spriteToUse != null)
            {
                iconImage.sprite = spriteToUse;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false); // 맞는 아이콘 없으면 숨김
            }

            // V1/V2 공통: 텍스트 적용
            if (textMesh != null)
            {
                // 팁 텍스트가 0보다 클 때만 보이도록 수정
                string tipText = (tip > 0) ? $"\n(팁: {tip}G)" : "";
                textMesh.text = $"지불금액: {totalPayment}G{tipText}\n만족도: {GetSatisfactionString(level)}";
            }
        }
        // --- [V2 UI 로직 적용 완료] ---

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

    // [V1] 직원의 특성 보너스까지 계산하는 정교한 로직
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

        // --- 시너지로 인한 서비스 점수 보너스 적용 ---
        if (SynergyManager.Instance != null)
        {
            int serviceBonus = SynergyManager.Instance.GetServiceScoreBonus();
            satisfactionScore += serviceBonus; // "활기찬 식당"(+2) 또는 "공포의 홀"(-2)
            if (serviceBonus != 0)
            {
                Debug.Log($"[시너지] 서비스 점수 보너스 {serviceBonus}점 적용!");
            }
        }

        // --- [V1] 특성으로 인한 서비스 점수 보너스/페널티 적용 ---
        if (serverEmployee != null)
        {
            // "실수투성이"(-5) 또는 "긍정적"(+?) 특성 효과 적용
            int traitBonus = serverEmployee.GetTraitServiceScoreBonus();
            satisfactionScore += traitBonus;
            if (traitBonus != 0)
            {
                Debug.Log($"[특성] {serverEmployee.firstName}의 특성으로 서비스 점수 {traitBonus}점 적용!");
            }
        }

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