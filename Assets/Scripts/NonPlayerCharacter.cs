using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    public float displayTime = 4.0f;
    public GameObject dialogBox;

    private void Start()
    {
        dialogBox.SetActive(false);

        gameObject.layer = LayerMask.NameToLayer("NPC");
    }

    public void DisplayDialogue()
    {
        dialogBox.SetActive(true);

        StartCoroutine(DisableDialogueBox(displayTime));
    }

    private IEnumerator DisableDialogueBox(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        dialogBox.SetActive(false);
    }    
}
