using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExtendableDrawBridge : MonoBehaviour, ISaveable
{
    public float endLength;
    public float moveSpeed = 1f;

    [Tooltip("This is only used to mark the switch as hit once this is extended")]
    public HittableSwitch drawbridgeSwitch;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private BoxCollider2D boxCollider;

    private bool isExtended = false;

    [SerializeField]
    private int saveID = -1;

    public int SaveIDNumber { get => saveID; set => saveID = value; }
    public bool DoneLoading { get; set; }

    private void Awake()
    {
        SaveManager.RegisterSaveable(this);

        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    public void ExtendDrawbridge()
    {
        if (!isExtended)
        {
            StartCoroutine(MoveDrawbridge());
            isExtended = true;

            Destroy(drawbridgeSwitch);
        }
    }

    private IEnumerator MoveDrawbridge()
    {
        // Play the audio
        audioSource.Play();
        while (boxCollider.size.y < endLength - moveSpeed * Time.deltaTime)
        {
            // Increase the drawbridge's size
            Vector2 size = boxCollider.size;
            size.y += moveSpeed * Time.deltaTime;

            boxCollider.size = size;

            // Increase the drawbridge's offset
            Vector2 offset = boxCollider.offset;
            offset.y = -boxCollider.size.y / 2;

            boxCollider.offset = offset;

            // Set the tiling height
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, boxCollider.size.y);

            yield return null;
        }

        // Put the drawbridge at the end state
        boxCollider.size = new Vector2(boxCollider.size.x, endLength);
        boxCollider.offset = new Vector2(boxCollider.offset.x, -endLength / 2);
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, endLength);

        // Stop the audio
        audioSource.Stop();
    }

    public WorldObjectSaveData GetSaveData()
    {
        DrawBridgeSaveData data = new DrawBridgeSaveData();
        data.isExtended = isExtended;

        return data;
    }

    public void LoadData(WorldObjectSaveData saveData)
    {
        DrawBridgeSaveData data = saveData as DrawBridgeSaveData;

        if (data.isExtended)
        {
            isExtended = true;

            // Put the drawbridge at the end state
            boxCollider.size = new Vector2(boxCollider.size.x, endLength);
            boxCollider.offset = new Vector2(boxCollider.offset.x, -endLength / 2);
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, endLength);

            drawbridgeSwitch.GetComponent<SpriteRenderer>().sprite = drawbridgeSwitch.onSprite;
            Destroy(drawbridgeSwitch);
        }

        DoneLoading = true;
    }

    [System.Serializable]
    public class DrawBridgeSaveData : WorldObjectSaveData
    {
        public bool isExtended;
    }

#if UNITY_EDITOR
    public void MarkAsDirty()
    {
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Changed saveID");
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif  
}
