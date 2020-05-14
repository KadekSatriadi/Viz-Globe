﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticRotation : MonoBehaviour
{
    public float speed = 0.1f;
    public bool isActive = true;
    public bool isInteractable = false;
    public float interactionSpeed = 0.1f;
    public Transform target;
    [Header("Sync Rotation")]
    public bool syncRotationAllObject = true; 

    private bool isPlaying = true;
    private Quaternion prevRotation;
    private bool isInteracting = false;
    private Vector3 prevPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPlaying = !isPlaying;
        }

        if (isInteractable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == target)
                    {
                        isInteracting = true;
                        prevRotation = hit.transform.rotation;
                        prevPosition = Input.mousePosition;
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                isInteracting = false;
            }
        }

        if (isInteracting)
        {
            Vector3 currentPos = Input.mousePosition;
            Vector3 dir = currentPos - prevPosition;
            if (syncRotationAllObject)
            {
                foreach (AutomaticRotation r in FindObjectsOfType<AutomaticRotation>())
                {
                    r.target.Rotate(new Vector3(dir.y * interactionSpeed, -dir.x * interactionSpeed, 0), Space.World);
                }
            }
            else
            {
                target.Rotate(Vector3.up, -dir.x * interactionSpeed);
            }
            prevRotation = target.rotation;
            prevPosition = currentPos;
        }

        Quaternion rot = Quaternion.identity;
        if (isPlaying && !isInteracting && isActive)
        {
            rot = Quaternion.Slerp(transform.rotation, 
                Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + speed, transform.rotation.eulerAngles.z), 
                Time.time * speed);
                target.rotation = rot;
        }

        }
}
