using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonTest : MonoBehaviour
{
    public void Enter()
    {
        
    }

    private void OnGUI()
    {
        Debug.Log(Event.current.mousePosition);
    }
}
