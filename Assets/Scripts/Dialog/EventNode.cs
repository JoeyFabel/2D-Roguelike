using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventNode : DialogNode
{
    [Tooltip("This action is performed when this dialog screen is loaded")]
    public UnityEvent action;
}
