using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    public float rotationSpeed = 0.4f;
    public float zoomSpeed = 0.5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    private Vector3 lastTouchPosition;
    private bool isRotating = false;

    void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Check if touch is over UI
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    lastTouchPosition = touch.position;
                    isRotating = true;
                    break;

                case TouchPhase.Moved:
                    if (isRotating)
                    {
                        Vector2 delta = touch.position - (Vector2)lastTouchPosition;
                        RotateCamera(delta);
                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                    isRotating = false;
                    break;
            }
        }
        else if (Input.touchCount == 2)
        {
            // Pinch to zoom
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (touch1PrevPos - touch2PrevPos).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            ZoomCamera(difference * 0.01f);
        }
    }

    private void RotateCamera(Vector2 delta)
    {
        transform.RotateAround(Vector3.zero, Vector3.up, delta.x * rotationSpeed);
        transform.RotateAround(Vector3.zero, transform.right, -delta.y * rotationSpeed);
    }

    private void ZoomCamera(float increment)
    {
        Vector3 direction = transform.position.normalized;
        float currentDistance = transform.position.magnitude;
        float newDistance = Mathf.Clamp(currentDistance - increment, minZoom, maxZoom);

        transform.position = direction * newDistance;
    }
}