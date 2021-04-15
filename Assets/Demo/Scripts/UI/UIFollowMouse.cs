using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowMouse : MonoBehaviour
{
    private const int CURSOR_WIDTH = 13;
    private const int CURSOR_HEIGHT = 20;

    [SerializeField] [Range(0, 50)] private float distanceToMouse = 10f;

    private Vector2 screenCenter = new Vector2(Screen.width/2, Screen.height/2);
    private RectTransform rectTransform;

    [HideInInspector] public Vector3 targetPos = Vector3.zero;

    private void Awake() 
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() 
    {
        UpdatePosition();
    }

    private void Update() 
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 pivot = Vector2.zero;
        pivot = Vector2.zero;
        pivot += (targetPos.x >= screenCenter.x) ? new Vector2(1, 0) : new Vector2(0, 0);
        pivot += (targetPos.y >= screenCenter.y) ? new Vector2(0, 1) : new Vector2(0, 0);
        rectTransform.pivot = pivot;

        Vector2 posDelta = Vector2.zero;
        posDelta += (targetPos.x >= screenCenter.x) ? new Vector2(-1, 0) : new Vector2(1, 0);
        posDelta += (targetPos.y >= screenCenter.y) ? new Vector2(0, -1) : new Vector2(0, 1);

        transform.position = Input.mousePosition + new Vector3(posDelta.x, posDelta.y) * distanceToMouse;

        // adjustments for the size of mouse cursor
        transform.position += (targetPos.x >= screenCenter.x) ? new Vector3(0, 0) : new Vector3(CURSOR_WIDTH, 0);
        transform.position += (targetPos.y >= screenCenter.y) ? new Vector3(0, 0) : new Vector3(0, -CURSOR_HEIGHT);
    }
}
