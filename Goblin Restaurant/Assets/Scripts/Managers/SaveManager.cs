using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // 씬 이벤트를 위해 필수

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string saveFilePath;
    
    private SaveData loadedDataBuffer; 

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TitleScene") return;
        
        if (loadedDataBuffer != null && GameManager.instance != null)
        {
            ApplyLoadedData();
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        if (GameManager.instance != null)
        {
            data.dayCount = GameManager.instance.DayCount;
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
            data.employees.Clear();
            foreach (var empInstance in EmployeeManager.Instance.hiredEmployees)
            {
                var empSaveData = new SaveData.EmployeeSaveData
                {
                    speciesName = empInstance.BaseData.name, // Using ScriptableObject's name as ID
                    firstName = empInstance.firstName,
                    currentLevel = empInstance.currentLevel,
                    currentExperience = empInstance.currentExperience,
                    skillPoints = empInstance.skillPoints,
                    currentSalary = empInstance.currentSalary,
                    currentCookingStat = empInstance.currentCookingStat,
                    currentServingStat = empInstance.currentServingStat,
                    currentCharmStat = empInstance.currentCharmStat,
                    assignedRole = empInstance.assignedRole,
                    grade = empInstance.grade,
                    isProtagonist = empInstance.isProtagonist,
                    traitNames = empInstance.currentTraits.Select(t => t.name).ToList()
                };
                data.employees.Add(empSaveData);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"[SaveManager] 게임 저장 완료! 경로: {saveFilePath}");
        if (NotificationController.instance != null)
            NotificationController.instance.ShowNotification("게임이 저장되었습니다.");
    }

    public bool LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("저장된 파일이 없습니다.");
            return false;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            loadedDataBuffer = JsonUtility.FromJson<SaveData>(json);
            
            Debug.Log("[SaveManager] 파일 로드 성공! (씬 전환 후 적용됩니다)");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 로드 중 오류 발생: {e.Message}");
            loadedDataBuffer = null;
            return false;
        }
    }

    private void ApplyLoadedData()
    {
        SaveData data = loadedDataBuffer;

        Debug.Log("[SaveManager] 게임 씬 로드 감지. 데이터를 매니저에 적용합니다.");

        if (GameManager.instance != null)
        {
            GameManager.instance.DayCount = data.dayCount;
            GameManager.instance.totalGoldAmount = data.gold;
            GameManager.instance.SpendGold(0); 
        }

        if (FameManager.instance != null)
        {
            // (FameManager에 SetFame 함수가 없으면 AddFame 등으로 값을 맞춰줘야 함)
            // 여기서는 프로퍼티에 값을 넣는다고 가정 (setter가 private면 수정 필요)
             // FameManager.instance.CurrentFamePoints = data.famePoints; 
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
                RecipeManager.instance.UnlockRecipe(recipeData.id);
            }
        }

        if (EmployeeManager.Instance != null && data.employees != null)
        {
            EmployeeManager.Instance.LoadHiredEmployees(data.employees);
        }

        loadedDataBuffer = null;
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
}