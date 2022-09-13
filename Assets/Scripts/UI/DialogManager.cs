using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public Image speakerImage;
    public Text speechText;

    public Transform choiceOptionParent;

    public GameObject dialogOptionPrefab;

    public float defaultCharactersPerSecond = 15f;
    private float charactersPerSecond = 15f;
    
    private static DialogManager instance;

    private List<DialogOption> dialogOptions;

    public static bool isTyping;
    private string targetText;

    public void InitializeDialogManager()
    {
        instance = this;

        dialogOptions = new List<DialogOption>();

        instance.gameObject.SetActive(false);
    }

    public static void SetSpeakerIcon(Sprite speakerIcon)
    {
        instance.speakerImage.sprite = speakerIcon;
    }

    public static void DisplayDialog(DialogNode dialogNode)
    {
        DisplayDialog(dialogNode, instance.defaultCharactersPerSecond);
    }

    public static void DisplayDialog(DialogNode dialogNode, float charactersPerSecond)
    {
        instance.charactersPerSecond = charactersPerSecond;
        
        instance.gameObject.SetActive(true);

        // Remove any old dialog options       
        for (int i = instance.dialogOptions.Count - 1; i >= 0; i--)
        {
            Destroy(instance.dialogOptions[i].gameObject);

            instance.dialogOptions.RemoveAt(i);
        }

        //instance.speechText.text = dialogNode.dialog;

        instance.StartCoroutine(instance.TypeInDialog(dialogNode.dialog));

        if (dialogNode is ForkingNode)
        {
            ForkingNode forkedNode = dialogNode as ForkingNode;
            //foreach (var option in forkedNode.nextNodes)
            for (int i = 0; i < forkedNode.nextNodes.Length; i++)
            {
                if (forkedNode.nextNodes[i].requiredItem == null || Inventory.PlayerHasItem(forkedNode.nextNodes[i].requiredItem))
                {
                    DialogOption choiceButton = Instantiate(instance.dialogOptionPrefab, instance.choiceOptionParent).GetComponent<DialogOption>();

                    choiceButton.Initialize(forkedNode.nextNodes[i].displayText, i, forkedNode);

                    instance.dialogOptions.Add(choiceButton);
                }
            }
            
            instance.dialogOptions[0].Select();
        }         
    }

    private IEnumerator TypeInDialog(string dialog)
    {
        instance.speechText.text = "";
        targetText = dialog;

        isTyping = true;

        int currentCharacterIndex = 0;
        int blanksBefore = dialog.Length;

        while (!instance.speechText.text.Equals(dialog))
        {
            instance.speechText.text += dialog[currentCharacterIndex];

            currentCharacterIndex++;

            yield return new WaitForSecondsRealtime(1f / charactersPerSecond);
        }

        isTyping = false;
    }

    public static void FinishDialogLine()
    {
        instance.StopAllCoroutines();
        instance.speechText.text = instance.targetText;

        isTyping = false;
    }

    public static void CloseDialog()
    {
        instance.gameObject.SetActive(false);
    }

    public static bool IsSpeechBubbleEnabled()
    {
        return instance.gameObject.activeSelf;
    }
}
