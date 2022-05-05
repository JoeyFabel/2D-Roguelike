using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Tile Data", menuName = "Tile Data")]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;


    [Tooltip("How is the walking speed affected by this tile?")]
    public float walkingSpeedMultiplier = 1;

    [Tooltip("How high is this tile, for collision purposes?")]
    public int layerHeight;

    public AudioClip[] footstepSounds;
}
