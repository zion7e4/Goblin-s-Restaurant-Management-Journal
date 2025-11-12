using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    public List<GameObject> customerPrefabs; // 스폰할 고객 프리팹
    public Transform spawnPoint; // 고객이 스폰될 위치

    public float spawnInterval = 5f; // 스폰 시도 간격
    private float spawnTimer;


    private void Update()
    {
        if (GameManager.instance.currentState != GameManager.GameState.Open || MenuPlanner.instance.isSoldOut)
        {
            return;
        }

        if (GameManager.instance.currentState == GameManager.GameState.Open &&
            !MenuPlanner.instance.AreAnyItemsLeftToSell())
        {
            Debug.Log("모든 메뉴 완판! 손님 스폰을 중지합니다.");
            return; // Update 함수 종료
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
        if (!MenuPlanner.instance.AreAnyItemsLeftToSell() && GameManager.instance.currentState == GameManager.GameState.Open)
        {
            return;
        }

        Table emptyTable = FindEmptyTable();

        if (emptyTable != null)
        {
            if (customerPrefabs == null || customerPrefabs.Count == 0)
            {
                Debug.LogError("CustomerSpawner: 'Customer Prefabs' 리스트가 비어있습니다. 인스펙터에서 할당해주세요.");
                return;
            }

            int randomIndex = Random.Range(0, customerPrefabs.Count);
            GameObject selectedPrefab = customerPrefabs[randomIndex];

            if (selectedPrefab == null)
            {
                Debug.LogWarning($"CustomerSpawner: 'Customer Prefabs' 리스트의 {randomIndex}번째 항목이 비어있습니다.");
                return;
            }

            Debug.Log("빈 테이블 발견. 랜덤 고객 스폰");

            GameObject newCustomerObj = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);

            Customer newCustomer = newCustomerObj.GetComponent<Customer>();

            RestaurantManager.instance.customers.Add(newCustomer);

            newCustomer.Initialize(emptyTable.transform, spawnPoint);
            emptyTable.isOccupied = true;
        }

        else
        {
            Debug.Log("모든 테이블이 꽉 찼음");
        }
    }

    Table FindEmptyTable()
    {
        if (RestaurantManager.instance == null || RestaurantManager.instance.tables == null)
        {
            Debug.LogError("RestaurantManager 또는 테이블 리스트가 없습니다!");
            return null;
        }

        foreach (Table table in RestaurantManager.instance.tables)
        {
            if (table != null && !table.isOccupied && !table.isDirty)
            {
                return table;
            }
        }
        return null;
    }
}