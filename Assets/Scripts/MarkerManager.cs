using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerManager : MonoBehaviour
{
    private TaskManager taskManage;
    private MasterManager masterManage;
    private AudioSource speaker;
    private List<GameObject> colliding;
    private List<GameObject> collidingAll;

    private void Start()
    {
        colliding = new List<GameObject>();
        collidingAll = new List<GameObject>();
        taskManage = FindObjectOfType<TaskManager>();
        masterManage = FindObjectOfType<MasterManager>();
        speaker = GameObject.Find("Speaker").GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Unaligned")
        {
            collidingAll.Add(other.gameObject);
        }
        if (other.gameObject.tag == "Unaligned" && (other.gameObject.name.ToLower().Contains(taskManage.type) || taskManage.type == "")
            && (taskManage.color == "" || masterManage.colors[taskManage.color].color == other.GetComponent<MeshRenderer>().material.color))
        {
            if (taskManage.type == "sphere" && other.gameObject.name.ToLower().Contains("hemisphere"))
            {
                return;
            }
            colliding.Add(other.gameObject);
            if (colliding.Count != collidingAll.Count) { return; }
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;

            if (colliding.Count == 1)
            {
                speaker.Play();
            }

            //check if all done
            GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

            foreach (GameObject obj in markers)
            {
                if (obj.GetComponent<MeshRenderer>().enabled)
                {
                    return;
                }
            }
            Invoke("FinishTask", 1.0f);
        }
    }

    private void FinishTask()
    {
        taskManage.taskDone = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collidingAll.Contains(collision.gameObject)) { collidingAll.Remove(collision.gameObject); }

        if (collision.gameObject.tag == "Unaligned" && (collision.gameObject.name.ToLower().Contains(taskManage.type) || taskManage.type == "")
            && (taskManage.color == "" || (masterManage.colors[taskManage.color].color == collision.GetComponent<MeshRenderer>().material.color)))
         {
            if (taskManage.type == "sphere" && collision.gameObject.name.ToLower().Contains("hemisphere"))
            {
                return;
            }
            colliding.Remove(collision.gameObject);
            if (colliding.Count == 0)
            {
                this.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        //check if all done
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

        foreach (GameObject obj in markers)
        {
            if (obj.GetComponent<MeshRenderer>().enabled)
            {
                return;
            }
        }
        Invoke("FinishTask", 1.0f);
    }
}
