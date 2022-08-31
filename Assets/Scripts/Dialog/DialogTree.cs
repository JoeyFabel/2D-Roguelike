    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

public class DialogTree : MonoBehaviour, IInteractable, ISaveable
{
    [Tooltip("The initial dialog node.")]
    public DialogNode startingNode;

    [Tooltip("The icon to be displayed while the character is talking")]
    public Sprite speakerIcon;

    [Header("Ready to Talk Options")]
    public GameObject speechBubbleIcon;
    public bool faceTowardsPlayer = true;
    
    private Text text;

    private DialogNode currentNode;
    [SerializeField]
    private int currentNodeID = -1;

    private SpriteRenderer sprite;
    protected PlayerController player;
    private static readonly int toPlayerYProperty = Animator.StringToHash("To Player Y");
    private Coroutine watchPlayerRoutine;

    private Animator animator;

    private bool started = false;
    private bool hasSaveData = false;

    [SerializeField] private int saveID = -1;
    public int SaveIDNumber {get => saveID; set => saveID = value; }
    
    public bool DoneLoading { get; set; }

    private void Awake()
    {
        SaveManager.RegisterSaveable(this);
    }

    private void OnDestroy()
    {
        SaveManager.UnRegisterSaveable(this);
    }

    protected virtual void Start()
    {
        if (started) return;

        player = CharacterSelector.GetPlayerController();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (!hasSaveData) currentNode = startingNode;        

        speechBubbleIcon.SetActive(false);

        started = true;
    }

    public void SetSpeechBubblePosition(Transform newPosition)
    {
        speechBubbleIcon.transform.position = newPosition.position;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out PlayerController player))
        {
            speechBubbleIcon.SetActive((true));
            watchPlayerRoutine = StartCoroutine((WatchPlayer()));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            speechBubbleIcon.SetActive(false);
            if (watchPlayerRoutine != null) StopCoroutine(watchPlayerRoutine);
        }
    }

    public void SetFacePlayer(bool enabled)
    {
        faceTowardsPlayer = enabled;
    }

    private IEnumerator WatchPlayer()
    {
        // Face towards the player if enabled
        while (faceTowardsPlayer)
        {
            Vector2 towardsPlayer = player.transform.position - transform.position;

            sprite.flipX = towardsPlayer.x < 0;
            animator.SetFloat(toPlayerYProperty, towardsPlayer.y);
            
            yield return null;
        }
    }
    
    public void Interact()
    {
        // If the dialog bubble is closed, open the current starting dialog node
        if (!DialogManager.IsSpeechBubbleEnabled())
        {
            currentNode = startingNode;

            //speechBubble.SetActive(true);
            //text.text = currentNode.dialog;
            DialogManager.SetSpeakerIcon(speakerIcon);
            DialogManager.DisplayDialog(currentNode);

            player.DisableControlsForDialog();

            // See if on a primary node
            if (currentNode.isPrimaryNode)
            {
                currentNodeID = currentNode.id;
                startingNode = currentNode;
                Debug.Log("changing starting node!");
            }
            else if (currentNode is EventNode)
            {
                EventNode eventNode = currentNode as EventNode;
                eventNode?.action?.Invoke();
                print("running event actions");
            }
        }
        else if (DialogManager.isTyping)
        {
            DialogManager.FinishDialogLine();
        }
        else if (currentNode is ExitNode)
        {
            startingNode = currentNode.GetNextNode();

            //speechBubble.SetActive(false);
            DialogManager.CloseDialog();

            player.EnableControlsAfterUI();
        }
        else
        {
            currentNode = currentNode.GetNextNode();

            if (currentNode != null)
            {
                //text.text = currentNode.dialog;
                DialogManager.DisplayDialog(currentNode);

                if (currentNode.isPrimaryNode)
                {
                    currentNodeID = currentNode.id;
                    print(currentNodeID + ", " + currentNode.id);
                    startingNode = currentNode;
                    Debug.Log("changing starting node!");
                }
                else if (currentNode is EventNode)
                {
                    EventNode eventNode = currentNode as EventNode;
                    eventNode?.action?.Invoke();
                    print("running event actions");
                }
            }
            else
            {
                //speechBubble.SetActive(false);
                DialogManager.CloseDialog();

                player.EnableControlsAfterUI();
            }
        }
    }

    /*
    public override WorldObjectSaveData GetSaveData()
    {
        throw new System.NotImplementedException();
    }

    protected override void LoadData()
    {
        throw new System.NotImplementedException();
    }
    */

    public virtual void LoadData(WorldObjectSaveData saveData)
    {
        // Start the object if it hasn't started already
        hasSaveData = true;
        if (!started) Start();
        
        DialogTreeSaveData data = saveData as DialogTreeSaveData;

        //PrimaryNode[] possibleNodes = GetComponentsInChildren<PrimaryNode>();
        List<DialogNode> possibleNodes = new List<DialogNode>(GetComponentsInChildren<DialogNode>());

        // only have primary nodes
        possibleNodes.RemoveAll((DialogNode node) => !node.isPrimaryNode);        

        print("loading data for " + gameObject.name);

        currentNodeID = data.startingPrimaryNodeID;
        print("starting node id is " + currentNodeID);

        if (data.startingPrimaryNodeID < 0)
        {
            DoneLoading = true;

            return;
        }

        foreach (var node in possibleNodes)
        {
            // print("    searching for starting node");
            if (!node.isPrimaryNode || node.id != data.startingPrimaryNodeID) continue;
            
            startingNode = node;
            currentNode = node;

            print("    Found the starting dialog node (" + node.dialog + ")");

            break;
        }        

        DoneLoading = true;
    }

    public virtual WorldObjectSaveData GetSaveData()
    {
        print("saving dialog tree");
        DialogTreeSaveData data = new DialogTreeSaveData();

        data.startingPrimaryNodeID = currentNodeID;

        return data;
    }

    public string GetSaveID()
    {
        return GameManager.GetCurrentSceneName() + ": " + saveID;
    }
    
    [System.Serializable]
    public class DialogTreeSaveData : WorldObjectSaveData
    {
        public int startingPrimaryNodeID;
    }

#if UNITY_EDITOR
    [ContextMenu("Assign IDs to Primary Nodes")]
    private void AssignPrimaryNodeIDs()
    {
        //PrimaryNode[] possibleNodes = GetComponentsInChildren<PrimaryNode>();
        List<DialogNode> possibleNodes = new List<DialogNode>(GetComponentsInChildren<DialogNode>());

        // only have primary nodes
        possibleNodes.RemoveAll((DialogNode node) => !node.isPrimaryNode);

        for (int i = 0; i < possibleNodes.Count; i++) possibleNodes[i].id = i;
    }

    private void Reset()
    {
        Debug.Log("Dialog Tree added to " + gameObject.name + ", Initializing!");

        GameObject dialogHelper = new GameObject("Dialog Helper", typeof(DialogNode));
        dialogHelper.transform.SetParent(transform);

        startingNode = dialogHelper.GetComponent<DialogNode>();
    }
    public void MarkAsDirty()
    {
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Changed saveID");
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}
