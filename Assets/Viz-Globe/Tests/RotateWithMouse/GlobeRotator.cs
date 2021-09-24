using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobeRotator : MonoBehaviour
{
    public Globe globe;
    public AnimationCurve transferFunction;
    [Range(0.1f, 1f)]
    public float maxSpeed = 0.5f;
    [Range(0.1f, 1f)]
    public float minSpeed = 0.05f;

    [Header("Unity Events")]
    public UnityEvent OnStartRotation;
    public UnityEvent OnFinishRotation;
    public UnityEvent OnRotation;

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
            OnStartRotation.Invoke();
        }

        if (Input.GetMouseButtonUp(0))
        {
            interactionStatus = InteractioStatus.Null;
            OnFinishRotation.Invoke();
        }

        if (interactionStatus == InteractioStatus.Drag)
        {

            Vector3 velocity = Input.mousePosition - lastCursorPosition;

            float x = -TransferFunction(velocity.x);
            float y = TransferFunction(velocity.y);

            globe.RollLimit(Camera.main.transform.position, y);
            globe.Yaw(x);

            OnRotation.Invoke();
        }

        lastCursorPosition = Input.mousePosition;
    }

    float TransferFunction(float speed)
    {
        float val = transferFunction.Evaluate(Mathf.Lerp(minSpeed, maxSpeed, (Mathf.Abs(speed) / maxSpeed)));
        return speed * val;


    }
}
