using UnityEngine;
using UnityEngine.UI;

public class OrderIconUI : MonoBehaviour
{
    public Image iconImage;

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null && sprite != null)
        {
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
        }
    }
}