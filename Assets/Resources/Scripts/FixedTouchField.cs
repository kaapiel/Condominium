using UnityEngine;
using UnityEngine.EventSystems;

public class FixedTouchField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [HideInInspector]
    public Vector2 TouchDist;
    [HideInInspector]
    public Vector2 PointerOld;
    [HideInInspector]
    protected int PointerId;
    [HideInInspector]
    public bool Pressed;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Pressed)
        {
            if (PointerId >= 0 && PointerId < Input.touches.Length)
            {
                TouchDist = Input.touches[PointerId].position - PointerOld;
                PointerOld = Input.touches[PointerId].position;
            }
            else
            {
                if (Input.touchCount > 1)
                {
                    TouchDist = new Vector2(Input.GetTouch(1).position.x, Input.GetTouch(1).position.y) - PointerOld;
                    PointerOld = Input.GetTouch(1).position;
                } else
                {
                    TouchDist = new Vector2(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y) - PointerOld;
                    PointerOld = Input.GetTouch(0).position;
                }
                
            }
        }
        else
        {
            TouchDist = new Vector2();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
        PointerId = eventData.pointerId;
        PointerOld = eventData.position;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        
    }
}