using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableCharacter : MonoBehaviour, IInteractable
{
    public string characterName;

    //protected override void Start()
    private void Start()
    {
        if (GameManager.GetPlayerCharacterName().Equals(characterName))
        {            
            Destroy(gameObject);
            return;
        }        
    }

    public void Interact()
    {
        GameManager.SwitchToCharacter(characterName);
    }
}
