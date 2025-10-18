using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Employee : MonoBehaviour
{
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

    void FindTask()
    {
        targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o => o.status == OrderStatus.Pending);

        if (targetOrder != null)
        {
            Debug.Log("요리할 주문 발견");
            targetCountertop = RestaurantManager.instance.counterTops.FirstOrDefault(s => !s.isBeingUsed);

            if (targetCountertop != null)
            {
                targetOrder.status = OrderStatus.Cooking;
                targetCountertop.isBeingUsed = true;
                currentState = EmployeeState.MovingToCounterTop;
            }


            return;
        }

        targetTable = RestaurantManager.instance.tables.FirstOrDefault(t => t != null && !t.isOccupied && t.isDirty && !t.isBeingUsedForCleaning);
        if (targetTable != null)
        {
            targetTable.isBeingUsedForCleaning = true;
            currentState = EmployeeState.MovingToTable;
            return; 
        }


        if (idlePosition != null && Vector2.Distance(transform.position, idlePosition.position) > 0.1f)
        {
            currentState = EmployeeState.MovingToIdle;
        }
    }

    // 요리 코루틴
    IEnumerator CookFoodCoroutine()
    {
        currentState = EmployeeState.Cooking;
        Debug.Log($"{targetOrder.recipe.data.recipeName} 요리 시작");

        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.position = targetCountertop.transform.position;
            targetOrder.foodObject.SetActive(true);
        }

        yield return new WaitForSeconds(targetOrder.recipe.data.baseCookTime);

        Debug.Log("요리 완성");

        targetOrder.status = OrderStatus.ReadyToServe;
        targetCustomer = targetOrder.customer;
        currentState = EmployeeState.MovingToServe;

        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.SetParent(this.transform);
            targetOrder.foodObject.transform.localPosition = new Vector3(0, 1.2f, 0);
        }
    }

    // 음식 서빙
    void ServeFood()
    {
        Debug.Log("서빙 완료");
        targetCustomer.ReceiveFood();

        if (targetOrder.foodObject != null)
        {
            Destroy(targetOrder.foodObject);
        }

        RestaurantManager.instance.OrderQueue.Remove(targetOrder);

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
            targetTable.isBeingUsedForCleaning = false;
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
