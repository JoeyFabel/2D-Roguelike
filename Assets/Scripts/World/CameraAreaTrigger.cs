using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAreaTrigger : MonoBehaviour
{
    public GameObject cameraCollider;

    private void Start()
    {
        var trigger = GetComponent<BoxCollider2D>();

        if (trigger.OverlapPoint(CharacterSelector.GetPlayerController().transform.position))
        {
            cameraCollider.SetActive(true);
        }
    }
}
