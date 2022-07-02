using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSpell : Spell
{
    public int healAmount;

    public override void OnCast(Vector2 lookDirection, GameObject player)
    {
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        player.GetComponent<PlayerController>().GainHealth(healAmount);

        Destroy(gameObject, 1f);
    }
}
