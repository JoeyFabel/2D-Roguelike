using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (oldRect.localPosition.y < target.localPosition.y) contentPanel.anchoredPosition += new Vector2(0, -incrementSize);
            // If the new rect is too low, move the view upward
            else if (oldRect.localPosition.y > target.localPosition.y) contentPanel.anchoredPosition += new Vector2(0, incrementSize);
        }

        // Assign the old rect
        oldRect = target;
    }
    
}
