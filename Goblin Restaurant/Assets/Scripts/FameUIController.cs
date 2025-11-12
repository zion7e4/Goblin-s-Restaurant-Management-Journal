using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FameUIController : MonoBehaviour
{
    public List<Image> starImages;

    public Sprite inactiveStarSprite;
    public Sprite activeStarSprite;

    void OnEnable()
    {
        if (FameManager.instance != null)
        {
            FameManager.instance.OnFameChanged += UpdateFameUI;
            UpdateFameUI();
        }
    }

    void OnDisable()
    {
        if (FameManager.instance != null)
        {
            FameManager.instance.OnFameChanged -= UpdateFameUI;
        }
    }

    void UpdateFameUI()
    {
        if (FameManager.instance == null)
        {
            return;
        }

        int currentLevel = FameManager.instance.CurrentFameLevel;

        for (int i = 0; i < starImages.Count; i++)
        {
            if (i < currentLevel)
            {
                starImages[i].sprite = activeStarSprite;
            }
            else
            {
                starImages[i].sprite = inactiveStarSprite;
            }
        }
    }
}