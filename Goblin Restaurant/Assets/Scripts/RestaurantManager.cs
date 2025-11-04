using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RestaurantManager : MonoBehaviour
{
    public static RestaurantManager instance;

    // [직원 스폰을 위한 추가 필드]
    [Header("Employee Spawning")]
    [Tooltip("맵에 스폰시킬 직원 캐릭터 기본 프리팹 (Employee.cs가 붙어있어야 함)")]
    public GameObject employeePrefab; // 기본 프리팹
    [Tooltip("직원들이 처음 나타날 위치")]
    public Transform spawnPoint;

    // 기존 필드
    public List<Customer> customers;
    public List<Table> tables;
    public List<CounterTop> counterTops;
    [SerializeField]
    private List<KitchenOrder> orderQueue;

    public List<KitchenOrder> OrderQueue => orderQueue;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        customers = new List<Customer>();
        orderQueue = new List<KitchenOrder>();
    }

    /// <summary>
    /// GameManager의 Start()에서 호출되어, 기본 프리팹을 사용하여 고용된 직원들을 맵에 생성합니다.
    /// (이전 버전 호환성 유지를 위해 남겨두었으나, 새 함수가 호출될 것입니다.)
    /// </summary>
    public void SpawnHiredEmployees(List<EmployeeInstance> hiredEmployees)
    {
        // 이 함수는 GameManager에서 SpawnWorkersWithPrefabs가 대체합니다.

        if (employeePrefab == null || spawnPoint == null)
        {
            Debug.LogError("RestaurantManager ERROR: Employee Prefab or Spawn Point is not set in Inspector!");
            return;
        }

        // 단일 프리팹으로 스폰하는 로직 (기본 동작)
        List<(EmployeeInstance, GameObject)> workersToSpawn = hiredEmployees
            .Select(data => (data, employeePrefab))
            .ToList();

        SpawnWorkersWithPrefabs(workersToSpawn);
    }

    /// <summary>
    /// GameManager에서 전달된, 데이터와 종족별 프리팹 쌍을 사용하여 직원들을 스폰합니다.
    /// </summary>
    public void SpawnWorkersWithPrefabs(List<(EmployeeInstance data, GameObject prefab)> workersToSpawn)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("RestaurantManager ERROR: Spawn Point is not set in Inspector! Cannot spawn workers.");
            return;
        }

        foreach (var (employeeData, employeePrefab) in workersToSpawn)
        {
            if (employeePrefab == null)
            {
                Debug.LogWarning($"Skipping spawn for {employeeData.firstName}: Prefab is missing for this employee!");
                continue;
            }

            // Worker 오브젝트 생성
            // transform을 Parent로 설정하여 Hierarchy에서 RestaurantManager의 자식으로 보이게 합니다.
            GameObject workerObject = Instantiate(employeePrefab, spawnPoint.position, Quaternion.identity, this.transform);

            // Employee.cs 스크립트 가져오기
            Employee workerComponent = workerObject.GetComponent<Employee>();

            if (workerComponent != null)
            {
                // 생성된 직원 오브젝트에 EmployeeInstance 데이터를 할당하고 초기화합니다.
                workerComponent.Initialize(employeeData, spawnPoint);
                workerObject.name = $"Worker - {employeeData.firstName} ({employeeData.BaseData.speciesName})";
            }
            else
            {
                Debug.LogError($"Employee Prefab에 Employee.cs 스크립트가 없습니다: {workerObject.name}");
                Destroy(workerObject);
            }
        }
    }
    /// <summary>
    /// 단일 직원을 맵에 스폰합니다. (게임 중 고용 시 사용)
    /// </summary>
    /// <param name="employeeData">스폰할 직원의 인스턴스 데이터</param>
    /// <param name="employeePrefab">해당 직원의 외형 프리팹</param>
    public void SpawnSingleWorker(EmployeeInstance employeeData, GameObject employeePrefab)
    {
        // --- 스폰 지점 확인 ---
        if (spawnPoint == null)
        {
            Debug.LogError("RestaurantManager ERROR: Spawn Point is not set! Cannot spawn worker.");
            return;
        }
        // --- 프리팹 확인 ---
        if (employeePrefab == null)
        {
            Debug.LogWarning($"Skipping spawn for {employeeData.firstName}: Prefab is missing!");
            return;
        }

        // --- Worker 오브젝트 생성 ---
        // (this.transform을 부모로 설정)
        GameObject workerObject = Instantiate(employeePrefab, spawnPoint.position, Quaternion.identity, this.transform);

        // --- Employee.cs 스크립트 가져오기 및 초기화 ---
        Employee workerComponent = workerObject.GetComponent<Employee>();
        if (workerComponent != null)
        {
            workerComponent.Initialize(employeeData, spawnPoint);
            workerObject.name = $"Worker - {employeeData.firstName} ({employeeData.BaseData.speciesName})";
        }
        else
        {
            Debug.LogError($"Employee Prefab에 Employee.cs 스크립트가 없습니다: {workerObject.name}");
            Destroy(workerObject);
        }
    }
}
