using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventObject))]
public class Door : MonoBehaviour
{
    public Vector2 destination;
    public EventCondition condition;

    private void OnMouseUpAsButton()
    {
        // 先執行該有的事件
        GetComponent<EventObject>().RunEvent();
        // 結束後執行傳送
        if (GetComponent<EventObject>().CheckEventContition(condition))
        {
            GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
            gameObject.transform.position = destination;
            Camera.main.transform.position = new Vector3(destination.x, destination.y, -10);
        }
    }
}
