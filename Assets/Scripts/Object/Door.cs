using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventObject))]
public class Door : MonoBehaviour
{
    public Vector2 characterDestination;
    public Vector2 cameraDestination;
    public EventCondition condition;

    private void OnMouseUpAsButton()
    {
        // 先執行該有的事件
        GetComponent<EventObject>().RunEvent();
        // 結束後執行傳送
        if (GetComponent<EventObject>().CheckEventContition(condition))
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = characterDestination;
            Camera.main.transform.position = new Vector3(cameraDestination.x, cameraDestination.y, -10);
        }
    }
}
