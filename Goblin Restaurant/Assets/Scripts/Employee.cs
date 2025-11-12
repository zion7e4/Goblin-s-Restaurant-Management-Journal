using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;

// 이 스크립트는 직원 프리팹(Prefab)에 붙여서 사용해야 합니다.
public class Employee : MonoBehaviour
{
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
    private Transform idlePosition; // 직원이 할 일이 없을 때 가만히 서 있을 위치
    private KitchenOrder targetOrder;

    // RestaurantManager에서 호출됨
    public void Initialize(EmployeeInstance data, Transform defaultIdlePosition)
    {
        this.employeeData = data;
        this.idlePosition = defaultIdlePosition;

        if (data.BaseData != null)
        {
            this.movespeed = data.BaseData.baseMoveSpeed;
        }
        else
        {
            Debug.LogWarning($"Initialize: {data.firstName}의 BaseData가 null입니다. 기본 이동속도(3f)를 사용합니다.");
        }

        Debug.Log($"{data.firstName} 초기화 완료. (이동 속도: {this.movespeed})");
        currentState = EmployeeState.Idle;
    }


    void Start()
    {
        if (employeeData == null)
        {
            currentState = EmployeeState.Idle;
        }
    }

    void Update()
    {
        // 상태에 따라 다른 행동을 수행
        switch (currentState)
        {
            case EmployeeState.Idle:
                FindTask();
                break;
            case EmployeeState.MovingToIdle:
                FindTask(); // (대기 위치로 가면서도 새 작업을 찾을 수 있음)

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
                MoveTo(targetCountertop.transform.position, () => { StartCoroutine(CookFoodCoroutine()); });
                break;
            case EmployeeState.Cooking:
                break;
            case EmployeeState.MovingToPickupFood:
                MoveTo(targetCountertop.transform.position, () => { StartCoroutine(PickupFoodCoroutine()); });
                break;
            case EmployeeState.MovingToServe:
                MoveTo(targetCustomer.transform.position, ServeFood);
                break;
            case EmployeeState.CheckingTable:
                CheckTable();
                break;
            case EmployeeState.MovingToTable:
                MoveTo(targetTable.transform.position, () => { StartCoroutine(CleaningTable()); });
                break;
            case EmployeeState.Cleaning:
                break;
        }
    }

    void FindTask()
    {
        // (방어 코드) 직원 데이터가 없으면 아무 작업도 찾지 않아야 합니다.
        if (employeeData == null)
        {
            Debug.LogWarning("Employee.cs: employeeData가 null이라 FindTask를 실행할 수 없습니다.");
            return;
        }

        // --- 1. '홀' 담당: 서빙할 '완성된 음식' 찾기 ---
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

                // (중요) 다른 직원이 이 주문을 못 채가게 하기 위해 상태 변경
                targetOrder.status = OrderStatus.Completed; // (임시로 'Completed'로 변경, 'Serving' 상태가 필요할 수 있음)

                targetCustomer = targetOrder.customer;
                targetCountertop = targetOrder.cookedOnCounterTop; // (픽업할 카운터 위치)

                currentState = EmployeeState.MovingToPickupFood;
                return;
            }
        }

        // --- 2. '주방' 담당: '요리할 주문' 찾기 ---
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
                    return;
                }
            }
        }


        // --- 3. '홀' 담당: '청소할 테이블' 찾기 ---
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
                return;
            }
        }


        // 4. 할 일이 없으면 대기 위치로 이동
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

        // --- !! 방어 코드 수정 1 !! ---
        // 요리하기 *전*에 foodObject가 유효한지(null이 아닌지) 확인합니다.
        if (targetOrder.foodObject == null)
        {
            Debug.LogError($"[요리 오류] {targetOrder.recipe.data.recipeName}의 foodObject가 null입니다! 작업을 중단합니다.");

            // 작업 중단 (카운터 사용 가능하게 풀기)
            if (targetCountertop != null) targetCountertop.isBeingUsed = false;

            // 상태 및 타겟 초기화
            currentState = EmployeeState.MovingToIdle;
            targetCustomer = null;
            targetCountertop = null;
            targetOrder = null;

            yield break; // 코루틴 즉시 정지
        }

        // --- 검증 통과 시, 요리 진행 ---
        targetOrder.foodObject.transform.position = targetCountertop.transform.position;
        targetOrder.foodObject.SetActive(true);

        // 1. 레시피에서 '기본 요리 시간'을 가져옵니다.
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
        float specificStatMultiplier_Trait = 0f; // "셰프" 특성 보너스
        float allStatMultiplier_Trait = 0f;      // "만능인" 특성 보너스
        float workSpeedMultiplier_Trait = 0f;    // "느림보/빠름" 특성 보너스

        if (SynergyManager.Instance != null)
        {
            // 2. 시너지 매니저에서 '스탯'과 '속도' 보너스를 받아옵니다.
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusCookingStat_Synergy = cookBonus;
            speedBonusPercent_Synergy = SynergyManager.Instance.GetCookingSpeedBonus(employeeData);
        }

        // 3. 직원 데이터에서 '특성' 보너스를 받아옵니다.
        if (employeeData != null)
        {
            specificStatMultiplier_Trait = employeeData.GetTraitCookingStatMultiplier(); // "셰프"
            allStatMultiplier_Trait = employeeData.GetTraitAllStatMultiplier();      // "만능인"
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier();    // "느림보/빠름"
        }

        // 4. 최종 스탯 = (기본 + 시너지) * (1 + 셰프 + 만능인)
        float totalMultiplier = 1.0f + specificStatMultiplier_Trait + allStatMultiplier_Trait;
        int finalCookingStat = (int)((baseCookingStat + bonusCookingStat_Synergy) * totalMultiplier);

        // 5. 완성된 스탯으로 '요리'에 따른 시간 계산
        float finalCookTime = baseRecipeTime / (1 + (finalCookingStat * 0.008f));

        // 6. '속도' 보너스(시너지)와 '작업 속도' 보너스(특성)를 추가로 적용
        finalCookTime = finalCookTime * (1.0f - speedBonusPercent_Synergy); // 시너지
        finalCookTime = finalCookTime * (1.0f - workSpeedMultiplier_Trait); // 특성

        finalCookTime = Mathf.Max(0.5f, finalCookTime); // (최소 0.5초 보장)

        Debug.Log($"[{employeeData.firstName}] 요리 시간 계산. " +
                    $"기본시간: {baseRecipeTime:F1}s, 스탯: {finalCookingStat} (기본 {baseCookingStat} + 시너지 {bonusCookingStat_Synergy}) * 특성(x{totalMultiplier}), " +
                    $"속도(시너지): {speedBonusPercent_Synergy * 100}%, 작업속도(특성): {workSpeedMultiplier_Trait * 100}%, " +
                    $"최종시간: {finalCookTime:F1}s");

        yield return new WaitForSeconds(finalCookTime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} 요리 완성");

        // --- 재료 절약 확률 계산 ---
        float totalSaveChance = 0f;
        if (SynergyManager.Instance != null) { totalSaveChance += SynergyManager.Instance.GetIngredientSaveChance(); }
        if (employeeData != null) { totalSaveChance += employeeData.GetTraitSaveChance(); }
        if (totalSaveChance > 0 && UnityEngine.Random.Range(0f, 1f) < totalSaveChance)
        {
            Debug.Log($"[재료 절약!] {employeeData.firstName}이(가) 요리 재료를 절약했습니다! (확률: {totalSaveChance * 100:F0}%)");
            if (GameManager.instance != null && targetOrder.recipe != null)
            {
                GameManager.instance.RefundIngredients(targetOrder.recipe.data);
            }
        }

        // --- '주방' 담당: 요리 완료 후 '대기' 상태로 ---

        // 1. 주문 상태를 '서빙 준비 완료'로 변경
        targetOrder.status = OrderStatus.ReadyToServe;

        // 2. '홀' 직원이 픽업할 수 있도록, 요리가 완료된 카운터 위치를 주문서에 저장
        targetOrder.cookedOnCounterTop = this.targetCountertop;

        // 3. 요리사는 음식을 놔두고 '대기' 상태로 돌아감
        currentState = EmployeeState.MovingToIdle;

        // 4. 음식(foodObject)은 카운터 위에 둡니다.

        // 5. 작업이 끝났으므로 타겟을 초기화합니다.
        if (targetCountertop != null) targetCountertop.isBeingUsed = false;

        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;
    }

    // 음식 서빙 (홀 직원이 픽업 후 호출)
    void ServeFood()
    {
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 서빙 완료");
        if (targetCustomer != null)
        {
            // Customer.ReceiveFood 함수에 이 직원 데이터(employeeData)를 전달합니다.
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

        // 사용했던 자원들 초기화
        if (targetCountertop != null) targetCountertop.isBeingUsed = false; // (픽업 시 사용한 카운터)
        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;

        currentState = EmployeeState.MovingToIdle;
    }

    void CheckTable()
    {
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
        float allStatMultiplier_Trait = 0f;      // "만능인" 특성 보너스
        float workSpeedMultiplier_Trait = 0f;    // "느림보/빠름" 특성 보너스

        // 3. 시너지 매니저에서 '서빙 보너스'를 받아옵니다.
        if (SynergyManager.Instance != null)
        {
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusServingStat = serveBonus;
        }

        // 4. 직원 데이터에서 '특성' 보너스를 받아옵니다.
        if (employeeData != null)
        {
            allStatMultiplier_Trait = employeeData.GetTraitAllStatMultiplier(); // "만능인"
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier(); // "느림보/빠름"
        }

        // 5. 최종 스탯 = (기본 서빙 + 시너지 보너스) * (1 + 만능인)
        float totalMultiplier = 1.0f + allStatMultiplier_Trait;
        int finalServingStat = (int)((baseServingStat + bonusServingStat) * totalMultiplier);

        // 6. 완성된 스탯으로 '청소'에 따른 시간 계산
        float finalCleaningTime = baseCleaningTime / (1 + (finalServingStat * 0.008f));

        // 7. '작업 속도' 보너스(특성)를 추가로 적용
        finalCleaningTime = finalCleaningTime * (1.0f - workSpeedMultiplier_Trait);

        finalCleaningTime = Mathf.Max(0.5f, finalCleaningTime); // (최소 0.5초 보장)

        Debug.Log($"[{employeeData.firstName}] 청소 시간 계산. " +
                    $"기본시간: {baseCleaningTime:F1}s, 서빙스탯: {finalServingStat} (기본 {baseServingStat} + 보너스 {bonusServingStat}) * 특성(x{totalMultiplier}), " +
                    $"작업속도(특성): {workSpeedMultiplier_Trait * 100}%, 최종시간: {finalCleaningTime:F1}s");

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

        // 1. 시너지 매니저에서 보너스 획득
        if (SynergyManager.Instance != null)
        {
            synergySpeedBonus = SynergyManager.Instance.GetMoveSpeedMultiplier();
        }

        // 2. 특성(Trait)에서 보너스 획득
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

    // (홀 직원이) 카운터에서 음식 픽업
    IEnumerator PickupFoodCoroutine()
    {
        // --- !! 방어 코드 수정 2 !! ---
        // 픽업하기 *전*에 foodObject가 유효한지(null이 아닌지) 확인합니다.
        if (targetOrder.foodObject != null)
        {
            // --- 픽업 성공 ---
            Debug.Log($"{employeeData?.firstName ?? "Worker"}이(가) {targetOrder.recipe.data.recipeName} 픽업 완료.");

            // 1. 카운터에 있던 음식(foodObject)을 이 직원(this.transform)의 자식으로 붙입니다.
            targetOrder.foodObject.transform.SetParent(this.transform);
            targetOrder.foodObject.transform.localPosition = new Vector3(0, 1.2f, 0);

            // 2. 픽업했으므로 카운터 사용 완료
            if (targetCountertop != null)
            {
                targetCountertop.isBeingUsed = false;
            }

            // 3. 주문서에서 '요리 완료된 카운터' 정보 제거 (이제 내가 들고 있으므로)
            targetOrder.cookedOnCounterTop = null;

            // 4. 이제 '서빙' 상태로 변경하고 손님에게 이동
            currentState = EmployeeState.MovingToServe;

            yield return null;
        }
        else
        {
            // --- 픽업 실패 (이 부분이 원래 에러가 나던 곳입니다) ---
            Debug.LogError($"[픽업 오류] {targetOrder.recipe.data.recipeName}의 foodObject가 null입니다! 작업을 중단합니다.");

            // 작업 중단 (카운터 사용 가능하게 풀기)
            if (targetCountertop != null)
            {
                targetCountertop.isBeingUsed = false;
            }
            targetOrder.cookedOnCounterTop = null;

            // 상태 및 타겟 초기화
            currentState = EmployeeState.MovingToIdle;
            targetCustomer = null;
            targetCountertop = null;
            targetOrder = null;

            yield break; // 코루틴 즉시 정지
        }
    }
}