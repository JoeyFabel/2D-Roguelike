using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Acts like a button and is a Selectable
public class DialogOption : Selectable
{
    public Text text;

    private int optionID;

    ForkingNode parent;

    public void Initialize(string optionText, int optionID, ForkingNode parentNode)
    {
        text.text = optionText;

        this.optionID = optionID;

        parent = parentNode;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        parent.SelectOption(optionID);
    }
}
