using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeManager : MonoBehaviour
{
    private TaskManager taskManage;

    private void Start()
    {
        taskManage = FindObjectOfType<TaskManager>();

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Collider")
        {
            taskManage.inGameZone = true;
        }
            
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.name == "Collider")
        {
            taskManage.inGameZone = false;
        }
    }
}
