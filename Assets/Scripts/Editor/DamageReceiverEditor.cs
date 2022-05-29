using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//#if UNITY_EDITOR


[CustomEditor(typeof(DamageReceiver))]
public class DamageReceiverEditor : Editor
{

    SerializedProperty objectToDamage;
    SerializedProperty damageMultiplier;

    void OnEnable()
    {
        objectToDamage = serializedObject.FindProperty("objectToDamage");
        damageMultiplier = serializedObject.FindProperty("damageMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(objectToDamage);
        EditorGUILayout.PropertyField(damageMultiplier);
        
        serializedObject.ApplyModifiedProperties();
    }

}
