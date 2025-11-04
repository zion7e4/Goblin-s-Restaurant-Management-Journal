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
    public float cookingtime = 5f;

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

        // EmployeeInstance 데이터에서 능력치를 가져와 설정
        // 요리 시간은 능력치에 반비례하여 설정 (최소 1초)
        cookingtime = Mathf.Max(1f, 5f - (data.currentCookingStat * 0.1f));

        // 디버그
        Debug.Log($"{data.firstName} 스폰 완료. Cooking Time: {cookingtime}s");

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

        // 직원 능력치(cookingtime) 반영
        yield return new WaitForSeconds(cookingtime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} 요리 완성");

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
            targetCustomer.ReceiveFood();
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

        // 직원 능력치(청소 능력) 반영이 필요
        yield return new WaitForSeconds(1f); // 청소 시간 고정 (나중에 능력치로 변경)

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
        transform.position = Vector2.MoveTowards(transform.position, destination, movespeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            onArrived?.Invoke();
        }
    }
}