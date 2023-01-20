using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField, Tooltip("Note - the first tileData in the list is used for default values")] private List<TileData> tileDatas;
    [SerializeField] private List<ColliderHeightScriptableObject> tileColliderDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;
    private Dictionary<TileBase, TileCollisionData> collisionDataFromTiles;

    private Tilemap tileMap;

    private void Awake()
    {
        tileMap = GameObject.FindWithTag("Ground").GetComponent<Tilemap>();
        if (!tileMap) Debug.LogError("Map Manager did not find a tile map with the 'Ground' tag!");

        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles) if (!dataFromTiles.ContainsKey(tile))
                {
                    dataFromTiles.Add(tile, tileData);
                }
        }

        collisionDataFromTiles = new Dictionary<TileBase, TileCollisionData>();

        foreach (var tileData in tileColliderDatas)
        {
            foreach (var tile in tileData.leftSideCollisionTiles)
            {
                collisionDataFromTiles.Add(tile, new TileCollisionData());
                collisionDataFromTiles[tile].leftSideCollisions = true;
            }
            
            foreach (var tile in tileData.rightSideCollisionTiles)
            {
                if (!collisionDataFromTiles.ContainsKey(tile)) collisionDataFromTiles.Add(tile, new TileCollisionData());
                collisionDataFromTiles[tile].rightSideCollision = true;
            }

            foreach (var tile in tileData.topSideCollisionTiles)
            {
                if (!collisionDataFromTiles.ContainsKey(tile)) collisionDataFromTiles.Add(tile, new TileCollisionData());
                collisionDataFromTiles[tile].topSideCollisions = true;
            }

            foreach (var tile in tileData.bottomSideCollisionTiles)
            {
                if (!collisionDataFromTiles.ContainsKey(tile)) collisionDataFromTiles.Add(tile, new TileCollisionData());
                collisionDataFromTiles[tile].bottomSideCollisions = true;
            }

            foreach (var tile in tileData.noProjectileCollisionTiles)
            {
                if (!collisionDataFromTiles.ContainsKey(tile)) collisionDataFromTiles.Add(tile, new TileCollisionData());
                collisionDataFromTiles[tile].noCollisions = true;
            }
        }
    }

    public AudioClip GetFootstepSound(Vector2 position, out bool foundTile)
    {
        TileBase groundTile = tileMap.GetTile(tileMap.WorldToCell(position));
        foundTile = false;

        if (groundTile == null)
        {
            Debug.LogWarning("There is no tile at position " + position);            
            return null;
        }

        if (dataFromTiles.ContainsKey(groundTile))
        {
            foundTile = true;
            return dataFromTiles[groundTile].footstepSounds[Random.Range(0, dataFromTiles[groundTile].footstepSounds.Length)];
        }
        else return tileDatas[0].footstepSounds[Random.Range(0, tileDatas[0].footstepSounds.Length)];        
    }

    public float getMovementMultiplier(Vector2 position)
    {
        TileBase groundTile = tileMap.GetTile(tileMap.WorldToCell(position));

        if (groundTile == null)
        {
            Debug.LogWarning("There is no tile at position " + position);
            return 1f;
        }

        if (dataFromTiles.ContainsKey(groundTile))
        {
         //   print("movement multiplier is " + dataFromTiles[groundTile].walkingSpeedMultiplier + " for " + groundTile);
            return dataFromTiles[groundTile].walkingSpeedMultiplier;
        }
        else
        {
        //    print("no key for " + groundTile);
            return 1f;
        }
    }

    public TileCollisionData GetTileCollisionData(Vector2 position)
    {
        TileBase hitTile = tileMap.GetTile(tileMap.WorldToCell(position));
        if (hitTile is null) return null;
        

        if (collisionDataFromTiles.ContainsKey(hitTile))
        {
            return collisionDataFromTiles[hitTile];
        }
        else return null;
    }

    public class TileCollisionData
    {
        public bool leftSideCollisions;
        public bool rightSideCollision;
        public bool topSideCollisions;
        public bool bottomSideCollisions;
        public bool noCollisions;

        public TileCollisionData()
        {
            leftSideCollisions = false;
            rightSideCollision = false;
            topSideCollisions = false;
            bottomSideCollisions = false;
            noCollisions = false;
        }

        public new string ToString()
        {
            string output = "";

            if (leftSideCollisions) output += "Left side collision. ";
            if (rightSideCollision) output += "Right side collision. ";
            if (topSideCollisions) output += "Top side collision. ";
            if (bottomSideCollisions) output += "Bottom side collision. ";

            if (!leftSideCollisions && !rightSideCollision && !topSideCollisions && !bottomSideCollisions) return "No special collision sides.";
            else if (noCollisions) return "but projectile collisions are ignored for this tile";
            else return output;
        }
    }
}