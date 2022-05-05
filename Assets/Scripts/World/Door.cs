using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 moveDistance;

    public float moveSpeed;
    // Note - start door in closed position   

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = transform.position + moveDistance;

        transform.position = openPosition;
    }

    public void CloseDoor()
    {
        StartCoroutine(MoveDoor(closedPosition));
    }

    public void OpenDoor()
    {
        StartCoroutine(MoveDoor(openPosition));
    }

    private IEnumerator MoveDoor(Vector3 targetPosition)
    {
        while (Mathf.Abs((targetPosition - transform.position).magnitude) > moveSpeed * Time.deltaTime)
        {
            transform.position += moveSpeed * Time.deltaTime * (targetPosition - transform.position).normalized;

            yield return null;
        }

        transform.position = targetPosition;
    }
}
