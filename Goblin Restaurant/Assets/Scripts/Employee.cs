using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Employee : MonoBehaviour
{
    public enum EmployeeState
    {
        Idle,
        MovingToCustomer,
        TakingOrder,
        MovingToCounterTop,
        Cooking,
        MovingToServe,
        MovingToIdle,
        CheckingTable,
        MovingToTable,
        Cleaning
    }

    public EmployeeState currentState;
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

    void Start()
    {
        currentState = EmployeeState.Idle;
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
                if (idlePosition != null)
                {
                    MoveTo(idlePosition.position, () => { currentState = EmployeeState.Idle; });
                }
                else
                {
                    currentState = EmployeeState.Idle;
                }
                break;
            case EmployeeState.MovingToCustomer:
                MoveTo(targetCustomer.transform.position, () => { currentState = EmployeeState.TakingOrder; });
                break;
            case EmployeeState.TakingOrder:
                TakeOrder();
                break;
            case EmployeeState.MovingToCounterTop:
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
                MoveTo(targetTable.transform.position, () => { StartCoroutine(CleaningTable()); });
                break;
            case EmployeeState.Cleaning:
                break;
        }
    }

    // 할 일 찾기
    void FindTask()
    {
        targetCustomer = RestaurantManager.instance.customers.FirstOrDefault(c => c.currentState == Customer.CustomerState.WaitingForOrder);
        if (targetCustomer != null)
        {
            currentState = EmployeeState.MovingToCustomer;
            return;
        }

        targetTable = RestaurantManager.instance.tables.FirstOrDefault(t => !t.isOccupied && t.isDirty);
        if (targetTable != null)
        {
            currentState = EmployeeState.MovingToTable;
            return;
        }

        if (Vector2.Distance(transform.position, idlePosition.position) > 0.1f)
        {
            currentState = EmployeeState.MovingToIdle;
        }
    }

    // 주문 수주
    void TakeOrder()
    {
        Debug.Log("주문 수주");

        // 미사용 화구 찾기
        targetCountertop = RestaurantManager.instance.counterTops.FirstOrDefault(s => !s.isBeingUsed);

        if (targetCountertop != null)
        {
            targetCountertop.isBeingUsed = true; // 화구를 사용 상태로 변경
            currentState = EmployeeState.MovingToCounterTop;
        }

        else
        {
            // 만약 모든 화구가 사용 중이라면 잠시 대기 (Idle 상태로 돌아가 다시 탐색)
            currentState = EmployeeState.Idle;
        }
    }

    // 요리 코루틴
    IEnumerator CookFoodCoroutine()
    {
        currentState = EmployeeState.Cooking;
        Debug.Log("요리 시작");

        yield return new WaitForSeconds(cookingtime);

        Debug.Log("요리 완성");
        currentState = EmployeeState.MovingToServe;
    }

    // 음식 서빙
    void ServeFood()
    {
        Debug.Log("서빙 완료");

        targetCustomer.ReceiveFood(); // 손님에게 음식을 전달

        // 사용했던 자원들을 초기화
        targetCountertop.isBeingUsed = false;
        targetCustomer = null;
        targetCountertop = null;

        currentState = EmployeeState.MovingToIdle;
    }

    void CheckTable()
    {
        targetTable = RestaurantManager.instance.tables.FirstOrDefault(t => !t.isOccupied && t.isDirty);

        if (targetTable.isDirty && targetTable.isOccupied)
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
        Debug.Log("테이블 청소 시작");

        yield return new WaitForSeconds(1f); // 청소 시간

        Debug.Log("테이블 청소 완료");
        if (targetTable != null)
        {
            targetTable.isDirty = false;
        }
        targetTable = null;
        currentState = EmployeeState.MovingToIdle;
    }

    // 목표 지점까지 이동하고, 도착하면 지정된 행동(Action)을 실행하는 함수
    void MoveTo(Vector3 destination, System.Action onArrived)
    {
        transform.position = Vector2.MoveTowards(transform.position, destination, movespeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            onArrived?.Invoke();
        }
    }
}
