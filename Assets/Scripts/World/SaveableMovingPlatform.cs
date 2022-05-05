using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveableMovingPlatform : SaveableObject
{
    #region MoveingPlatform code
    public Transform[] targetPoints;
    public Transform movingPlatform;
    public bool startMoving;
    public bool startMovingForward;
    public int startTargetIndex;

    public bool oneWay = false;
    public bool savePosition = false;

    [Tooltip("The platform will wait until the ideal wait time finishes before it moves again, even if it already reached its destination")]
    public float idealLapTime;
    public float moveSpeed = 2;

    private bool isMoving;
    private bool movingForward;
    private bool completedOneWay;

    private int currentTargetIndex;

    private const float AllowableError = 0.01f;

    private float lapStartTime;

    private bool loadedSave;

    private AudioSource audioSource;

    protected override void Start()
    {
        if (started) return;

        movingPlatform.gameObject.layer = LayerMask.NameToLayer("Moving Platform");

        if (loadedSave) return;
        
        isMoving = startMoving;
        movingForward = startMovingForward;
        currentTargetIndex = startTargetIndex;
        completedOneWay = false;

        TryGetComponent<AudioSource>(out audioSource);

        if (isMoving) lapStartTime = Time.time;

        started = true;
    }

    public void StartMovement()
    {
        if (oneWay && completedOneWay) return;

        isMoving = true;
        if (audioSource) audioSource.Play();
    }

    public void EndMovement()
    {
        isMoving = false;
        if (audioSource) audioSource.Stop();
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
                            if (audioSource) audioSource.Stop();

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

                            if (audioSource) audioSource.Stop();
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
#endregion

    public override WorldObjectSaveData GetSaveData()
    {
        MovingPlatformSaveData saveData = new MovingPlatformSaveData();

        saveData.isActivated = isMoving;
        saveData.completedOneWay = completedOneWay;

        if (savePosition)
        {
            saveData.currentPosition = movingPlatform.transform.position;
            saveData.nextIndex = currentTargetIndex;
            saveData.movingForward = movingForward;
        }

        return saveData;
    }

    protected override void LoadData()
    {
        MovingPlatformSaveData data = saveData as MovingPlatformSaveData;

        if (data == null)
        {
            Debug.LogError("ERROR -- " + gameObject + " did not get a valid save data!", gameObject);
            return;
        }

        if (data.completedOneWay) // Good
        {
            currentTargetIndex = targetPoints.Length - 1;

            movingPlatform.transform.position = targetPoints[currentTargetIndex].position;
            completedOneWay = true;
            isMoving = false;
        }
        else if (savePosition) // Good
        {
            movingPlatform.transform.position = data.currentPosition;
            currentTargetIndex = data.nextIndex;
            movingForward = data.movingForward;

            isMoving = data.isActivated;
        }
        else if (data.isActivated) 
        {
            isMoving = true;
            movingForward = startMovingForward;
            currentTargetIndex = startTargetIndex;
            completedOneWay = false;

            lapStartTime = Time.time;
        }

        loadedSave = true;
        isDoneLoading = true;
    }

    #region Editor Functions
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
#endif
#endregion

    [System.Serializable]
    public class MovingPlatformSaveData : WorldObjectSaveData
    {
        public bool completedOneWay;
        public bool isActivated;
        
        // Only for platforms that have stopped moving
        public SerializableVector3 currentPosition;
        public int nextIndex;
        public bool movingForward;
    }
}
