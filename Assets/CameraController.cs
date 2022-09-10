using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera _camera;

    public float maxZoom = 0.0008733823f;
    public float minZoom = 5;

    public float dragScale = 1;
    public Vector3 lastPosition = Vector2.zero;
    public Vector3 targetPos = Vector2.zero;
    public bool isDragging = false;
    
    
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        targetPos = _camera.ScreenToViewportPoint(Input.mousePosition);
        Vector3 targetMove = targetPos - lastPosition;
        targetMove.x *= -dragScale * _camera.orthographicSize;
        targetMove.y *= -dragScale * _camera.orthographicSize;

        isDragging = Input.GetMouseButton(1) && !targetPos.Equals(lastPosition);

        if (isDragging)
        {
            transform.position += targetMove;
        }

        lastPosition = targetPos;
        
        var a = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(a,0)) return;
        var abs = Mathf.Abs(a).Remap(0.5f,10,1.1f,2);
        a = Mathf.Sign(a) * abs;
        if (a > 0)
        {
            _camera.orthographicSize /= a;
        }
        else if (a < 0)
        {
            _camera.orthographicSize *= -a;
        }

        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, maxZoom, minZoom);
    }
}
