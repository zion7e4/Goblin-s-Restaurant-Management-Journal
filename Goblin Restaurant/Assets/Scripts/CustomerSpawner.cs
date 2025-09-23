using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab; // 스폰할 고객 프리팹
    public List<Table> tables; // 가게 내 테이블 리스트
    public Transform spawnPoint; // 고객이 스폰될 위치

    public float spawnInterval = 5f; // 스폰 시도 간격
    private float spawnTimer;
    

    private void Update()
    {
        if (GameManager.instance.currentState != GameManager.GameState.Open)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnCustomer();
        }
    }

    void TrySpawnCustomer()
    {
        Table emptyTable = FindEmptyTable();

        if (emptyTable != null)
        {
            Debug.Log("빈 테이블 발견 고객 스폰");

            GameObject newCustomerObj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
            Customer newCustomer = newCustomerObj.GetComponent<Customer>();

            RestaurantManager.instance.customers.Add(newCustomer);

            newCustomer.SetTable(emptyTable.transform);
            emptyTable.isOccupied = true;
        }

        else
        {
            Debug.Log("모든 테이블이 꽉 찼음");
        }
    }

    Table FindEmptyTable()
    {
        foreach (Table table in tables)
        {
            if (!table.isOccupied)
            {
                return table;
            }
        }
        return null; // 모든 테이블이 꽉 찼음
    }
}
