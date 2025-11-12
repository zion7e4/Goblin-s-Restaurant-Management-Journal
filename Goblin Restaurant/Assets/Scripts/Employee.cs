using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;

// 이 스크립트가 맵에 스폰되는 직원 캐릭터에 붙어 일꾼 역할을 합니다.
public class Employee : MonoBehaviour
{
    // [추가된 필드]
    [Tooltip("이 직원에 연결된 데이터 인스턴스")]
    private EmployeeInstance employeeData;

    public enum EmployeeState
    {
        Idle,
        MovingToCounterTop,
        Cooking,
        MovingToServe,
        MovingToIdle,
        CheckingTable,
        MovingToTable,
        Cleaning
    }

    public EmployeeState currentState;

    // [수정: private에서 public으로 변경]
    [SerializeField]
    private float movespeed = 3f;

    // 이 변수는 CookFoodCoroutine에서 'finalCookTime'으로 대체되었습니다.
    // public float cookingtime = 5f; 

    /*[SerializeField]
    private int cookingskill = 1; // 추후 기능 추가 예정*/
    [SerializeField]
    private Customer targetCustomer;
    [SerializeField]
    private CounterTop targetCountertop;
    [SerializeField]
    private Table targetTable;
    [SerializeField]
    private Transform idlePosition; // 직원이 할 일이 없을 때 서 있을 위치
    private KitchenOrder targetOrder;

    // [추가된 Initialize 함수] - RestaurantManager에서 호출됨
    public void Initialize(EmployeeInstance data, Transform defaultIdlePosition)
    {
        this.employeeData = data;
        this.idlePosition = defaultIdlePosition;

        // (낡은 요리 시간 계산 로직 제거)

        // 디버그
        Debug.Log($"{data.firstName} 스폰 완료. (요리 시간은 레시피에 따라 결정됩니다)");

        currentState = EmployeeState.Idle;
    }


    void Start()
    {
        // 만약 Initialize가 호출되지 않았다면 (기존에 맵에 있던 직원이라면)
        if (employeeData == null)
        {
            // 이 직원은 기존 맵에 있던 주인공 직원일 수 있으므로, 기본 상태로 시작
            currentState = EmployeeState.Idle;
        }
    }

    void Update()
    {
        // 상태에 따라 다른 행동을 실행
        switch (currentState)
        {
            case EmployeeState.Idle:
                FindTask();
                break;
            case EmployeeState.MovingToIdle:
                FindTask();

                if (idlePosition != null)
                {
                    if (currentState == EmployeeState.MovingToIdle)
                    {
                        MoveTo(idlePosition.position, () => { currentState = EmployeeState.Idle; });
                    }
                }
                else
                {
                    currentState = EmployeeState.Idle;
                }
                break;
            case EmployeeState.MovingToCounterTop:
                // MoveTo 함수가 도착 시 CookFoodCoroutine 시작
                MoveTo(targetCountertop.transform.position, () => { StartCoroutine(CookFoodCoroutine()); });
                break;
            case EmployeeState.Cooking:
                break;
            case EmployeeState.MovingToServe:
                MoveTo(targetCustomer.transform.position, ServeFood);
                break;
            case EmployeeState.CheckingTable:
                CheckTable();
                break;
            case EmployeeState.MovingToTable:
                // MoveTo 함수가 도착 시 CleaningTable 시작
                MoveTo(targetTable.transform.position, () => { StartCoroutine(CleaningTable()); });
                break;
            case EmployeeState.Cleaning:
                break;
        }
    }

    void FindTask()
    {
        // 1. 요리 주문 찾기
        if (RestaurantManager.instance != null)
        {
            targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o => o != null && o.status == OrderStatus.Pending);
        }

        if (targetOrder != null)
        {
            Debug.Log($"{employeeData?.firstName ?? "Worker"} 요리할 주문 발견");
            targetCountertop = RestaurantManager.instance.counterTops.FirstOrDefault(s => s != null && !s.isBeingUsed);

            if (targetCountertop != null)
            {
                // **경쟁 상태 방지 로직 필요:** 작업 할당 시 즉시 상태 변경 후 리스트 업데이트
                targetOrder.status = OrderStatus.Cooking;
                targetCountertop.isBeingUsed = true;
                currentState = EmployeeState.MovingToCounterTop;
            }
            // 작업 할당에 성공했으므로 종료
            return;
        }

        // 2. 청소할 테이블 찾기 (더럽고, 사용 중이지 않고, 청소 중이지 않은 테이블)
        if (RestaurantManager.instance != null)
        {
            targetTable = RestaurantManager.instance.tables.FirstOrDefault(t =>
                t != null && t.isDirty && !t.isBeingUsedForCleaning); // isOccupied는 청소와 무관
        }

        if (targetTable != null)
        {
            targetTable.isBeingUsedForCleaning = true;
            currentState = EmployeeState.MovingToTable;
            // 작업 할당에 성공했으므로 종료
            return;
        }


        // 3. 할 일 없으면 대기 위치로 이동
        if (idlePosition != null && Vector2.Distance(transform.position, idlePosition.position) > 0.1f)
        {
            currentState = EmployeeState.MovingToIdle;
        }
    }
    // 요리 코루틴
    IEnumerator CookFoodCoroutine()
    {
        currentState = EmployeeState.Cooking;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} {targetOrder.recipe.data.recipeName} 요리 시작");

        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.position = targetCountertop.transform.position;
            targetOrder.foodObject.SetActive(true);
        }

        // 1. 레시피의 '기본 요리 시간'을 가져옵니다. (RecipeData의 'Base Cook Time')
        float baseRecipeTime = 10f; // (오류 시 기본값)
        if (targetOrder.recipe != null && targetOrder.recipe.data != null)
        {
            // RecipeData.cs에 'baseCookTime' 변수가 있다고 가정 (스크린샷 확인)
            baseRecipeTime = targetOrder.recipe.data.baseCookTime;
        }
        else
        {
            Debug.LogError("CookFoodCoroutine: targetOrder.recipe.data가 null입니다!");
        }

        // 2. 이 직원의 '요리' 스탯을 가져옵니다.
        int cookingStat = employeeData.currentCookingStat;

        // 3. 기획서 공식으로 최종 요리 시간을 계산합니다.
        float finalCookTime = baseRecipeTime / (1 + (cookingStat * 0.008f));

        finalCookTime = Mathf.Max(0.5f, finalCookTime); // (최소 0.5초 보장)

        Debug.Log($"[{employeeData.firstName}] 요리 시간 계산 (기획서 공식). " +
                    $"기본시간: {baseRecipeTime:F1}s, 스탯: {cookingStat}, 최종시간: {finalCookTime:F1}s");

        // 계산된 최종 요리 시간만큼 대기
        yield return new WaitForSeconds(finalCookTime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} 요리 완성");

        // --- 식재료 절약 확률 적용 ---
        float totalSaveChance = 0f;

        // 1. "완벽주의 주방" 같은 시너지 보너스 획득 [cite: 110-112]
        if (SynergyManager.Instance != null)
        {
            totalSaveChance += SynergyManager.Instance.GetIngredientSaveChance(); // (예: 0.05)
        }

        // 2. (TODO) 직원의 "손재주" 특성 보너스 획득 
        // (EmployeeInstance.cs에 HasTrait("손재주") 같은 함수가 필요합니다)
        // if (employeeData.HasTrait("손재주")) { totalSaveChance += 0.15f; } // 15%

        // 3. 확률 실행
        if (totalSaveChance > 0 && UnityEngine.Random.Range(0f, 1f) < totalSaveChance)
        {
            Debug.Log($"[식재료 절약!] {employeeData.firstName}이(가) 요리 재료를 절약했습니다! (확률: {totalSaveChance * 100:F0}%)");
            // (TODO: GameManager.instance.RefundIngredients(targetOrder.recipe) 같은 재료 반환 함수 호출)
        }
        // --- 적용 완료 ---

        targetOrder.status = OrderStatus.ReadyToServe;
        targetCustomer = targetOrder.customer;
        currentState = EmployeeState.MovingToServe;

        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.SetParent(this.transform);
            targetOrder.foodObject.transform.localPosition = new Vector3(0, 1.2f, 0);
        }

        // 서빙 로직으로 이동
    }
    // 음식 서빙
    void ServeFood()
    {
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 서빙 완료");
        if (targetCustomer != null)
        {
            // [수정] Customer.ReceiveFood 함수에 내 직원 데이터(employeeData)를 전달합니다.
            targetCustomer.ReceiveFood(this.employeeData);
        }

        if (targetOrder.foodObject != null)
        {
            Destroy(targetOrder.foodObject);
        }

        if (RestaurantManager.instance != null && targetOrder != null)
        {
            RestaurantManager.instance.OrderQueue.Remove(targetOrder);
        }

        // 사용했던 자원들을 초기화
        if (targetCountertop != null) targetCountertop.isBeingUsed = false;
        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;

        currentState = EmployeeState.MovingToIdle;
    }

    void CheckTable()
    {
        // FindTask 로직에서 청소할 테이블을 찾았다고 가정
        if (targetTable != null && targetTable.isDirty)
        {
            currentState = EmployeeState.MovingToTable;
        }
        else
        {
            currentState = EmployeeState.Idle;
        }
    }

    // 테이블 청소
    // 테이블 청소
    IEnumerator CleaningTable()
    {
        currentState = EmployeeState.Cleaning;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 테이블 청소 시작");

        // ▼▼▼ [수정] '서빙' 스탯으로 '청소 시간' 계산 (기획서 공식) ▼▼▼

        // 1. '기본 청소 시간'을 설정합니다. (임시로 2초 설정)
        float baseCleaningTime = 2f;

        // 2. 이 직원의 '서빙' 스탯을 가져옵니다.
        int servingStat = employeeData.currentServingStat;

        // 3. 기획서 공식으로 최종 청소 시간을 계산합니다.
        float finalCleaningTime = baseCleaningTime / (1 + (servingStat * 0.008f));
        finalCleaningTime = Mathf.Max(0.5f, finalCleaningTime); // (최소 0.5초 보장)

        Debug.Log($"[{employeeData.firstName}] 청소 시간 계산. " +
                  $"기본시간: {baseCleaningTime:F1}s, 서빙스탯: {servingStat}, 최종시간: {finalCleaningTime:F1}s");

        // yield return new WaitForSeconds(1f); // <-- 이 줄 대신
        yield return new WaitForSeconds(finalCleaningTime); // <-- 이 줄을 사용

        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        Debug.Log($"{employeeData?.firstName ?? "Worker"} 테이블 청소 완료");
        if (targetTable != null)
        {
            targetTable.isDirty = false;
            targetTable.isBeingUsedForCleaning = false;
        }
        targetTable = null;
        currentState = EmployeeState.MovingToIdle;
    }

    // 목표 지점까지 이동하고, 도착하면 지정된 행동(Action)을 실행하는 함수
    void MoveTo(Vector3 destination, Action onArrived)
    {
        // 1. 시너지 매니저에서 현재 '이동 속도' 보너스 총합을 가져옵니다.
        // (예: "우울한 작업장"이 발동 중이면 -0.05f 반환)
        float synergySpeedBonus = 0f;
        if (SynergyManager.Instance != null)
        {
            synergySpeedBonus = SynergyManager.Instance.GetMoveSpeedMultiplier();
        }

        // 2. (1.0f + 보너스)를 곱하여 최종 속도 계산
        // (예: 1.0 + (-0.05) = 0.95 (즉 95% 속도))
        float finalMoveSpeed = movespeed * (1.0f + synergySpeedBonus);

        // 3. 최종 속도를 적용하여 이동
        transform.position = Vector2.MoveTowards(transform.position, destination, finalMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            onArrived?.Invoke();
        }
    }
}
