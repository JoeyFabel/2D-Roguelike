using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spell : MonoBehaviour
{
    public abstract void OnCast(Vector2 lookDirection, GameObject caster);
}
