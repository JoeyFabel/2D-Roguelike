using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public GameObject[] playableCharacters;

    [SerializeField]
    private string playerCharacterName;

    private static PlayerController player;

    public void SpawnCharacter()
    {
        GameObject playerCharacter = null;

        foreach(var character in playableCharacters)
        {
            if (character.name.Equals(playerCharacterName))
            {
                print("Spawning a " + playerCharacterName);
                playerCharacter = Instantiate(character);
                break;
            }
        }

        player = playerCharacter.GetComponent<PlayerController>();

        // Load datas
    }

    public void SetCharacterName(string newPlayerName)
    {
        playerCharacterName = newPlayerName;
    }

    public string GetCharacterName()
    {
        return playerCharacterName;
    }

    public static PlayerController GetPlayerController()
    {
        return player;
    }

    public void UseDefaultCharacter()
    {
        playerCharacterName = playableCharacters[0].name;
    }
}
