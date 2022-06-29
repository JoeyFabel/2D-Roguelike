using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HittableSwitch : Damageable
{
    public UnityEvent OnSwitchHit;

    public Sprite offSprite;
    public Sprite onSprite;

    // Could implement numStates
    // with 1 action for each state

    SpriteRenderer spriteRenderer;

    [SerializeField]
    private bool switchOn;

    protected override void Start()
    {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = switchOn ? onSprite : offSprite;
    }

    public override void ApplyDamage(float amount)
    {
        OnHit();
    }

    protected override void Death()
    {
        Debug.LogError(this + " died! That should not happen!");
    }

    private void OnHit()
    {
        OnSwitchHit.Invoke();

        switchOn = !switchOn;

        spriteRenderer.sprite = switchOn ? onSprite : offSprite;
    }
}

// custom editor layout only in editor
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(HittableSwitch))]
[UnityEditor.CanEditMultipleObjects]
public class HittableSwitchEditor : UnityEditor.Editor
{
    UnityEditor.SerializedProperty OnSwitchHit;
    UnityEditor.SerializedProperty offSprite;
    UnityEditor.SerializedProperty onSprite;
    UnityEditor.SerializedProperty switchOn;

    private void OnEnable()
    {
        OnSwitchHit = serializedObject.FindProperty("OnSwitchHit");
        offSprite = serializedObject.FindProperty("offSprite");
        onSprite = serializedObject.FindProperty("onSprite");
        switchOn = serializedObject.FindProperty("switchOn");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        UnityEditor.EditorGUILayout.PropertyField(OnSwitchHit);
        UnityEditor.EditorGUILayout.PropertyField(offSprite);
        UnityEditor.EditorGUILayout.PropertyField(onSprite);
        UnityEditor.EditorGUILayout.PropertyField(switchOn);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
