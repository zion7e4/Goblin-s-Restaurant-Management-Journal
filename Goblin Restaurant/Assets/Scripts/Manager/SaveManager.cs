using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Application.persistentDataPath + "/savefile.json";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 저장 (Save) ---
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. GameManager 데이터
        if (GameManager.instance != null)
        {
            data.dayCount = GameManager.instance.DayCount; // (GameManager에 접근자 필요할 수 있음)
            data.gold = GameManager.instance.totalGoldAmount;
        }

        if (FameManager.instance != null)
        {
            data.famePoints = FameManager.instance.CurrentFamePoints;
        }

        if (InventoryManager.instance != null)
        {
            foreach (var kvp in InventoryManager.instance.playerIngredients)
            {
                data.inventoryItems.Add(new SaveData.IngredientSaveData { id = kvp.Key, count = kvp.Value });
            }
            data.discoveredIngredients = InventoryManager.instance.discoveredIngredients.ToList();
        }

        if (RecipeManager.instance != null)
        {
            foreach (var kvp in RecipeManager.instance.playerRecipes)
            {
                data.unlockedRecipes.Add(new SaveData.RecipeSaveData { id = kvp.Key, level = kvp.Value.currentLevel });
            }
        }

        if (EmployeeManager.Instance != null)
        {
            foreach (var emp in EmployeeManager.Instance.hiredEmployees)
            {
                data.employees.Add(new SaveData.EmployeeSaveData
                {
                    name = emp.firstName,
                    cookStat = emp.currentCookingStat,
                    serveStat = emp.currentServingStat,
                    charmStat = emp.currentCharmStat
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"[SaveManager] 게임 저장 완료! 경로: {saveFilePath}");
        NotificationController.instance.ShowNotification("게임이 저장되었습니다.");
    }

    // --- 불러오기 (Load) ---
    // (이번 요청엔 없지만, 저장 기능 테스트를 위해 필요할 수 있습니다.)
    public bool LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("저장된 파일이 없습니다.");
            return false; // 로드 실패
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // 데이터 적용 (매니저들이 존재하는지 확인 필요)
            // 주의: 타이틀 씬에서 로드할 경우, 매니저들이 아직 없을 수 있으므로
            // 씬을 먼저 로드한 뒤 데이터를 적용하거나, DontDestroyOnLoad된 매니저에 값을 넣어야 함.
            
            // 여기서는 "데이터를 메모리에 들고 있다가, 게임 씬이 시작될 때 적용"하는 방식 대신,
            // "매니저들이 DontDestroyOnLoad로 살아있다고 가정"하고 바로 넣습니다.
            // (타이틀 씬에도 GameManager 등이 배치되어 있어야 합니다)

            if (GameManager.instance != null)
            {
                GameManager.instance.DayCount = data.dayCount;
                GameManager.instance.totalGoldAmount = data.gold;
                // UI 갱신 필요 시 호출 (예: GameManager.instance.UpdateUI())
            }

            if (FameManager.instance != null)
            {
                FameManager.instance.CurrentFamePoints = data.famePoints;
                // FameManager.instance.UpdateFameLevel(); // 레벨 재계산 필요 시 public으로 바꾸고 호출
            }

            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.playerIngredients.Clear();
                foreach (var item in data.inventoryItems)
                {
                    InventoryManager.instance.playerIngredients[item.id] = item.count;
                }
                InventoryManager.instance.discoveredIngredients = new HashSet<string>(data.discoveredIngredients);
            }

            if (RecipeManager.instance != null)
            {
                RecipeManager.instance.playerRecipes.Clear();
                foreach (var recipeData in data.unlockedRecipes)
                {
                    // 레시피 해금 및 레벨 설정 로직
                    RecipeManager.instance.UnlockRecipe(recipeData.id); 
                    // (심화: 레벨 복구 로직이 필요하다면 RecipeManager에 SetLevel 함수 추가 필요)
                }
            }

            // (직원 데이터 로드는 복잡하므로, 일단 기존 직원을 클리어하고 저장된 데이터로 새로 생성하는 방식 등이 필요함)
            
            Debug.Log("[SaveManager] 게임 불러오기 성공!");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 로드 중 오류 발생: {e.Message}");
            return false;
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
}