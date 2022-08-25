using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(KeybindingButton)), CanEditMultipleObjects]
public class KeybindingButtonEditor : Editor
{
   public override void OnInspectorGUI()
   {
      /*
      KeybindingButton button = (KeybindingButton)target;

      button.timeout = EditorGUILayout.FloatField("Timeout", button.timeout);
      button.currentBindingText = EditorGUILayout.ObjectField("Current Binding Text", button.currentBindingText, typeof(Text), true) as Text;
      button.autoScroller = EditorGUILayout.ObjectField("Auto Scroller", button.autoScroller, typeof(AutoScrollContent), true) as AutoScrollContent;
      */
      
      //DrawDefaultInspector();
      base.OnInspectorGUI();
   }
}
