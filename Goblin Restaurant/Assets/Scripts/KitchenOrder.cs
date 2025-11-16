using UnityEngine;

public enum OrderStatus { Pending, Cooking, ReadyToServe, Completed }

[System.Serializable]
public class KitchenOrder
{
    public Customer customer;
    public PlayerRecipe recipe; // TempRecipe -> PlayerRecipe
    public OrderStatus status;
    public GameObject foodObject;

    //  요리가 완료된 카운터탑. 홀 직원이 픽업할 위치입니다.
    public CounterTop cookedOnCounterTop;

    public KitchenOrder(Customer cust, PlayerRecipe rec, GameObject food)
    {
        customer = cust;
        recipe = rec;
        status = OrderStatus.Pending;
        foodObject = food;
        cookedOnCounterTop = null; // 처음엔 비어있음
    }
}