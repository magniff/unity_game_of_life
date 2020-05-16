using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDrag : MonoBehaviour
{
    private Vector3 touchStart;
    private bool canDrag = false;

    void Update()
    {
        if (Input.touchCount == 1 & this.canDrag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 direction = (
                    this.touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition)
                );
                Camera.main.transform.position += direction;
            }
        }
        else if (Input.touchCount >= 2)
        {
            this.canDrag = false;
        }
        else if (Input.touchCount == 0)
        {
            this.canDrag = true;
        }
    }
}

