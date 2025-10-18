using UnityEngine;
using System.Collections;
using System.Linq;

public class Customer : MonoBehaviour
{
    public enum CustomerState { MovingToTable, DecidingMenu, WaitingForFood, Eating, Leaving }
    public CustomerState currentState;
    public GameObject orderIconPrefab; 
    public Transform iconSpawnPoint;   // 아이콘이 표시될 머리 위 위치
    private GameObject currentOrderIcon; // 현재 떠 있는 아이콘을 저장할 변수

    private Transform targetTable;
    [SerializeField]
    private float speed = 3f;
    private PlayerRecipe myOrderedRecipe;
    private Transform exitPoint; // 퇴장 시 이동할 목표 지점

    public void Initialize(Transform table, Transform exit)
    {
        targetTable = table;
        exitPoint = exit;
        currentState = CustomerState.MovingToTable;
    }

    void Update()
    {
        switch (currentState)
        {
            case CustomerState.MovingToTable:
                // 테이블로 이동
                transform.position = Vector2.MoveTowards(transform.position, targetTable.position, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetTable.position) < 0.1f)
                {
                    currentState = CustomerState.DecidingMenu;
                    targetTable.GetComponent<Table>().Occupy(gameObject); // 테이블 점유

                    StartCoroutine(DecideMenuCoroutine()); // 메뉴 결정 코루틴 시작
                }
                break;
            case CustomerState.Leaving:
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(0, -5), speed * Time.deltaTime);
                Destroy(gameObject, 2f);
                break;
        }
    }

    IEnumerator DecideMenuCoroutine()
    {
        Debug.Log("손님이 메뉴를 고르는 중...");
        yield return new WaitForSeconds(2f);

        var unlockedRecipes = RecipeManager.instance.playerRecipes.Values.ToList();

        if (unlockedRecipes.Count > 0)
        {
            int randomIndex = Random.Range(0, unlockedRecipes.Count);
            PlayerRecipe orderedRecipe = unlockedRecipes[randomIndex];
            myOrderedRecipe = orderedRecipe; //내가 주문한 메뉴 기억

            Debug.Log($"{orderedRecipe.data.recipeName} 결정! 주방에 주문을 넣습니다.");

            GameObject foodInstance = null;
            if (orderedRecipe.data.foodPrefab != null)
            {
                foodInstance = Instantiate(orderedRecipe.data.foodPrefab);
                foodInstance.SetActive(false);
            }

            if (orderIconPrefab != null && iconSpawnPoint != null)
            {
                currentOrderIcon = Instantiate(orderIconPrefab, iconSpawnPoint.position, Quaternion.identity);
                currentOrderIcon.transform.SetParent(iconSpawnPoint);
                OrderIconUI iconUI = currentOrderIcon.GetComponent<OrderIconUI>();
                if (iconUI != null)
                {
                    iconUI.SetIcon(orderedRecipe.data.icon);
                }
            }

            KitchenOrder newOrder = new KitchenOrder(this, orderedRecipe, foodInstance);
            RestaurantManager.instance.OrderQueue.Add(newOrder);

            currentState = CustomerState.WaitingForFood;
        }
        else
        {
            Debug.LogError("손님이 주문할 수 있는 레시피가 하나도 없습니다! RecipeManager를 확인하세요.");
        }
    }

    public void ReceiveFood()
    {
        if (currentOrderIcon != null)
        {
            Destroy(currentOrderIcon);
        }

        currentState = CustomerState.Eating;
        StartCoroutine(EatAndLeave());
    }

    public void SetTable(Transform table)
    {
        targetTable = table;
        currentState = CustomerState.MovingToTable;
    }


    System.Collections.IEnumerator EatAndLeave()
    {
        Debug.Log("식사 시작");
        yield return new WaitForSeconds(2f); // 2초간 식사
        Debug.Log("식사 완료");
        if (myOrderedRecipe != null)
        {
            int price = myOrderedRecipe.GetCurrentPrice();
            GameManager.instance.AddGold(price);
            Debug.Log($"손님이 {myOrderedRecipe.data.recipeName}을(를) 먹고 {price}원을 지불했습니다.");
        }
        else
        {
            Debug.LogError("주문한 레시피 정보가 없습니다!");
        }
        GameManager.instance.AddCustomerCount();
        targetTable.GetComponent<Table>().Vacate(); // 테이블 비우기
        targetTable.GetComponent<Table>().isDirty = true; // 테이블 더러운 상태로 변경
        currentState = CustomerState.Leaving;
        RestaurantManager.instance.customers.Remove(this);
    }
}
