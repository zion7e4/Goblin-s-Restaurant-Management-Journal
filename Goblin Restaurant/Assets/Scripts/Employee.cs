using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;

// �� ��ũ��Ʈ�� �ʿ� �����Ǵ� ���� ĳ���Ϳ� �پ� �ϲ� ������ �մϴ�.
public class Employee : MonoBehaviour
{
    [Tooltip("�� ������ ����� ������ �ν��Ͻ�")]
    private EmployeeInstance employeeData;

    public enum EmployeeState
    {
        Idle,
        MovingToCounterTop,
        Cooking,
        MovingToServe,
        MovingToPickupFood,
        MovingToIdle,
        CheckingTable,
        MovingToTable,
        Cleaning
    }

    public EmployeeState currentState;

    [SerializeField]
    private float movespeed = 3f;

    // �� ������ CookFoodCoroutine���� 'finalCookTime'���� ��ü�Ǿ����ϴ�.
    // public float cookingtime = 5f; 

    /*[SerializeField]
    private int cookingskill = 1; // ���� ��� �߰� ����*/

    [SerializeField]
    private Customer targetCustomer;
    [SerializeField]
    private CounterTop targetCountertop;
    [SerializeField]
    private Table targetTable;
    [SerializeField]
    private Transform idlePosition; // ������ �� ���� ���� �� �� ���� ��ġ
    private KitchenOrder targetOrder;

    // RestaurantManager���� ȣ���
    public void Initialize(EmployeeInstance data, Transform defaultIdlePosition)
    {
        this.employeeData = data;
        this.idlePosition = defaultIdlePosition;

        // (���� �丮 �ð� ��� ���� ����)

        // �����
        Debug.Log($"{data.firstName} ���� �Ϸ�. (�丮 �ð��� �����ǿ� ���� �����˴ϴ�)");
        // 1. ���� ������(BaseData)���� '�⺻ �̵� �ӵ�'�� �����ɴϴ�.
        if (data.BaseData != null)
        {
            this.movespeed = data.BaseData.baseMoveSpeed;
        }
        else
        {
            // (BaseData�� ���� ��� �⺻�� 3f�� ����)
            Debug.LogWarning($"Initialize: {data.firstName}�� BaseData�� null�Դϴ�. �⺻ �̵��ӵ�(3f)�� ����մϴ�.");
        }

        // �����
        Debug.Log($"{data.firstName} ���� �Ϸ�. (�̵� �ӵ�: {this.movespeed})");

        currentState = EmployeeState.Idle;
    }


    void Start()
    {
        // ���� Initialize�� ȣ����� �ʾҴٸ� (������ �ʿ� �ִ� �����̶��)
        if (employeeData == null)
        {
            // �� ������ ���� �ʿ� �ִ� ���ΰ� ������ �� �����Ƿ�, �⺻ ���·� ����
            currentState = EmployeeState.Idle;
        }
    }

    void Update()
    {
        // ���¿� ���� �ٸ� �ൿ�� ����
        switch (currentState)
        {
            case EmployeeState.Idle:
                FindTask();
                break;
            case EmployeeState.MovingToIdle:
                FindTask();

                if (idlePosition != null)
                {
                    if (currentState == EmployeeState.MovingToIdle)
                    {
                        MoveTo(idlePosition.position, () => { currentState = EmployeeState.Idle; });
                    }
                }
                else
                {
                    currentState = EmployeeState.Idle;
                }
                break;
            case EmployeeState.MovingToCounterTop:
                // MoveTo �Լ��� ���� �� CookFoodCoroutine ����
                MoveTo(targetCountertop.transform.position, () => { StartCoroutine(CookFoodCoroutine()); });
                break;
            case EmployeeState.Cooking:
                break;
            case EmployeeState.MovingToPickupFood:
                // MoveTo �Լ��� ���� �� PickupFoodCoroutine ����
                MoveTo(targetCountertop.transform.position, () => { StartCoroutine(PickupFoodCoroutine()); });
                break;
            case EmployeeState.MovingToServe:
                MoveTo(targetCustomer.transform.position, ServeFood);
                break;
            case EmployeeState.CheckingTable:
                CheckTable();
                break;
            case EmployeeState.MovingToTable:
                // MoveTo �Լ��� ���� �� CleaningTable ����
                MoveTo(targetTable.transform.position, () => { StartCoroutine(CleaningTable()); });
                break;
            case EmployeeState.Cleaning:
                break;
        }
    }
    void FindTask()
    {
        // (���� ��ġ) ������ �����Ͱ� ������ �ƹ� �۾��� ã�� �ʽ��ϴ�.
        if (employeeData == null)
        {
            Debug.LogWarning("Employee.cs: employeeData�� null�̶� FindTask�� ������ �� �����ϴ�.");
            return;
        }

        // --- 1. 'Ȧ' ����: �ֿ켱���� '������ ����' ã�� ---
        // (������ 'Ȧ'�̰ų� '������'�� ���� ���� �۾��� ã��)
        if (employeeData.assignedRole == EmployeeRole.Hall ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                // (ReadyToServe �����̰�, cookedOnCounterTop�� �Ҵ�� �ֹ� ã��)
                targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o =>
                    o != null &&
                    o.status == OrderStatus.ReadyToServe &&
                    o.cookedOnCounterTop != null
                );
            }

            if (targetOrder != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (Ȧ) ������ ���� �߰�!");

                // (�߿�) �ٸ� ������ �� �ֹ��� �� ä������ ��� ���� ����
                targetOrder.status = OrderStatus.Completed; // (�ӽ÷� 'Completed'�� ����, 'Serving' ���°� �ʿ��� ���� ����)

                targetCustomer = targetOrder.customer;
                targetCountertop = targetOrder.cookedOnCounterTop; // (�Ⱦ��� ī���� ��ġ)

                currentState = EmployeeState.MovingToPickupFood; // '�Ⱦ�'�Ϸ� �̵�
                return; // �۾��� ã�����Ƿ� ����
            }
        }

        // --- 2. '�ֹ�' ����: '�丮�� �ֹ�' ã�� ---
        // (������ '�ֹ�'�̰ų� '������'�� ���� �丮 �۾��� ã��)
        if (employeeData.assignedRole == EmployeeRole.Kitchen ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                targetOrder = RestaurantManager.instance.OrderQueue.FirstOrDefault(o => o != null && o.status == OrderStatus.Pending);
            }

            if (targetOrder != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (�ֹ�) �丮�� �ֹ� �߰�");
                targetCountertop = RestaurantManager.instance.counterTops.FirstOrDefault(s => s != null && !s.isBeingUsed);

                if (targetCountertop != null)
                {
                    targetOrder.status = OrderStatus.Cooking;
                    targetCountertop.isBeingUsed = true;
                    currentState = EmployeeState.MovingToCounterTop;
                    return; // �۾��� ã�����Ƿ� ����
                }
            }
        }


        // --- 3. 'Ȧ' ����: 'û���� ���̺�' ã�� ---
        // (������ 'Ȧ'�̰ų� '������'�� ���� û�� �۾��� ã��)
        if (employeeData.assignedRole == EmployeeRole.Hall ||
            employeeData.assignedRole == EmployeeRole.Unassigned)
        {
            if (RestaurantManager.instance != null)
            {
                targetTable = RestaurantManager.instance.tables.FirstOrDefault(t =>
                    t != null && t.isDirty && !t.isBeingUsedForCleaning);
            }

            if (targetTable != null)
            {
                Debug.Log($"{employeeData?.firstName ?? "Worker"} (Ȧ) û���� ���̺� �߰�");
                targetTable.isBeingUsedForCleaning = true;
                currentState = EmployeeState.MovingToTable;
                return; // �۾��� ã�����Ƿ� ����
            }
        }


        // 4. �� �� ������ ��� ��ġ�� �̵�
        if (idlePosition != null && Vector2.Distance(transform.position, idlePosition.position) > 0.1f)
        {
            currentState = EmployeeState.MovingToIdle;
        }
    }
    // �丮 �ڷ�ƾ
    IEnumerator CookFoodCoroutine()
    {
        currentState = EmployeeState.Cooking;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} {targetOrder.recipe.data.recipeName} �丮 ����");

        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.position = targetCountertop.transform.position;
            targetOrder.foodObject.SetActive(true);
        }

        // 1. �������� '�⺻ �丮 �ð�'�� �����ɴϴ�. (RecipeData�� 'Base Cook Time')
        // 1. �������� '�⺻ �丮 �ð�'�� �����ɴϴ�.
        float baseRecipeTime = 10f; // (���� �� �⺻��)
        if (targetOrder.recipe != null && targetOrder.recipe.data != null)
        {
            baseRecipeTime = targetOrder.recipe.data.baseCookTime;
        }
        else
        {
            Debug.LogError("CookFoodCoroutine: targetOrder.recipe.data�� null�Դϴ�!");
        }

        // --- �ó��� �� Ư�� ���ʽ� ����/�ӵ� �ջ� ---
        int baseCookingStat = employeeData.currentCookingStat;
        int bonusCookingStat_Synergy = 0;
        float speedBonusPercent_Synergy = 0f;
        float specificStatMultiplier_Trait = 0f; // "�Ĳ���" Ư�� ���ʽ� (��: 0.1)
        float allStatMultiplier_Trait = 0f;      // "���ΰ�" Ư�� ���ʽ� (��: 0.1)
        float workSpeedMultiplier_Trait = 0f;    // "������/������" Ư�� ���ʽ� (��: -0.1 �Ǵ� +0.1)

        if (SynergyManager.Instance != null)
        {
            // 2. �ó��� �Ŵ������� '����'�� '�ӵ�' ���ʽ��� ����ϴ�.
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusCookingStat_Synergy = cookBonus;
            speedBonusPercent_Synergy = SynergyManager.Instance.GetCookingSpeedBonus(employeeData);
        }

        // 3. ���� �����Ϳ��� 'Ư��' ���ʽ��� ����ϴ�.
        if (employeeData != null)
        {
            specificStatMultiplier_Trait = employeeData.GetTraitCookingStatMultiplier(); // "�Ĳ���"
            allStatMultiplier_Trait = employeeData.GetTraitAllStatMultiplier();          // "���ΰ�"
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier();    // "������/������"
        }

        // 4. ���� ���� = (�⺻ + �ó���) * (1 + �Ĳ��� + ���ΰ�)
        float totalMultiplier = 1.0f + specificStatMultiplier_Trait + allStatMultiplier_Trait;
        int finalCookingStat = (int)((baseCookingStat + bonusCookingStat_Synergy) * totalMultiplier);

        // 5. ��ȹ�� �������� '����'�� ���� �ð� ���
        float finalCookTime = baseRecipeTime / (1 + (finalCookingStat * 0.008f));

        // 6. '�ӵ�' ���ʽ�(�ó���)�� '�۾� �ӵ�' ���ʽ�(Ư��)�� �߰��� ����
        // (��: "������"(-0.1) ���� �� 1.0 - (-0.1) = 1.1 (�ð� 10% ����))
        finalCookTime = finalCookTime * (1.0f - speedBonusPercent_Synergy); // �ó���
        finalCookTime = finalCookTime * (1.0f - workSpeedMultiplier_Trait); // Ư��

        finalCookTime = Mathf.Max(0.5f, finalCookTime); // (�ּ� 0.5�� ����)

        Debug.Log($"[{employeeData.firstName}] �丮 �ð� ���. " +
                    $"�⺻�ð�: {baseRecipeTime:F1}s, ����: {finalCookingStat} (�⺻ {baseCookingStat} + �ó��� {bonusCookingStat_Synergy}) * Ư��(x{totalMultiplier}), " +
                    $"�ӵ�(�ó���): {speedBonusPercent_Synergy * 100}%, �۾��ӵ�(Ư��): {workSpeedMultiplier_Trait * 100}%, " +
                    $"�����ð�: {finalCookTime:F1}s");

        // ���� ���� �丮 �ð���ŭ ���
        yield return new WaitForSeconds(finalCookTime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} �丮 �ϼ�");

        // --- ����� ���� Ȯ�� ���� ---
        float totalSaveChance = 0f;
        if (SynergyManager.Instance != null) { totalSaveChance += SynergyManager.Instance.GetIngredientSaveChance(); }
        if (employeeData != null) { totalSaveChance += employeeData.GetTraitSaveChance(); }
        if (totalSaveChance > 0 && UnityEngine.Random.Range(0f, 1f) < totalSaveChance)
        {
            Debug.Log($"[����� ����!] {employeeData.firstName}��(��) �丮 ��Ḧ �����߽��ϴ�! (Ȯ��: {totalSaveChance * 100:F0}%)");
            if (GameManager.instance != null && targetOrder.recipe != null)
            {
                GameManager.instance.RefundIngredients(targetOrder.recipe.data);
            }
        }
        // --- ���� �Ϸ� ---

        // --- '�ֹ�' ����: �丮 �Ϸ� �� '���' ���·� ---

        // 1. �ֹ� ���¸� '���� �غ� �Ϸ�'�� ����
        targetOrder.status = OrderStatus.ReadyToServe;
        targetCustomer = targetOrder.customer;
        currentState = EmployeeState.MovingToServe;

        if (targetOrder.foodObject != null)
        {
            targetOrder.foodObject.transform.SetParent(this.transform);
            targetOrder.foodObject.transform.localPosition = new Vector3(0, 1.2f, 0);
        }

        // ���� �������� �̵�
    }

        // 2. 'Ȧ' ������ �Ⱦ��� �� �ֵ���, �丮�� �Ϸ�� ī���� ��ġ�� �ֹ����� ����
        targetOrder.cookedOnCounterTop = this.targetCountertop;

        // 3. �丮��� �������� �ʰ� '���' ���·� ���ư�
        currentState = EmployeeState.MovingToIdle;

        // 4. ����(foodObject)�� �ڽſ��� ������ �ʰ�, ī���� ���� �Ӵϴ�.

        // 5. �۾��� �������Ƿ� Ÿ���� ���ϴ�.
        if (targetCountertop != null) targetCountertop.isBeingUsed = false;

        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;
    }

    // ���� ����
    void ServeFood()
    {
        Debug.Log($"{employeeData?.firstName ?? "Worker"} ���� �Ϸ�");
        if (targetCustomer != null)
        {
            // Customer.ReceiveFood �Լ��� �� ���� ������(employeeData)�� �����մϴ�.
            targetCustomer.ReceiveFood(this.employeeData);
        }

        if (targetOrder.foodObject != null)
        {
            Destroy(targetOrder.foodObject);
        }

        if (RestaurantManager.instance != null && targetOrder != null)
        {
            RestaurantManager.instance.OrderQueue.Remove(targetOrder);
        }

        // ����ߴ� �ڿ����� �ʱ�ȭ
        if (targetCountertop != null) targetCountertop.isBeingUsed = false;
        targetCustomer = null;
        targetCountertop = null;
        targetOrder = null;

        currentState = EmployeeState.MovingToIdle;
    }

    void CheckTable()
    {
        // FindTask �������� û���� ���̺��� ã�Ҵٰ� ����
        if (targetTable != null && targetTable.isDirty)
        {
            currentState = EmployeeState.MovingToTable;
        }
        else
        {
            currentState = EmployeeState.Idle;
        }
    }

    // ���̺� û��
    IEnumerator CleaningTable()
    {
        currentState = EmployeeState.Cleaning;
        Debug.Log($"{employeeData?.firstName ?? "Worker"} ���̺� û�� ����");

        // --- �ó��� �� Ư�� ���ʽ� ����/�ӵ� �ջ� ---

        // 1. '�⺻ û�� �ð�'�� �����մϴ�. (�ӽ÷� 3�� ����)
        float baseCleaningTime = 3f;

        // 2. �� ������ '�⺻ ����' ������ �����ɴϴ�.
        int baseServingStat = employeeData.currentServingStat;
        int bonusServingStat = 0;
        float allStatMultiplier_Trait = 0f;   // "���ΰ�" Ư�� ���ʽ�
        float workSpeedMultiplier_Trait = 0f; // "������/������" Ư�� ���ʽ�

        // 3. �ó��� �Ŵ������� '���� ���ʽ�'�� ����ϴ�.
        if (SynergyManager.Instance != null)
        {
            var (cookBonus, serveBonus, charmBonus) = SynergyManager.Instance.GetStatBonuses(employeeData);
            bonusServingStat = serveBonus;
        }

        // 4. ���� �����Ϳ��� 'Ư��' ���ʽ��� ����ϴ�.
        if (employeeData != null)
        {
            allStatMultiplier_Trait = employeeData.GetTraitAllStatMultiplier(); // "���ΰ�"
            workSpeedMultiplier_Trait = employeeData.GetTraitWorkSpeedMultiplier(); // "������/������"
        }

        // 5. ���� ���� = (�⺻ ���� + �ó��� ���ʽ�) * (1 + ���ΰ�)
        float totalMultiplier = 1.0f + allStatMultiplier_Trait;
        int finalServingStat = (int)((baseServingStat + bonusServingStat) * totalMultiplier);

        // 6. ��ȹ�� �������� '����'�� ���� �ð� ���
        float finalCleaningTime = baseCleaningTime / (1 + (finalServingStat * 0.008f));

        // 7. '�۾� �ӵ�' ���ʽ�(Ư��)�� �߰��� ����
        finalCleaningTime = finalCleaningTime * (1.0f - workSpeedMultiplier_Trait);

        finalCleaningTime = Mathf.Max(0.5f, finalCleaningTime); // (�ּ� 0.5�� ����)

        Debug.Log($"[{employeeData.firstName}] û�� �ð� ���. " +
                  $"�⺻�ð�: {baseCleaningTime:F1}s, ��������: {finalServingStat} (�⺻ {baseServingStat} + ���ʽ� {bonusServingStat}) * Ư��(x{totalMultiplier}), " +
                  $"�۾��ӵ�(Ư��): {workSpeedMultiplier_Trait * 100}%, �����ð�: {finalCleaningTime:F1}s");

        // ���� ���� �ð���ŭ ���
        yield return new WaitForSeconds(finalCleaningTime);

        Debug.Log($"{employeeData?.firstName ?? "Worker"} ���̺� û�� �Ϸ�");
        if (targetTable != null)
        {
            targetTable.isDirty = false;
            targetTable.isBeingUsedForCleaning = false;
        }
        targetTable = null;
        currentState = EmployeeState.MovingToIdle;
    }

    // ��ǥ �������� �̵��ϰ�, �����ϸ� ������ �ൿ(Action)�� �����ϴ� �Լ�
    void MoveTo(Vector3 destination, Action onArrived)
    {
        float synergySpeedBonus = 0f;
        float traitSpeedBonus = 0f;

        // 1. �ó��� �Ŵ������� ���ʽ� ȹ�� ("����� �۾���" ��)
        if (SynergyManager.Instance != null)
        {
            synergySpeedBonus = SynergyManager.Instance.GetMoveSpeedMultiplier();
        }

        // 2. Ư��(Trait)���� ���ʽ� ȹ�� ("������" ��)
        if (employeeData != null)
        {
            traitSpeedBonus = employeeData.GetTraitMoveSpeedMultiplier();
        }

        // 3. (1.0f + �ó��� + Ư��)�� ���Ͽ� ���� �ӵ� ���
        float finalMoveSpeed = movespeed * (1.0f + synergySpeedBonus + traitSpeedBonus);
        finalMoveSpeed = Mathf.Max(0.1f, finalMoveSpeed); // �ӵ��� 0�� ���� �ʰ� �ּ� 0.1 ����

        // 4. ���� �ӵ��� �����Ͽ� �̵�
        transform.position = Vector2.MoveTowards(transform.position, destination, finalMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            onArrived?.Invoke();
        }
    }
}