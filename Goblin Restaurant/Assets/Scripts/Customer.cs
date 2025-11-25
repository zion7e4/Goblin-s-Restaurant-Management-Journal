using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class SatisfactionIcon
{
    public SatisfactionLevel level;
    public Sprite icon;
}

public class Customer : MonoBehaviour
{
    public enum CustomerState { MovingToTable, DecidingMenu, WaitingForFood, Eating, Leaving }
    public CustomerState currentState;

    public GameObject orderIconPrefab;
    public Transform iconSpawnPoint;
    public GameObject RestaurantReviwe;
    public List<SatisfactionIcon> satisfactionIcons;
    public Transform leavingPoint;

    private GameObject currentOrderIcon;
    private Transform targetTable;

    [SerializeField]
    private float speed = 3f;

    private PlayerRecipe myOrderedRecipe;
    private float foodWaitStartTime;
    private int satisfactionScore;
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
                    Destroy(gameObject);
                }
                break;
        }
    }

    // 직원이 음식을 서빙했을 때 호출
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

        // 모호성 해결을 위한 대기 시간
        yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

        var dailyMenu = MenuPlanner.instance.dailyMenu.Where(r => r != null);

        var availableMenuWithStock = dailyMenu
        .Where(r => MenuPlanner.instance.GetRemainingStock(r.data.id) > 0)
        .ToList();

        if (availableMenuWithStock.Count > 0)
        {
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

            // 음식 프리팹 생성 및 주문서 작성
            GameObject foodPrefab = myOrderedRecipe.data.foodPrefab;

            if (foodPrefab == null)
            {
                Debug.LogError($"[주문 오류!] {myOrderedRecipe.data.recipeName}의 레시피 데이터에 'foodPrefab'이 할당되지 않았습니다! 주문을 생성할 수 없습니다.");
                currentState = CustomerState.Leaving;
                yield break;
            }

            GameObject instantiatedFood = Instantiate(foodPrefab, Vector3.zero, Quaternion.identity);
            instantiatedFood.SetActive(false);
            instantiatedFood.name = $"{myOrderedRecipe.data.recipeName} (주문자: {this.name})";

            KitchenOrder newOrder = new KitchenOrder(this, myOrderedRecipe, instantiatedFood);
            RestaurantManager.instance.OrderQueue.Add(newOrder);

            MenuPlanner.instance.RecordSale(myOrderedRecipe.data.id);

            currentState = CustomerState.WaitingForFood;
            foodWaitStartTime = Time.time;
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

    IEnumerator EatAndLeave()
    {
        Debug.Log("식사 시작");
        yield return new WaitForSeconds(2f);
        Debug.Log("식사 완료");

        CalculateSatisfaction();
        SatisfactionLevel level = GetSatisfactionLevel();

        // [수정됨] 레시피 매니저를 통해 현재 레벨에 맞는 가격을 가져옴
        int price = RecipeManager.instance.GetRecipeSellingPrice(myOrderedRecipe.data.id);

        int tip = 0;

        // 만족도에 따른 명성 변화
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

        // 팁 계산 로직
        int baseCharmStat = 0;
        int bonusCharmStat_Synergy = 0;
        float traitTipBonus = 0f;
        float allStatMultiplier_Trait = 0f;

        if (serverEmployee != null)
        {
            baseCharmStat = serverEmployee.currentCharmStat;
            traitTipBonus = serverEmployee.GetTraitTipChanceBonus();
            allStatMultiplier_Trait = serverEmployee.GetTraitAllStatMultiplier();
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

        int totalPayment = price + tip;
        GameManager.instance.AddGold(totalPayment);

        // 리뷰(말풍선) 표시
        if (RestaurantReviwe != null && iconSpawnPoint != null)
        {
            GameObject textObj = Instantiate(RestaurantReviwe, iconSpawnPoint.position, Quaternion.identity);
            textObj.transform.SetParent(iconSpawnPoint);

            TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();
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
            case SatisfactionLevel.VerySatisfied: return "<color=#00FFFF>매우 만족!</color>";
            case SatisfactionLevel.Satisfied: return "<color=green>만족</color>";
            case SatisfactionLevel.Normal: return "보통";
            case SatisfactionLevel.Dissatisfied: return "<color=orange>불만</color>";
            case SatisfactionLevel.VeryDissatisfied: return "<color=red>매우 불만...</color>";
            default: return "";
        }
    }

    void CalculateSatisfaction()
    {
        satisfactionScore = 50; // 기본 점수

        float totalWaitTime = Time.time - foodWaitStartTime;
        if (totalWaitTime < 15f) satisfactionScore += 20;
        else if (totalWaitTime < 30f) satisfactionScore += 10;
        else if (totalWaitTime > 60f) satisfactionScore -= 20;
        else if (totalWaitTime > 45f) satisfactionScore -= 10;

        int dishGrade = myOrderedRecipe.GetCurrentGrade();
        // 1등급이 가장 좋은 등급(대가)이므로 점수를 많이 줌
        if (dishGrade == 1) satisfactionScore += 20;
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