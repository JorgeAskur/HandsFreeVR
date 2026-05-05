using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssemblyManager : MonoBehaviour
{
    private TaskManager taskManage;
    private MasterManager masterManage;
    private AudioSource speaker;

    private void Start()
    {
        taskManage = FindObjectOfType<TaskManager>();
        masterManage = FindObjectOfType<MasterManager>();
        speaker = GameObject.Find("Speaker").GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Primitive") {
            other.gameObject.transform.SetParent(this.transform);

            //search if in target surface are any target objs
            GameObject parent = GameObject.Find("Target Surface").gameObject;
            int objCount = parent.transform.childCount;
            
            for (int i = 0; i < objCount; i++)
            {
                GameObject obj = parent.transform.GetChild(i).gameObject;

                if (obj.gameObject.tag == "Primitive" && (obj.gameObject.name.ToLower().Contains(taskManage.type) || taskManage.type == "") && (taskManage.color == "" || (masterManage.colors[taskManage.color].color == obj.GetComponent<MeshRenderer>().material.color)))
                {
                    if (taskManage.type == "sphere" && obj.gameObject.name.ToLower().Contains("hemisphere"))
                    {
                        continue;
                    }

                    return;
                }
                
            }

            // check that all objs in user surface are correct
            parent = this.gameObject;
            objCount = parent.transform.childCount;

            for (int i = 0; i < objCount; i++)
            {
                GameObject obj = parent.transform.GetChild(i).gameObject;
                if (obj.gameObject.tag == "Primitive")
                {
                    if ((obj.gameObject.name.ToLower().Contains(taskManage.type) || taskManage.type == "") && (taskManage.color == "" || masterManage.colors[taskManage.color].color == obj.GetComponent<MeshRenderer>().material.color))
                    {
                        if (taskManage.type == "sphere" && obj.gameObject.name.ToLower().Contains("hemisphere"))
                        {
                            return;
                        }
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
                
            }

            Invoke("FinishTask", 1.0f);
        }
    }
    private void FinishTask()
    {
        taskManage.taskDone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Primitive")
        {
            other.gameObject.transform.SetParent(GameObject.Find("Target Surface").gameObject.transform);

            //search if in target surface are any target objs
            GameObject parent = GameObject.Find("Target Surface").gameObject;
            int objCount = parent.transform.childCount;

            for (int i = 0; i < objCount; i++)
            {
                GameObject obj = parent.transform.GetChild(i).gameObject;

                if (obj.gameObject.tag == "Primitive" && (obj.gameObject.name.ToLower().Contains(taskManage.type) || taskManage.type == "") && (taskManage.color == "" || (masterManage.colors[taskManage.color].color == obj.GetComponent<MeshRenderer>().material.color)))
                {
                    if (taskManage.type == "sphere" && obj.gameObject.name.ToLower().Contains("hemisphere"))
                    {
                        continue;
                    }
                    return;
                }

            }

            // check that all objs in user surface are correct
            parent = this.gameObject;
            objCount = parent.transform.childCount;

            for (int i = 0; i < objCount; i++)
            {
                GameObject obj = parent.transform.GetChild(i).gameObject;
                if (obj.gameObject.tag == "Primitive")
                {
                    if ((obj.gameObject.name.ToLower().Contains(taskManage.type) || taskManage.type == "") && (masterManage.colors[taskManage.color].color == obj.GetComponent<MeshRenderer>().material.color || taskManage.color == ""))
                    {
                        if (taskManage.type == "sphere" && obj.gameObject.name.ToLower().Contains("hemisphere"))
                        {
                            return;
                        }
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }

            }
            Invoke("FinishTask", 1.0f);
        }
    }
}
