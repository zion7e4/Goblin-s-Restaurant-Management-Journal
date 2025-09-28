using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum CustomerState { MovingToTable, WaitingForOrder, Eating, Leaving }
    public CustomerState currentState;

    private Transform targetTable;
    [SerializeField]
    private float speed = 3f;

    void Update()
    {
        switch (currentState)
        {
            case CustomerState.MovingToTable:
                // 테이블로 이동
                transform.position = Vector2.MoveTowards(transform.position, targetTable.position, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetTable.position) < 0.1f)
                {
                    currentState = CustomerState.WaitingForOrder;
                    targetTable.GetComponent<Table>().Occupy(gameObject); // 테이블 점유
                    Debug.Log("손님 착석, 주문 대기 중");
                }
                break;
            case CustomerState.Leaving:
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(0, -5), speed * Time.deltaTime);
                Destroy(gameObject, 2f);
                break;
        }
    }

    public void SetTable(Transform table)
    {
        targetTable = table;
        currentState = CustomerState.MovingToTable;
    }

    // 음식을 받으면 호출될 함수
    public void ReceiveFood()
    {
        currentState = CustomerState.Eating;
        StartCoroutine(EatAndLeave());
    }

    System.Collections.IEnumerator EatAndLeave()
    {
        Debug.Log("식사 시작");
        yield return new WaitForSeconds(2f); // 2초간 식사
        Debug.Log("식사 완료");
        GameManager.instance.AddGold(100);
        GameManager.instance.AddCustomerCount();
        targetTable.GetComponent<Table>().Vacate(); // 테이블 비우기
        targetTable.GetComponent<Table>().isDirty = true; // 테이블 더러운 상태로 변경
        currentState = CustomerState.Leaving;
        RestaurantManager.instance.customers.Remove(this);
    }
}
