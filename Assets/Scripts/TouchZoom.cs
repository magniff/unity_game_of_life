using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchZoom : MonoBehaviour
{
    public float zoomMax = 200;
    public float zoomMin = 30;
    public float zoomSensitivity = 0.02f;
    Vector3 touchStart;

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);
            var touch0PrevPosition = touch0.position - touch0.deltaPosition;
            var touch1PrevPosition = touch1.position - touch1.deltaPosition;

            var prevMagnitude = (touch0PrevPosition - touch1PrevPosition).magnitude;
            var currentMagnitude = (touch0.position - touch1.position).magnitude;

            Camera.main.orthographicSize = Mathf.Clamp(
                value: (
                    Camera.main.orthographicSize -
                    (currentMagnitude - prevMagnitude) * this.zoomSensitivity
                ),
                min: zoomMin,
                max: zoomMax
            );
        }
    }
}
