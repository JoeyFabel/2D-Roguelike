using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(DialogNode), true), CanEditMultipleObjects]
public class DialogNodeEditor : Editor
{
    SerializedProperty isPrimaryNodeProp;
    SerializedProperty nodeIDProp;

    private void OnEnable()
    {
        isPrimaryNodeProp = serializedObject.FindProperty("isPrimaryNode");
        nodeIDProp = serializedObject.FindProperty("id");
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, "isPrimaryNode", "id", "m_Script");

        EditorGUILayout.PropertyField(isPrimaryNodeProp);

        if (isPrimaryNodeProp.boolValue)
        {
            EditorGUILayout.PropertyField(nodeIDProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
