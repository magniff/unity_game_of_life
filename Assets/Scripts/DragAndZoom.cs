using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndZoom : MonoBehaviour
{
    public float zoomMax = 200;
    public float zoomMin = 30;
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

            zoom((currentMagnitude - prevMagnitude) * 0.03f);
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 direction = this.touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Camera.main.transform.position += direction;
            }
        }

        void zoom(float increment)
        {
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomMin, zoomMax);
        }
    }
}
