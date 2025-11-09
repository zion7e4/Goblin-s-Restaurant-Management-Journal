using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;

// 이 스크립트가 맵에 스폰되는 직원 캐릭터에 붙어 일꾼 역할을 합니다.
public class Employee : MonoBehaviour
{
    [Tooltip("이 직원에 연결된 데이터 인스턴스")]
    private EmployeeInstance employeeData;

    public enum EmployeeState
    {
        Idle,
        MovingToCounterTop,
        Cooking,
        MovingToServe,
        MovingToPickupFood,
        MovingToIdle,
        CheckingTable,
        MovingToTable,
        Cleaning
    }

    public EmployeeState currentState;

    [SerializeField]
    private float movespeed = 3f;

    [SerializeField]
    private Customer targetCustomer;
    [SerializeField]
    private CounterTop targetCountertop;
    [SerializeField]
    private Table targetTable;
    [SerializeField]
    private Transform idlePosition; // 직원이 할 일이 없을 때 서 있을 위치
    private KitchenOrder targetOrder;

    // RestaurantManager에서 호출됨
    public void Initialize(EmployeeInstance data, Transform defaultIdlePosition)
    {
        this.employeeData = data;
        this.idlePosition = defaultIdlePosition;

        // 1. 종족 데이터(BaseData)에서 '기본 이동 속도'를 가져옵니다.
        if (data.BaseData != null)
        {
            this.movespeed = data.BaseData.baseMoveSpeed;
        }
        else
        {
            // (BaseData가 없는 경우 기본값 3f를 유지)
            Debug.LogWarning($"Initialize: {data.firstName}의 BaseData가 null입니다. 기본 이동속도(3f)를 사용합니다.");
        }

        // 디버그
        Debug.Log($"{data.firstName} 스폰 완료. (이동 속도: {this.movespeed})");

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
            case EmployeeState.MovingToPickupFood:
                // MoveTo 함수가 도착 시 PickupFoodCoroutine 시작
                MoveTo(targetCountertop.transform.position, () => { StartCoroutine(PickupFoodCoroutine()); });
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
        // (안전 장치) 직원의 데이터가 없으면 아무 작업도 찾지 않습니다.
        if (employeeData == null)
        {
            Debug.LogWarning("Employee.cs: employeeData가 null이라 FindTask를 실행할 수 없습니다.");
            return;
        }

        // --- 1. '홀' 역할: 최우선으로 '서빙할 음식' 찾기 ---
        // (역할이 '홀'이거나 '미지정'일 때만 서빙 작업을 찾음)
        if (employeeData.assignedRole == EmployeeRole.Hall ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                // (ReadyToServe 상태이고, cookedOnCounterTop이 할당된 주문 찾기)
                targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o =>
                    o != null &&
                    o.status == OrderStatus.ReadyToServe &&
                    o.cookedOnCounterTop != null
                );
            }

            if (targetOrder != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (홀) 서빙할 음식 발견!");

                // (중요) 다른 직원이 이 주문을 못 채가도록 즉시 상태 변경
                targetOrder.status = OrderStatus.Completed; // (임시로 'Completed'로 변경, 'Serving' 상태가 필요할 수도 있음)

                targetCustomer = targetOrder.customer;
                targetCountertop = targetOrder.cookedOnCounterTop; // (픽업할 카운터 위치)

                currentState = EmployeeState.MovingToPickupFood; // '픽업'하러 이동
                return; // 작업을 찾았으므로 종료
            }
        }

        // --- 2. '주방' 역할: '요리할 주문' 찾기 ---
        // (역할이 '주방'이거나 '미지정'일 때만 요리 작업을 찾음)
        if (employeeData.assignedRole == EmployeeRole.Kitchen ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o => o != null && o.status == OrderStatus.Pending);
            }

            if (targetOrder != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (주방) 요리할 주문 발견");
                targetCountertop = RestaurantManager.instance.counterTops.FirstOrDefault(s => s != null && !s.isBeingUsed);

                if (targetCountertop != null)
                {
                    targetOrder.status = OrderStatus.Cooking;
                    targetCountertop.isBeingUsed = true;
                    currentState = EmployeeState.MovingToCounterTop;
                    return; // 작업을 찾았으므로 종료
                }
            }
        }


        // --- 3. '홀' 역할: '청소할 테이블' 찾기 ---
        // (역할이 '홀'이거나 '미지정'일 때만 청소 작업을 찾음)
        if (employeeData.assignedRole == EmployeeRole.Hall ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                targetTable = RestaurantManager.instance.tables.FirstOrDefault(t =>
                    t != null && t.isDirty && !t.isBeingUsedForCleaning);
            }

            if (targetTable != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (홀) 청소할 테이블 발견");
                targetTable.isBeingUsedForCleaning = true;
                currentState = EmployeeState.MovingToTable;
                return; // 작업을 찾았으므로 종료
            }
        }


        // 4. 할 일 없으면 대기 위치로 이동
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

        // 1. 레시피의 '기본 요리 시간'을 가져옵니다.
        float baseRecipeTime = 10f; // (오류 시 기본값)
        if (targetOrder.recipe != null && targetOrder.recipe.data != null)
        {
            baseRecipeTime = targetOrder.recipe.data.baseCookTime;
        }
        else
        {
            Debug.LogError("CookFoodCoroutine: targetOrder.recipe.data가 null입니다!");
        }

        // --- 시너지 및 특성 보너스 스탯/속도 합산 ---
        int baseCookingStat = employeeData.currentCookingStat;
        int bonusCookingStat_Synergy = 0;
        float speedBonusPercent_Synergy = 0f;
        float statMultiplier_Trait = 0f; // "꼼꼼함" 특성 보너스 (예: 0.1)
        float workSpeedMultiplier_Trait = 0f; // "게으름" 특성 보너스 (예: -0.1)

        if (SynergyManager.Instance != null)
        {
            // 2. 시너지 매니저에게 '스탯'과 '속도' 보너스를 물어봅니다.
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusCookingStat_Synergy = cookBonus;
            speedBonusPercent_Synergy = SynergyManager.Instance.GetCookingSpeedBonus(employeeData);
        }

        // 3. 직원 데이터에게 '특성' 보너스를 물어봅니다.
        if (employeeData != null)
        {
            statMultiplier_Trait = employeeData.GetTraitCookingStatMultiplier(); // "꼼꼼함"
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier(); // "게으름"
        }

        // 4. 최종 스탯 = (기본 스탯 + 시너지 스탯) * (1 + 특성 배율)
        int finalCookingStat = (int)((baseCookingStat + bonusCookingStat_Synergy) * (1.0f + statMultiplier_Trait));

        // 5. 기획서 공식으로 '스탯'에 의한 시간 계산
        float finalCookTime = baseRecipeTime / (1 + (finalCookingStat * 0.008f));

        // 6. '속도' 보너스(시너지)와 '작업 속도' 보너스(특성)를 추가로 적용
        // (예: "게으름"(-0.1) 적용 시 1.0 - (-0.1) = 1.1 (시간 10% 증가))
        finalCookTime = finalCookTime * (1.0f - speedBonusPercent_Synergy); // 시너지
        finalCookTime = finalCookTime * (1.0f - workSpeedMultiplier_Trait); // 특성

        finalCookTime = Mathf.Max(0.5f, finalCookTime); // (최소 0.5초 보장)

        Debug.Log($"[{employeeData.firstName}] 요리 시간 계산. " +
                    $"기본시간: {baseRecipeTime:F1}s, 스탯: {finalCookingStat}, " +
                    $"속도(시너지): {speedBonusPercent_Synergy * 100}%, 작업속도(특성): {workSpeedMultiplier_Trait * 100}%, " +
                    $"최종시간: {finalCookTime:F1}s");

        // 계산된 최종 요리 시간만큼 대기
        yield return new WaitForSeconds(finalCookTime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} 요리 완성");

        // --- 식재료 절약 확률 적용 ---
        float totalSaveChance = 0f;
        if (SynergyManager.Instance != null) { totalSaveChance += SynergyManager.Instance.GetIngredientSaveChance(); }
        if (employeeData != null) { totalSaveChance += employeeData.GetTraitSaveChance(); }
        if (totalSaveChance > 0 && UnityEngine.Random.Range(0f, 1f) < totalSaveChance)
        {
            Debug.Log($"[식재료 절약!] {employeeData.firstName}이(가) 요리 재료를 절약했습니다! (확률: {totalSaveChance * 100:F0}%)");
            if (GameManager.instance != null && targetOrder.recipe != null)
            {
                GameManager.instance.RefundIngredients(targetOrder.recipe.data);
            }
        }

        // --- '주방' 역할: 요리 완료 후 '대기' 상태로 ---

        // 1. 주문 상태를 '서빙 준비 완료'로 변경
        targetOrder.status = OrderStatus.ReadyToServe;

        // 2. '홀' 직원이 픽업할 수 있도록, 요리가 완료된 카운터 위치를 주문서에 저장
        targetOrder.cookedOnCounterTop = this.targetCountertop;

        // 3. 요리사는 서빙하지 않고 '대기' 상태로 돌아감
        currentState = EmployeeState.MovingToIdle;

        // 4. 음식(foodObject)을 자신에게 붙이지 않고, 카운터 위에 둡니다.

        // 5. 작업이 끝났으므로 타겟을 비웁니다.
        if (targetCountertop != null) targetCountertop.isBeingUsed = false;

        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;
    }

    // 음식 서빙
    void ServeFood()
    {
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 서빙 완료");
        if (targetCustomer != null)
        {
            // Customer.ReceiveFood 함수에 내 직원 데이터(employeeData)를 전달합니다.
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
    IEnumerator CleaningTable()
    {
        currentState = EmployeeState.Cleaning;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 테이블 청소 시작");

        // --- 시너지 및 특성 보너스 스탯/속도 합산 ---

        // 1. '기본 청소 시간'을 설정합니다. (임시로 3초 설정)
        float baseCleaningTime = 3f;

        // 2. 이 직원의 '기본 서빙' 스탯을 가져옵니다.
        int baseServingStat = employeeData.currentServingStat;
        int bonusServingStat = 0;
        float workSpeedMultiplier_Trait = 0f; // "게으름" 특성 보너스 (예: -0.1)

        // 3. 시너지 매니저에게 '스탯 보너스'를 물어봅니다.
        if (SynergyManager.Instance != null)
        {
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusServingStat = serveBonus;
        }

        // 4. 직원 데이터에게 '특성' 보너스를 물어봅니다.
        if (employeeData != null)
        {
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier(); // "게으름"
        }

        // 5. 최종 스탯 = 기본 스탯 + 시너지 보너스
        int finalServingStat = baseServingStat + bonusServingStat;

        // 6. 기획서 공식으로 '스탯'에 의한 시간 계산
        float finalCleaningTime = baseCleaningTime / (1 + (finalServingStat * 0.008f));

        // 7. '작업 속도' 보너스(특성)를 추가로 적용
        finalCleaningTime = finalCleaningTime * (1.0f - workSpeedMultiplier_Trait);

        finalCleaningTime = Mathf.Max(0.5f, finalCleaningTime); // (최소 0.5초 보장)

        Debug.Log($"[{employeeData.firstName}] 청소 시간 계산. " +
                  $"기본시간: {baseCleaningTime:F1}s, 서빙스탯: {finalServingStat}, " +
                  $"작업속도(특성): {workSpeedMultiplier_Trait * 100}%, 최종시간: {finalCleaningTime:F1}s");

        // 계산된 최종 시간만큼 대기
        yield return new WaitForSeconds(finalCleaningTime);

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
        float synergySpeedBonus = 0f;
        float traitSpeedBonus = 0f;

        // 1. 시너지 매니저에서 보너스 획득 ("우울한 작업장" 등)
        if (SynergyManager.Instance != null)
        {
            synergySpeedBonus = SynergyManager.Instance.GetMoveSpeedMultiplier();
        }

        // 2. 특성(Trait)에서 보너스 획득 ("게으름" 등)
        if (employeeData != null)
        {
            traitSpeedBonus = employeeData.GetTraitMoveSpeedMultiplier();
        }

        // 3. (1.0f + 시너지 + 특성)을 곱하여 최종 속도 계산
        float finalMoveSpeed = movespeed * (1.0f + synergySpeedBonus + traitSpeedBonus);
        finalMoveSpeed = Mathf.Max(0.1f, finalMoveSpeed); // 속도가 0이 되지 않게 최소 0.1 보장

        // 4. 최종 속도를 적용하여 이동
        transform.position = Vector2.MoveTowards(transform.position, destination, finalMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            onArrived?.Invoke();
        }
    }

    // (새 코루틴) 음식을 픽업해서 손님에게 이동 시작
    IEnumerator PickupFoodCoroutine()
    {
        currentState = EmployeeState.Idle; // 임시로 Idle (혹시 버그 생길까봐)
        Debug.Log($"{employeeData?.firstName ?? "Worker"}이(가) {targetOrder.recipe.data.recipeName} 픽업 완료.");

        // 1. 카운터에 있던 음식(foodObject)을 이 직원(this.transform)의 자식으로 붙입니다.
        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.SetParent(this.transform);
            targetOrder.foodObject.transform.localPosition = new Vector3(0, 1.2f, 0);
        }
        else
        {
            Debug.LogError($"[픽업 오류] {targetOrder.recipe.data.recipeName}의 foodObject가 null입니다!");
        }

        // 2. 음식을 픽업했으니, 이 카운터는 더 이상 '요리 완료' 상태가 아님
        if (targetCountertop != null)
        {
            // (참고: 요리사가 isBeingUsed는 이미 false로 풀었음)
        }

        // 3. 주문서에서 '요리 완료된 카운터' 정보 제거 (이제 내가 들고 있으므로)
        targetOrder.cookedOnCounterTop = null;

        // 4. 이제 '서빙' 상태로 변경하고 손님에게 이동
        currentState = EmployeeState.MovingToServe;

        // (targetCustomer는 FindTask에서 이미 설정됨)
        yield return null;
    }
}