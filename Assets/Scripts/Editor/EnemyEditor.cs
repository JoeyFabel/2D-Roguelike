using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Enemy), true), CanEditMultipleObjects]
public class EnemyEditor : Editor
{
    SerializedProperty prop;

    private bool showEnemyFields;

    private void OnEnable()
    {
        prop = serializedObject.FindProperty("");

        showEnemyFields = false;

    }

    public override void OnInspectorGUI()
    {
        showEnemyFields = EditorGUILayout.Foldout(showEnemyFields, "Base Enemy Values");

        
        if (showEnemyFields)
        {
            EditorGUI.indentLevel++;

            base.DrawDefaultInspector();

            EditorGUI.indentLevel--;            
        } 

        if (target.GetType().IsSubclassOf(typeof(Enemy)))
        {
            EditorGUILayout.LabelField(target.GetType().ToString() + " Values", EditorStyles.boldLabel);

            // Draw the rest of the properties
            DrawPropertiesExcluding(serializedObject, "damage", "damageType", "xpForDefeating", "moneyForDefeating", "moneyDropChance", "maxHealth", "damageSounds", "deathSounds", "fireballImpactSound", "slashImpactSound", "thrustImpactSound", "bluntImpactSound", "magicImpactSound", "resistances", "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
