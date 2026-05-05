using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSystem : MonoBehaviour
{
    private GroupManager groups;
    private GameObject eventSystem;

    private void Start()
    {
        eventSystem = GameObject.Find("EventSystem");
        groups = eventSystem.GetComponent<GroupManager>();
    }

}
