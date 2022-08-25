using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeybindingButton : Button
{
    public float timeout = 3f;

    public Text currentBindingText;

    public AutoScrollContent autoScroller;

    public string inputActionName;

    private InputAction boundAction;
    
    private bool enableActionAfterBinding = false;
    
    protected override void Start()
    {
        base.Start();

        boundAction = CharacterSelector.GetPlayerController().GetComponent<PlayerInput>().actions[inputActionName];
    }
    
    public void RemapButtonClicked()
    {
        enableActionAfterBinding = boundAction.enabled;
        boundAction.Disable();
        
        var rebindOperation = boundAction.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithTimeout(timeout)
            .OnMatchWaitForAnother(0.1f)
            .Start();

        StartCoroutine(DisposeAfterRebind(rebindOperation));
    }

    /// <summary>
    /// Disposes the Rebind operation on completion, and updates the display text
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    private IEnumerator DisposeAfterRebind(InputActionRebindingExtensions.RebindingOperation operation)
    {
        while (!operation.completed) yield return null;
        
        string selectedBinding = boundAction.bindings[0].ToDisplayString();

        currentBindingText.text = "Current Binding : " + selectedBinding;

        operation.Dispose();

        if (enableActionAfterBinding) boundAction.Enable();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        
        autoScroller.SnapTo(GetComponent<RectTransform>());
    }
}
