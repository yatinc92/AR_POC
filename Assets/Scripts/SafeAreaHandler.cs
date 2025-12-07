using UnityEngine;

public class SafeAreaHandler : MonoBehaviour
{
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        ApplySafeArea();
    }

    void Update()
    {
        Rect safeArea = Screen.safeArea;
        if (safeArea != lastSafeArea)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        // Convert to anchor coordinates
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply to all child panels that need safe area
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect != null && child.name.Contains("Panel"))
            {
                childRect.anchorMin = anchorMin;
                childRect.anchorMax = anchorMax;
            }
        }
    }
}