using UnityEngine;

public class Table : MonoBehaviour
{
    // 이 테이블이 현재 손님에 의해 사용되고 있는지 여부
    public bool isOccupied = false;
    // 테이블의 청소 상태
    public bool isDirty = false;
    // 청소 중인지 여부
    public bool isBeingUsedForCleaning = false;

    // 현재 이 테이블에 앉아있는 손님 오브젝트를 저장할 변수
    public GameObject currentCustomer;
    public Transform seatPosition;

    public void Occupy(GameObject customer)
    {
        isOccupied = true;
        currentCustomer = customer;
    }

    public void Vacate()
    {
        isOccupied = false;
        currentCustomer = null;
    }
}
