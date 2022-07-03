using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterZone : MonoBehaviour
{
    [Tooltip("The higher the number, the farther characters sink in the water, meaning the higher the water effect appears"), Range(0f, 1f)]
    public float waterDepth = 0.5f;
    public float waterOpacity = 7f / 8f;

    public Sprite waterDecal;

    public ContactFilter2D contactFilter;

    // The objects with the decal will always be the parent
    List<(GameObject, BoxCollider2D)> decalColliderInstances;

    Collider2D trigger;

    private void Start()
    {
        trigger = GetComponent<Collider2D>();

        decalColliderInstances = new List<(GameObject, BoxCollider2D)>();

        List<Collider2D> results = new List<Collider2D>();

        if (trigger.OverlapCollider(contactFilter, results) > 0)
        {
            foreach (var obj in results) ApplyWaterDecal(obj.gameObject);
        }
    }

    private void Update()
    {
        foreach (var decalColliderPair in decalColliderInstances)
        {
            decalColliderPair.Item1.SetActive(InsideCol(decalColliderPair.Item2, trigger));

            
            Vector3 clampedPosition = decalColliderPair.Item2.transform.position;

            float yPos = clampedPosition.y + decalColliderPair.Item2.size.y;

            // limit the x and y positions to be between the area's min and max x and y.
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, trigger.bounds.min.x + decalColliderPair.Item2.size.x, trigger.bounds.max.x - decalColliderPair.Item2.size.x);
            clampedPosition.y = Mathf.Clamp(yPos, trigger.bounds.min.y + decalColliderPair.Item2.size.y / 2, trigger.bounds.max.y - decalColliderPair.Item2.offset.y);
            
            //decalColliderPair.Item1.transform.position = clampedPosition;
            decalColliderPair.Item1.transform.position = new Vector3(clampedPosition.x, yPos);

            /*
            if (negativeYDifferential > 0)
            {
                Vector3 newScale = decalColliderPair.Item1.transform.localScale;
                newScale.y += negativeYDifferential;

                decalColliderPair.Item1.transform.localScale = newScale;

                Vector3 newPosition = decalColliderPair.Item1.transform.position;
                newPosition.y -= negativeYDifferential;

                decalColliderPair.Item1.transform.position = newPosition;
            } */
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ApplyWaterDecal(collision.gameObject);
        print("applying water decal to " + collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        RemoveWaterDecal(collision.gameObject);        
    }

    private void RemoveWaterDecal(GameObject objectInWater)
    {
        int indexToRemove = -1;

        for (int i = 0; i < decalColliderInstances.Count; i++)
        {
            if (decalColliderInstances[i].Item1.transform.parent.gameObject.Equals(objectInWater))
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove >= 0)
        {
            Destroy(decalColliderInstances[indexToRemove].Item1);
            decalColliderInstances.RemoveAt(indexToRemove);
        }
    }

    private void ApplyWaterDecal(GameObject objectInWater)
    {
        SpriteRenderer decal = new GameObject("Water Decal").AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;

        Color decalColor = decal.color;
        decalColor.a = waterOpacity;
        decal.color = decalColor;

        decal.sprite = waterDecal;
        decal.sortingOrder = 1;

        decal.transform.SetParent(objectInWater.transform);
        decal.transform.position = Vector3.zero;

        BoxCollider2D objectInWaterCollider = objectInWater.GetComponent<BoxCollider2D>();
        float yPos = objectInWaterCollider.offset.y - (objectInWaterCollider.size.y / 2f);
        decal.transform.position += Vector3.up * yPos;

        Vector3 decalScale = new Vector3(1f, waterDepth, 1f);
        decal.transform.localScale = decalScale;        

        decalColliderInstances.Add((decal.gameObject, objectInWaterCollider));
    }

    private bool InsideCol(Collider2D mycol, Collider2D other)
    {
        if (other.bounds.min.x <= mycol.bounds.min.x && other.bounds.min.y <= mycol.bounds.min.y &&
             other.bounds.max.x >= mycol.bounds.max.x && other.bounds.max.y >= mycol.bounds.max.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
