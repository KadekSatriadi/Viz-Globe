using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeRotator : MonoBehaviour
{
    public Globe globe;
    public AnimationCurve transferFunction;
    public float maxSpeed = 5f;
    public float minSpeed = 2f;
    [Range(0,0.9f)]
    public float pichConstraint;
    private enum InteractioStatus
    {
        Drag, Null
    }

    private InteractioStatus interactionStatus = InteractioStatus.Null;
    private Vector3 lastCursorPosition;
    private Quaternion lastRotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            lastRotation = globe.transform.rotation;
            lastCursorPosition = Input.mousePosition;
            interactionStatus = InteractioStatus.Drag;
        }

        if (Input.GetMouseButtonUp(0))
        {
            interactionStatus = InteractioStatus.Null;
        }

        if (interactionStatus == InteractioStatus.Drag)
        {

            Vector3 velocity = Input.mousePosition - lastCursorPosition;

            float x = -TransferFunction(velocity.x);
            float y = TransferFunction(velocity.y);

            globe.RollLimit(Camera.main.transform.position, y);
            globe.Yaw(x);


            float t = pichConstraint;
            //if (globe.transform.rotation.x > t || globe.transform.rotation.x < -t)
            //{
            //    globe.transform.rotation = lastRotation;
            //}
            //else
            //{
            //    lastRotation = globe.transform.rotation;
            //}
        }

        lastCursorPosition = Input.mousePosition;
    }

    float TransferFunction(float speed)
    {
        float val = transferFunction.Evaluate(Mathf.Lerp(minSpeed, maxSpeed, (Mathf.Abs(speed) / maxSpeed)));
        return speed * val;


    }
}
