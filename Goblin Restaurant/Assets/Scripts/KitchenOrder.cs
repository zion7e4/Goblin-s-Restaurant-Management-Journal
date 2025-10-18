using UnityEngine;

public enum OrderStatus { Pending, Cooking, ReadyToServe, Completed }

[System.Serializable]
public class KitchenOrder
{
    public Customer customer;
    public PlayerRecipe recipe; // TempRecipe -> PlayerRecipe
    public OrderStatus status;
    public GameObject foodObject;

    public KitchenOrder(Customer cust, PlayerRecipe rec, GameObject food)
    {
        customer = cust;
        recipe = rec;
        status = OrderStatus.Pending;
        foodObject = food;
    }
}
