using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollElement : Selectable
{
    [SerializeField]
    public AutoScrollContent autoScroller;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
     print("todo - replace this with the keybinding change script");   
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        
        autoScroller.SnapTo(GetComponent<RectTransform>());
    }
}
