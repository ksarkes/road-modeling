using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    Vector3 startPos;
    bool clicked;
    [SerializeField]
    float scrollSensitivity = 0.05f;
    void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            if (!clicked)
            {
                clicked = true;
                startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
            clicked = false;

        if (clicked)
        {

            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var newPosDelta = mousePos - startPos;
            var oldPos = Camera.main.transform.position;
            Camera.main.transform.position = oldPos - newPosDelta/100;
            startPos = Camera.main.transform.position;
        }

        var delta = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize += delta;
    }
}
