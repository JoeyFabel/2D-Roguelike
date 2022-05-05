using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Collider Height Data", menuName = "Collider Height Data")]
public class ColliderHeightScriptableObject : ScriptableObject
{
    [Tooltip("Tiles with a collider on the left, meaning the player will always be left when hitting this tile")]
    public TileBase[] leftSideCollisionTiles;
    [Tooltip("Tiles with a collider on the right, meaning the player will always be right when hitting this tile")]
    public TileBase[] rightSideCollisionTiles;
    [Tooltip("Tiles with a collider on the top, meaning the player will always be above when hitting this tile")]
    public TileBase[] topSideCollisionTiles;
    [Tooltip("Tiles with a collider on the bottom, meaning the player will always be below when hitting this tile")]
    public TileBase[] bottomSideCollisionTiles;
    public TileBase[] noProjectileCollisionTiles;
}
