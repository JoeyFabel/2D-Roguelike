using UnityEngine;

[System.Serializable]
/// <summary>
/// A <see cref="DialogNode"/> is the most basic part of a character dialog, and multiple nodes are linked together to make a conversation
/// </summary>
public class DialogNode : UnityEngine.MonoBehaviour
{
    [TextArea]
    public string dialog;
    [SerializeField]
    public DialogNode nextNode;

    // primary node class not needed
    public bool isPrimaryNode = false;
    public int id;

    public virtual DialogNode GetNextNode()
    {
        return nextNode;
    }
}
