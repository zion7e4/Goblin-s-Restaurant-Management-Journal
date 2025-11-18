using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;
using Pathfinding;

public class Employee : MonoBehaviour
{
    private EmployeeInstance employeeData;

    [Header("직원 설정")]
    [Tooltip("음식 오브젝트가 붙을 손 위치")]
    public Transform handPosition;

    // A* 길찾기 관련 변수
    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;
    private float nextWaypointDistance = 0.5f; // 다음 경로 점으로 넘어갈 거리
    private Vector3 lastDestination = Vector3.zero;
    private float repathRate = 0.5f; // 경로 재계산 주기
    private float lastRepathTime = 0f;

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
    private Transform idlePosition;
    private KitchenOrder targetOrder;

    private Rigidbody2D rb;

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
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        if (rb == null)
        {
            Debug.LogError("Employee 프리팹에 Rigidbody 2D가 없습니다!");
        }
        if (seeker == null)
        {
            Debug.LogError("Employee 프리팹에 Seeker 컴포넌트가 없습니다!");
        }

        if (handPosition == null)
        {
            handPosition = this.transform;
        }

        if (employeeData == null)
        {
            currentState = EmployeeState.Idle;
        }
    }

    void Update()
    {
        // 상태에 따라 도착 여부 확인 및 행동 수행
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
                        CheckArrived(idlePosition.position, () => { currentState = EmployeeState.Idle; });
                    }
                }
                else
                {
                    currentState = EmployeeState.Idle;
                }
                break;
            case EmployeeState.MovingToCounterTop:
                CheckArrived(targetCountertop.transform.position, () => { StartCoroutine(CookFoodCoroutine()); });
                break;
            case EmployeeState.Cooking:
                break;
            case EmployeeState.MovingToPickupFood:
                CheckArrived(targetCountertop.transform.position, () => { StartCoroutine(PickupFoodCoroutine()); });
                break;
            case EmployeeState.MovingToServe:
                CheckArrived(targetCustomer.transform.position, ServeFood);
                break;
            case EmployeeState.CheckingTable:
                CheckTable();
                break;
            case EmployeeState.MovingToTable:
                CheckArrived(targetTable.transform.position, () => { StartCoroutine(CleaningTable()); });
                break;
            case EmployeeState.Cleaning:
                break;
        }
    }

    // 실제 물리 이동은 여기서 A* 알고리즘으로 처리
    void FixedUpdate()
    {
        // 이동해야 하는 상태가 아니면 정지
        if (currentState == EmployeeState.Idle ||
            currentState == EmployeeState.Cooking ||
            currentState == EmployeeState.Cleaning)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // 현재 상태에 맞는 목적지 가져오기
        Vector3 targetDest = GetTargetPositionByState();
        if (targetDest == Vector3.zero) return;

        // 경로 계산 요청 (목적지가 바뀌었거나 일정 시간이 지났을 때)
        if (Vector3.Distance(targetDest, lastDestination) > 0.1f || Time.time > lastRepathTime + repathRate)
        {
            lastRepathTime = Time.time;
            lastDestination = targetDest;
            if (seeker.IsDone())
            {
                seeker.StartPath(rb.position, targetDest, OnPathComplete);
            }
        }

        // 경로가 없으면 대기
        if (path == null) return;

        // 경로 끝에 도달했으면 정지
        if (currentWaypoint >= path.vectorPath.Count)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // 다음 웨이포인트 방향 계산 (normalized로 방향만 가져옴)
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        // 최종 이동 속도 계산
        float finalMoveSpeed = CalculateFinalSpeed();

        // Rigidbody로 이동
        if (rb != null)
        {
            rb.linearVelocity = direction * finalMoveSpeed;
        }

        // 웨이포인트 근접 체크 (다음 점으로 넘어가기)
        float distanceToWaypoint = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distanceToWaypoint < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    // Seeker가 경로 계산을 마치면 호출
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // 목표 지점에 도착했는지 확인하고 콜백 실행
    void CheckArrived(Vector3 destination, Action onArrived)
    {
        float dist = Vector2.Distance(transform.position, destination);
        // 도착 판정 거리 (약간 여유 있게 0.5f ~ 0.8f 권장)
        if (dist < 0.5f)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            path = null; // 경로 초기화
            onArrived?.Invoke();
        }
    }

    // 현재 상태에 따른 목적지 좌표 반환
    Vector3 GetTargetPositionByState()
    {
        switch (currentState)
        {
            case EmployeeState.MovingToIdle:
                return idlePosition != null ? idlePosition.position : Vector3.zero;
            case EmployeeState.MovingToCounterTop:
            case EmployeeState.MovingToPickupFood:
                return targetCountertop != null ? targetCountertop.transform.position : Vector3.zero;
            case EmployeeState.MovingToServe:
                return targetCustomer != null ? targetCustomer.transform.position : Vector3.zero;
            case EmployeeState.MovingToTable:
                return targetTable != null ? targetTable.transform.position : Vector3.zero;
            default:
                return Vector3.zero;
        }
    }

    // 시너지 및 특성을 적용한 최종 이동 속도 계산
    float CalculateFinalSpeed()
    {
        float synergySpeedBonus = 0f;
        float traitSpeedBonus = 0f;

        if (SynergyManager.Instance != null)
        {
            synergySpeedBonus = SynergyManager.Instance.GetMoveSpeedMultiplier();
        }

        if (employeeData != null)
        {
            traitSpeedBonus = employeeData.GetTraitMoveSpeedMultiplier();
        }

        float finalSpeed = movespeed * (1.0f + synergySpeedBonus + traitSpeedBonus);
        return Mathf.Max(0.1f, finalSpeed);
    }

    void FindTask()
    {
        if (employeeData == null)
        {
            Debug.LogWarning("Employee.cs: employeeData가 null이라 FindTask를 실행할 수 없습니다.");
            return;
        }

        // 홀 담당: 서빙할 음식 찾기
        if (employeeData.assignedRole == EmployeeRole.Hall ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o =>
                    o != null &&
                    o.status == OrderStatus.ReadyToServe &&
                    o.cookedOnCounterTop != null
                );
            }

            if (targetOrder != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (홀) 서빙할 음식 발견!");
                targetOrder.status = OrderStatus.Completed;
                targetCustomer = targetOrder.customer;
                targetCountertop = targetOrder.cookedOnCounterTop;
                currentState = EmployeeState.MovingToPickupFood;
                return;
            }
        }

        // 주방 담당: 요리할 주문 찾기
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


        // 홀 담당: 청소할 테이블 찾기
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

        // 할 일이 없으면 대기 위치로 이동
        if (idlePosition != null && Vector2.Distance(transform.position, idlePosition.position) > 0.5f)
        {
            currentState = EmployeeState.MovingToIdle;
        }
    }

    IEnumerator CookFoodCoroutine()
    {
        currentState = EmployeeState.Cooking;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} {targetOrder.recipe.data.recipeName} 요리 시작");

        if (targetOrder.foodObject == null)
        {
            Debug.LogError($"[요리 오류] {targetOrder.recipe.data.recipeName}의 foodObject가 null입니다! 작업을 중단합니다.");
            if (targetCountertop != null) targetCountertop.isBeingUsed = false;
            currentState = EmployeeState.MovingToIdle;
            targetCustomer = null;
            targetCountertop = null;
            targetOrder = null;
            yield break;
        }

        targetOrder.foodObject.transform.position = targetCountertop.transform.position;
        targetOrder.foodObject.SetActive(true);

        float baseRecipeTime = 10f;
        if (targetOrder.recipe != null && targetOrder.recipe.data != null)
        {
            baseRecipeTime = targetOrder.recipe.data.baseCookTime;
        }
        else
        {
            Debug.LogError("CookFoodCoroutine: targetOrder.recipe.data가 null입니다!");
        }

        int baseCookingStat = employeeData.currentCookingStat;
        int bonusCookingStat_Synergy = 0;
        float speedBonusPercent_Synergy = 0f;
        float specificStatMultiplier_Trait = 0f;
        float allStatMultiplier_Trait = 0f;
        float workSpeedMultiplier_Trait = 0f;

        if (SynergyManager.Instance != null)
        {
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusCookingStat_Synergy = cookBonus;
            speedBonusPercent_Synergy = SynergyManager.Instance.GetCookingSpeedBonus(employeeData);
        }

        if (employeeData != null)
        {
            specificStatMultiplier_Trait = employeeData.GetTraitCookingStatMultiplier();
            allStatMultiplier_Trait = employeeData.GetTraitAllStatMultiplier();
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier();
        }

        float totalMultiplier = 1.0f + specificStatMultiplier_Trait + allStatMultiplier_Trait;
        int finalCookingStat = (int)((baseCookingStat + bonusCookingStat_Synergy) * totalMultiplier);
        float finalCookTime = baseRecipeTime / (1 + (finalCookingStat * 0.008f));

        finalCookTime = finalCookTime * (1.0f - speedBonusPercent_Synergy);
        finalCookTime = finalCookTime * (1.0f - workSpeedMultiplier_Trait);
        finalCookTime = Mathf.Max(0.5f, finalCookTime);

        Debug.Log($"[{employeeData.firstName}] 요리 시간: {finalCookTime:F1}s");

        yield return new WaitForSeconds(finalCookTime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} 요리 완성");

        float totalSaveChance = 0f;
        if (SynergyManager.Instance != null) { totalSaveChance += SynergyManager.Instance.GetIngredientSaveChance(); }
        if (employeeData != null) { totalSaveChance += employeeData.GetTraitSaveChance(); }
        if (totalSaveChance > 0 && UnityEngine.Random.Range(0f, 1f) < totalSaveChance)
        {
            Debug.Log($"[재료 절약!] {employeeData.firstName} 재료 절약 성공");
            if (GameManager.instance != null && targetOrder.recipe != null)
            {
                GameManager.instance.RefundIngredients(targetOrder.recipe.data);
            }
        }

        targetOrder.status = OrderStatus.ReadyToServe;
        targetOrder.cookedOnCounterTop = this.targetCountertop;
        currentState = EmployeeState.MovingToIdle;

        if (targetCountertop != null) targetCountertop.isBeingUsed = false;

        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;
    }

    void ServeFood()
    {
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 서빙 완료");
        if (targetCustomer != null)
        {
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

        if (targetCountertop != null) targetCountertop.isBeingUsed = false;
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

    IEnumerator CleaningTable()
    {
        currentState = EmployeeState.Cleaning;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} 테이블 청소 시작");

        float baseCleaningTime = 3f;
        int baseServingStat = employeeData.currentServingStat;
        int bonusServingStat = 0;
        float allStatMultiplier_Trait = 0f;
        float workSpeedMultiplier_Trait = 0f;

        if (SynergyManager.Instance != null)
        {
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusServingStat = serveBonus;
        }

        if (employeeData != null)
        {
            allStatMultiplier_Trait = employeeData.GetTraitAllStatMultiplier();
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier();
        }

        float totalMultiplier = 1.0f + allStatMultiplier_Trait;
        int finalServingStat = (int)((baseServingStat + bonusServingStat) * totalMultiplier);

        float finalCleaningTime = baseCleaningTime / (1 + (finalServingStat * 0.008f));
        finalCleaningTime = finalCleaningTime * (1.0f - workSpeedMultiplier_Trait);
        finalCleaningTime = Mathf.Max(0.5f, finalCleaningTime);

        Debug.Log($"[{employeeData.firstName}] 청소 시간: {finalCleaningTime:F1}s");

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

    IEnumerator PickupFoodCoroutine()
    {
        if (targetOrder.foodObject != null)
        {
            Debug.Log($"{employeeData?.firstName ?? "Worker"} 픽업 완료.");

            targetOrder.foodObject.transform.SetParent(handPosition);
            targetOrder.foodObject.transform.localPosition = Vector3.zero;

            if (targetCountertop != null)
            {
                targetCountertop.isBeingUsed = false;
            }

            targetOrder.cookedOnCounterTop = null;
            currentState = EmployeeState.MovingToServe;

            yield return null;
        }
        else
        {
            Debug.LogError($"[픽업 오류] foodObject가 null입니다!");

            if (targetCountertop != null)
            {
                targetCountertop.isBeingUsed = false;
            }
            targetOrder.cookedOnCounterTop = null;

            currentState = EmployeeState.MovingToIdle;
            targetCustomer = null;
            targetCountertop = null;
            targetOrder = null;

            yield break;
        }
    }
}