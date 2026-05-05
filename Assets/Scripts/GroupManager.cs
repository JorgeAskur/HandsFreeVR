using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using Oculus.Interaction;

public class GroupManager : MonoBehaviour
{
    private MasterManager masterManager;
    [SerializeField]
    private TaskManager taskManager;

    [SerializeField]
    private TTSSpeaker speaker;

    public List<GameObject> selectedObjs;
    public bool floating = true;
    private void Start()    
    {
        masterManager = GameObject.Find("EventSystem").GetComponent<MasterManager>();
    }
    public void addToSelection(GameObject block)
    {
        if (taskManager.currentTask == "T1")
        {
            foreach (GameObject i in selectedObjs)
            {
                Destroy(i);
            }
            selectedObjs.Clear();
        }
        block.GetComponent<RayInteractable>().enabled = false;
        selectedObjs.Add(block);
    }

    public void removeSelection(GameObject block)
    {
        if (selectedObjs.Count == 0) { return; }
        selectedObjs.Remove(block);
    }
    public void clearSelection()
    {
        if (selectedObjs.Count == 0) { return; }
        selectedObjs.Clear();
    }
    public void duplicateBlocks()
    {
        if (taskManager.currentTask != "T4") { speaker.Speak("Duplication not currently available");  return; } 
        foreach (GameObject item in selectedObjs)
        {
            Vector3 offset = new Vector3(0.2f, 0.2f, 0.2f);
            Vector3 newPos = item.transform.position + offset;
            GameObject newObj = Instantiate(item, newPos, Quaternion.identity);
            newObj.GetComponent<StaticWireframeRenderer>().enabled = false;
        }
    }
    private void Update()
    {
        if (taskManager.currentTask != "T1" && floating)
        {
            foreach (GameObject i in selectedObjs)
            {
                //calculate what the new Y position will be
                float newY = (Mathf.Sin(Time.time * 1.5f) * 0.0005f) + i.transform.position.y;
                //set the object's Y to the new calculated Y
                i.transform.position = new Vector3(i.transform.position.x, newY, i.transform.position.z);
            }
        } 
    }
}
