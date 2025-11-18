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
    [Tooltip("'주방' 역할 직원이 대기할 위치")]
    public Transform kitchenIdlePoint;
    [Tooltip("'홀' 역할 직원이 대기할 위치")]
    public Transform hallIdlePoint;

    // 기존 필드
    public List<Customer> customers;
    public List<Table> tables;
    public List<CounterTop> counterTops;
    public int cleanliness = 100; // 식당 청결도 (0 ~ 100)
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
    /// (비활성) GameManager의 Start()에서 호출되어, 기본 프리팹을 사용하여 고용된 직원들을 맵에 생성합니다.
    /// </summary>
    public void SpawnHiredEmployees(List<EmployeeInstance> hiredEmployees)
    {
        // 이 함수는 GameManager에서 SpawnWorkersWithPrefabs가 대체합니다.
        if (employeePrefab == null || spawnPoint == null)
        {
            Debug.LogError("RestaurantManager ERROR: Employee Prefab or Spawn Point is not set in Inspector!");
            return;
        }

        List<(EmployeeInstance, GameObject)> workersToSpawn = hiredEmployees
          .Select(data => (data, employeePrefab))
          .ToList();

        SpawnWorkersWithPrefabs(workersToSpawn);
    }

    /// <summary>
    /// (게임 시작 시) GameManager에서 전달된, 데이터와 프리팹 쌍을 사용하여 직원들을 스폰합니다.
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

            // 1. 직원의 역할(Role)에 따라 대기 위치(Idle Point) 결정
            Transform targetIdlePoint = GetIdlePointForRole(employeeData.assignedRole);

            // 2. Worker 오브젝트 생성 (스폰은 spawnPoint에서)
            GameObject workerObject = Instantiate(employeePrefab, spawnPoint.position, Quaternion.identity, this.transform);

            // 3. Employee.cs 스크립트 가져오기
            Employee workerComponent = workerObject.GetComponent<Employee>();
            if (workerComponent != null)
            {
                // 4. '대기 위치'를 Initialize로 전달
                workerComponent.Initialize(employeeData, targetIdlePoint);
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
    /// (게임 중 고용 시) 단일 직원을 맵에 스폰합니다.
    /// </summary>
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

        // 1. 직원의 역할(Role)에 따라 대기 위치(Idle Point) 결정
        Transform targetIdlePoint = GetIdlePointForRole(employeeData.assignedRole);

        // 2. Worker 오브젝트 생성 (스폰은 spawnPoint에서)
        GameObject workerObject = Instantiate(employeePrefab, spawnPoint.position, Quaternion.identity, this.transform);

        // 3. Employee.cs 스크립트 가져오기 및 초기화 ('대기 위치' 전달)
        Employee workerComponent = workerObject.GetComponent<Employee>();
        if (workerComponent != null)
        {
            workerComponent.Initialize(employeeData, targetIdlePoint);
            workerObject.name = $"Worker - {employeeData.firstName} ({employeeData.BaseData.speciesName})";
        }
        else
        {
            Debug.LogError($"Employee Prefab에 Employee.cs 스크립트가 없습니다: {workerObject.name}");
            Destroy(workerObject);
        }
    }

    /// <summary>
    /// 직원의 역할(Role)에 맞는 대기 위치(Transform)를 반환합니다.
    /// </summary>
    private Transform GetIdlePointForRole(EmployeeRole role)
    {
        switch (role)
        {
            case EmployeeRole.Kitchen:
                // 주방 대기 위치가 설정되어 있으면 반환, 없으면 공용 스폰 위치 반환
                return (kitchenIdlePoint != null) ? kitchenIdlePoint : spawnPoint;

            case EmployeeRole.Hall:
                // 홀 대기 위치가 설정되어 있으면 반환, 없으면 공용 스폰 위치 반환
                return (hallIdlePoint != null) ? hallIdlePoint : spawnPoint;

            case EmployeeRole.Unassigned:
            default:
                // 미지정 역할은 공용 스폰 위치를 대기 위치로 사용
                return spawnPoint;
        }
    }
}