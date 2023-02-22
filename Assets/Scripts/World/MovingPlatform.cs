using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform[] targetPoints;
    public Transform movingPlatform;
    public bool startMoving;
    public bool startMovingForward;
    public int startTargetIndex;

    public bool oneWay = false;

    [Tooltip("The platform will wait until the ideal wait time finishes before it moves again, even if it already reached its destination")]
    public float idealLapTime;
    public float moveSpeed = 2;    

    protected bool isMoving;
    protected bool movingForward;
    protected bool completedOneWay;

    protected int currentTargetIndex;

    private const float AllowableError = 0.01f;

    protected float lapStartTime;

    private Collider2D trigger;

    protected virtual void Start()
    {
        isMoving = startMoving;
        movingForward = startMovingForward;
        currentTargetIndex = startTargetIndex;
        movingPlatform.gameObject.layer = LayerMask.NameToLayer("Moving Platform");
        completedOneWay = false;

        if (isMoving) lapStartTime = Time.time;
    }

    public virtual void StartMovement()
    {
        if (oneWay && completedOneWay) return;

        isMoving = true;
    }

    public virtual void EndMovement()
    {
        isMoving = false;
    }

    public void ToggleMovement()
    {
        if (oneWay && completedOneWay) return;

        if (isMoving) EndMovement();
        else StartMovement();
    }

    private void Update()
    {
        if (isMoving)
        {
            Vector3 toTarget = targetPoints[currentTargetIndex].position - movingPlatform.transform.position;

            Vector3 moveVector = moveSpeed * Time.deltaTime * toTarget.normalized;
            Vector3.ClampMagnitude(toTarget, toTarget.magnitude);

            movingPlatform.transform.position += moveVector;

            if ((movingPlatform.transform.position - targetPoints[currentTargetIndex].position).magnitude <= moveSpeed * Time.deltaTime)
            {
                movingPlatform.transform.position = targetPoints[currentTargetIndex].position;
                if (movingForward)
                {
                    if (currentTargetIndex < targetPoints.Length - 1) currentTargetIndex++;
                    else
                    {
                        if (oneWay)
                        {
                            print("one way finished!");
                            completedOneWay = true;
                            isMoving = false;
                            return;
                        }

                        currentTargetIndex -= 1;
                        movingForward = false;

                        StartCoroutine(WaitForLapToFinish(idealLapTime - (Time.time - lapStartTime)));
                    }
                }
                else
                {
                    if (currentTargetIndex > 0) currentTargetIndex--;
                    else
                    {
                        currentTargetIndex = 1;
                        movingForward = true;

                        if (oneWay)
                        {
                            print("one way finished!");
                            completedOneWay = true;
                            isMoving = false;
                            return;
                        }

                        StartCoroutine(WaitForLapToFinish(idealLapTime - (Time.time - lapStartTime)));
                    }
                }

            }
        }        
    }

    private IEnumerator WaitForLapToFinish(float waitTime)
    {
        isMoving = false;

        yield return new WaitForSeconds(waitTime);

        isMoving = true;

        lapStartTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            //player.transform.SetParent(movingPlatform.transform);
            // ignore holes
        }
        else if (collision.TryGetComponent(out Enemy enemy))
        {
            enemy.transform.SetParent(movingPlatform.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            player.transform.SetParent(null);
        }
        else if (collision.TryGetComponent(out Enemy enemy))
        {
            enemy.transform.SetParent(null);
        }
    }

#if UNITY_EDITOR
    public float moveTime;

    [ContextMenu("Set moveSpeed from time to complete full trip")]
    public void CalculateMoveSpeed()
    {
        float distanceToTravel = 0f;

        for (int i = 0; i < targetPoints.Length - 1; i++)
        {
            float distanceForSegment = (targetPoints[i + 1].position - targetPoints[i].position).magnitude;

            distanceToTravel += distanceForSegment;
        }

        moveSpeed = distanceToTravel / moveTime;
    }

    [ContextMenu("Set idealLapTime from speed")]
    public void CalculateLapTimeFromSpeed()
    {
        float distanceToTravel = 0f;

        for (int i = 0; i < targetPoints.Length - 1; i++)
        {
            float distanceForSegment = (targetPoints[i + 1].position - targetPoints[i].position).magnitude;

            distanceToTravel += distanceForSegment;
        }

        idealLapTime = distanceToTravel / moveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 platformOffset = movingPlatform.GetComponent<Collider2D>().bounds.center - movingPlatform.transform.position;
        Vector3 platformSize = movingPlatform.GetComponent<Collider2D>().bounds.size;
        Gizmos.color = Color.blue;
        
        for (int i = 0; i < targetPoints.Length - 1; i++)
        {
            Vector3 toEnd = ((targetPoints[i + 1].position + platformOffset) -
                            (targetPoints[i].position + platformOffset)).normalized;
            //Gizmos.DrawLine(targetPoints[i].position + platformOffset, targetPoints[i + 1].position + platformOffset - (toEnd * platformSize.x / 2));
            Gizmos.DrawLine(targetPoints[i].position + platformOffset, targetPoints[i + 1].position - (toEnd * platformSize.x / 2));
            Gizmos.DrawWireCube(targetPoints[i + 1].position + platformOffset, platformSize);
        }
    }
#endif
}
