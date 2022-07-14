using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogTree : SaveableObject, IInteractable
{
    public DialogNode startingNode;

    public Sprite speakerIcon;

    public GameObject speechBubble;
    private Text text;

    private DialogNode currentNode;
    [SerializeField]
    private int currentNodeID = -1;

    PlayerController player;

    // Start is called before the first frame update
    protected override void Start()
    {
        if (started) return;

        player = CharacterSelector.GetPlayerController();

        if (saveData == null) currentNode = startingNode;

        started = true;
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
                print(currentNodeID + ", " + currentNode.id);
                startingNode = currentNode;
                Debug.Log("changing starting node!");
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

    protected override void LoadData()
    {
        DialogTreeSaveData data = saveData as DialogTreeSaveData;

        //PrimaryNode[] possibleNodes = GetComponentsInChildren<PrimaryNode>();
        List<DialogNode> possibleNodes = new List<DialogNode>(GetComponentsInChildren<DialogNode>());

        // only have primary nodes
        possibleNodes.RemoveAll((DialogNode node) => !node.isPrimaryNode);        

        print("loading data for " + gameObject.name);

        currentNodeID = data.startingPrimaryNodeID;

        if (data.startingPrimaryNodeID < 0)
        {
            isDoneLoading = true;

            return;
        }

        foreach (var node in possibleNodes)
        {
            print("    searching for starting node");
            if (node.isPrimaryNode && node.id == data.startingPrimaryNodeID)
            {
                startingNode = node;
                currentNode = node;

                print("    Found the starting dialog node (" + node.dialog + ")");

                break;
            }
        }

        isDoneLoading = true;
    }

    public override WorldObjectSaveData GetSaveData()
    {
        DialogTreeSaveData data = new DialogTreeSaveData();

        data.startingPrimaryNodeID = currentNodeID;

        return data;
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
#endif
}
