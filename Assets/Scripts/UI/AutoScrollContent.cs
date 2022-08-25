using UnityEngine;

public class AutoScrollContent : MonoBehaviour
{
    public RectTransform scrollRectTransform;

    public RectTransform contentPanel;
    
    [SerializeField]
    private RectTransform oldRect;
    
    public void SnapTo(RectTransform target)
    {
        Vector2 position = target.position; // Get the position of the selected content

        bool inView = RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, position); // See if the selected content is visible

        float incrementSize = target.rect.height; // Get the height of the content item

        if (!inView && oldRect != null) // If the item is not visible and there was an old rect
        {
            // If the new rect is too high, move the view downward
            if (oldRect.position.y < target.position.y) contentPanel.anchoredPosition += new Vector2(0, -incrementSize); 
            // If the new rect is too low, move the view upward
            else if (oldRect.position.y > target.position.y) contentPanel.anchoredPosition += new Vector2(0, incrementSize); 
        }

        // Assign the old rect
        oldRect = target;
    }

    public void ResetPosition()
    {
        contentPanel.anchoredPosition = Vector2.zero;
    }
}
