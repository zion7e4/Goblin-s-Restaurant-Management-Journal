using System.Collections.Generic;
using UnityEngine;

public class RestaurantManager : MonoBehaviour
{
    public static RestaurantManager instance;

    public List<Customer> customers;
    public List<Table> tables;
    public List<CounterTop> counterTops;
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        customers = new List<Customer>();
    }
}
