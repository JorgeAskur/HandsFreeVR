using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using System;

public class MasterManager : MonoBehaviour
{
    [SerializeField]
    private MenuManager menuManage;
    //Stores the starting position of the assembly
    private Vector3 assemblyPos;
    //Stores the inspection position of the assembly
    private Vector3 assemblyNewPos;
    //Keeps track of the players transform
    private Transform player;
    //Empty GameObject that parents the target and player assembly
    public GameObject assembly;
    //Keeps track of the current mode, whether inspection, interaction or selection
    private string mode = "interact";

    public TTSSpeaker speaker;
    public Material red;
    public Material green;
    public Material blue;
    public Material yellow;
    public Material magenta;
    public Material cyan;
    public Material black;
    public Material white;
    public Material brown;

    public GameObject cube;
    public GameObject pyramid;
    public GameObject cylinder;
    public GameObject hemisphere;
    public GameObject sphere;

    public TCPClientDemo tcpClient;
    private GroupManager groupManage;
    [SerializeField]
    private TaskManager taskManage;
    private Vector3 topPos;
    
    public Dictionary<string, Func<GameObject, string, bool>> commands = new Dictionary<string, Func<GameObject, string, bool>>();
    public Dictionary<string, GameObject> blocks = new Dictionary<string, GameObject>();
    public Dictionary<string, Material> colors = new Dictionary<string, Material>();
    public Dictionary<string, Vector3> sizes = new Dictionary<string, Vector3>();

    private void Start()
    {   //Get the players transform
        player = FindObjectOfType<OVRPlayerController>().gameObject.transform;
        //Get the assembly's starting position
        assemblyPos = assembly.transform.position;
        groupManage = FindObjectOfType<GroupManager>();

        commands.Add("select", SelectItems);
        commands.Add("align", AlignItems);

        blocks.Add("cube", cube);
        blocks.Add("sphere", sphere);
        blocks.Add("pyramid", pyramid);
        blocks.Add("hemisphere", hemisphere);
        blocks.Add("cylinder", cylinder);

        colors.Add("white", white);
        colors.Add("black", black);
        colors.Add("red", red);
        colors.Add("blue", blue);
        colors.Add("green", green);
        colors.Add("cyan", cyan);
        colors.Add("magenta", magenta);
        colors.Add("brown", brown);
        colors.Add("yellow", yellow);

        sizes.Add("small", new Vector3(0.05f, 0.05f, 0.05f));
        sizes.Add("big", new Vector3(0.25f, 0.25f, 0.25f));
    }

    public bool userIntention(string intent)
    {
        List<GameObject> objectsSelected = groupManage.selectedObjs;
        intent = intent.ToLower();
        intent = intent.Trim(' ');
        
        string[] data = intent.Split(";");

        if (data.Length == 0) { data[0] = intent; }
        foreach (String s in data)
        {
            string key = "";
            GameObject parameter = null;
            string parameterS = "";

            UnityEngine.Debug.LogError("Recieved " + data[0]);

            // if percentage scaling
            if (s.Contains('%'))
            {
                parameterS = s.Replace("scale", "");
                parameterS = parameterS.Replace("(", "").Replace(")", "");
                key = "scale";
            }
            else
            {
                //string handling for parameter S in coloring, regular scaling, spawning, perspective, selection & alignment
                foreach (string k in commands.Keys)
                {
                    // if key in dict then set key and parameters
                    if (s.ToLower().Contains(k)) { key = k; parameterS = s.Replace(k, "").Replace("(", "").Replace(")", ""); break; };
                }
                parameterS = parameterS.ToLower();
                
                //check for zoomin, zoomout, language modes
                switch (s)
                {
                    case "language(spanish)":
                        menuManage.changeLanguage(menuManage.spanish);
                        continue;
                    case "language(english)":
                        menuManage.changeLanguage(menuManage.english);
                        continue;
                    case "box(add)":
                        moveToBox();
                        continue;
                    case "box(remove)":
                        removeFromBox();
                        continue;
                    case "reset()":
                        reset();
                        continue;
                    case "finish()":
                        if (taskManage.currentTask == "T0")
                        {
                            taskManage.StartTask();
                        }
                        else
                        {
                            if (taskManage.taskDone)
                            {
                                taskManage.FinishTask();
                            }
                        }
                        if (menuManage.changeTask) { menuManage.changeTask = !menuManage.changeTask; }
                        continue;
                }
            }
            UnityEngine.Debug.LogError("in string " + s + " found key " + key + " parameterS = " + parameterS);

            //check if key exists
            if (key == "" || parameterS == "") { Debug.Log("Error: key " + key + "parameterS " + parameterS); return false; }

            // if not for every selected item
            if (key == "instantiate" || key == "perspective" || key == "select" || key == "alignment")
            {
                if (key == "perspective") { parameter = assembly; } //if inspecting
                else if (key == "instantiate"){ if (blocks.ContainsKey(parameterS)) { parameter = blocks[parameterS]; } } //if spawning
                else if (key == "select") //if selecting
                {
                    string[] parameters = parameterS.Split(",");

                    foreach (string p in parameters)
                    {
                        if (blocks.ContainsKey(p)) { parameter = blocks[p]; parameterS.Replace(p, ""); }
                    }
                } 
                else if (key == "alignment") { parameter = player.gameObject; } //if aligning
                if (!commands[key](parameter, parameterS)) { UnityEngine.Debug.LogError("Error executing " + key + "(" + parameter.name + "," + parameterS + ")"); continue; }
            }
            else
            {
                // if for every selected item 
                foreach (GameObject i in objectsSelected)
                {
                    if (!commands[key](i, parameterS)) { UnityEngine.Debug.LogError("Error executing " + key + "(" + i.name + "," + parameterS + ")"); }
                }
            }
        }
        return true;
    }

    bool SelectItems(GameObject type, string conditions)
    {
        GameObject[] unsortedItems;
        if (taskManage.currentTask == "T2")
            unsortedItems = GameObject.FindGameObjectsWithTag("Primitive");
        else if (taskManage.currentTask == "T3")
            unsortedItems = GameObject.FindGameObjectsWithTag("Unaligned");
        else
        {
            return false;
        }

        if (groupManage.selectedObjs.Count > 0)
        {
            foreach (GameObject a in groupManage.selectedObjs)
            {
                a.GetComponent<Rigidbody>().useGravity = true;
            }
        }
        
        groupManage.selectedObjs.Clear();

        Color targetColor = new Color(0,0,0,0);
        string targetBlock = "";

        bool checkCol = false;
        bool checkType = false;

        //select by string
        if (type != null)
        {
            targetBlock = type.name;
            checkType = true;
        }

        string[] parameters = conditions.Split(",");
        UnityEngine.Debug.LogWarning(parameters);

        foreach (string p in parameters)
        {
            if (colors.ContainsKey(p))
            {
                //set the color to be found
                targetColor = colors[p].color;
                checkCol = true;
            }
        }

        bool rightCol = true;
        bool rightType = true;

        try {
            groupManage.floating = true;
            foreach (GameObject i in unsortedItems)
            {
                if (checkCol)
                {
                    Material m = i.GetComponent<MeshRenderer>().material;
                    if (m.color == targetColor) { rightCol = true; }
                    else { rightCol = false; }
                }
                if (checkType)
                {
                    if (i.name.Contains(targetBlock)) { 

                        if (targetBlock == "sphere" && i.name.Contains("hemisphere"))
                        {
                            rightType = false;
                        }
                        else
                        {
                            rightType = true;
                        }
                    }
                    else { rightType = false; }
                }

                if (rightType && rightCol)
                {
                    groupManage.addToSelection(i);
                }
           
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ERROR IN SELECTION: " + e);
            tcpClient.WriteToTXT("Crashed in selection\n");
        }

        if (groupManage.selectedObjs.Count <= 0) { return false; }
        foreach (GameObject obj in groupManage.selectedObjs)
        {
            Vector3 newPos = obj.transform.position;
            if (taskManage.currentTask == "T2") { newPos.y = taskManage.secondTaskStruct.transform.Find("Target Surface").gameObject.transform.position.y + obj.transform.localScale.y + 1.5f;  }
            else
            {
                newPos.y = taskManage.thirdTaskStruct.transform.Find("Target Surface").gameObject.transform.position.y + obj.transform.localScale.y + 1.5f;
            }
           
            obj.transform.position = newPos;
            obj.GetComponent<Rigidbody>().useGravity = false;
            obj.GetComponent<Rigidbody>().freezeRotation = true;
        }

        return true;
    }   

    void moveToBox()
    {
        if (taskManage.currentTask == "T2")
        {
            groupManage.floating = false;
            Vector3 newPos = taskManage.secondTaskStruct.transform.Find("User Surface").gameObject.transform.position;
            foreach (GameObject obj in groupManage.selectedObjs)
            {
                if (obj.name.ToLower().Contains("cylinder"))
                {
                    obj.transform.eulerAngles = new Vector3(0f, 0f, 90f);
                }

                newPos.y += 0.5f;
                obj.transform.position = newPos;

                obj.GetComponent<Rigidbody>().useGravity = true;
                obj.GetComponent<Rigidbody>().isKinematic = false;
                obj.GetComponent<Rigidbody>().freezeRotation = false;
            }
        }
    }

    void removeFromBox()
    {
        if (taskManage.currentTask == "T2")
        {
            if (taskManage.secondTaskStruct.transform.Find("User Surface").gameObject.transform.childCount == 4) { //speaker.Speak("No objects in box");
                                                                                                                   return; }
            Vector3 newPos = taskManage.secondTaskStruct.transform.Find("Target Surface").gameObject.transform.position;
            newPos.y += 1.5f;
            for (int i = 0; i < taskManage.secondTaskStruct.transform.Find("User Surface").gameObject.transform.childCount; i++)
            {
                GameObject child = taskManage.secondTaskStruct.transform.Find("User Surface").gameObject.transform.GetChild(i).gameObject;
                if (child.tag == "Unsorted") { continue; }
                newPos.y += 0.5f;
                child.transform.position = newPos;
                child.GetComponent<Rigidbody>().useGravity = true;
                child.GetComponent<Rigidbody>().isKinematic = false;
            }
            groupManage.floating = false;
        }
    }

    void reset()
    {
        if (taskManage.currentTask == "T2")
        {
            Destroy(taskManage.secondTaskStruct);
            taskManage.changeToT2();
        }
        else if (taskManage.currentTask == "T3")
        {
            GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

            foreach (GameObject obj in markers)
            {
                Destroy(obj);
            }
            Destroy(taskManage.thirdTaskStruct);
            taskManage.changeToT3();
        }
    }
    bool AlignItems(GameObject around, string shape)
    {
        if (taskManage.currentTask == "T2") { return false; }

        List<GameObject> selectedItems = groupManage.selectedObjs;
        Vector3 newPos = taskManage.thirdTaskStruct.transform.Find("User Surface").gameObject.transform.position;
        float offset = 0f;
        groupManage.floating = false;
        newPos.z -= 0.5f;
        switch (shape)
        {
            case "row":
                offset = 0f;
                foreach (GameObject i in selectedItems)
                {
                    newPos.y = i.transform.localScale.y;
                    
                    i.transform.position = newPos;
                    i.transform.rotation = Quaternion.identity;

                    if (i.name.Contains("pyramid") || i.name.Contains("hemisphere")) { i.transform.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f); }

                    newPos.x += i.transform.localScale.x * 1.5f;
                    offset += i.transform.localScale.x * 1.5f;
                }
                offset /= 2.0f;
                foreach (GameObject i in selectedItems)
                {
                    newPos.y = i.transform.localScale.y;
                    newPos = i.transform.position;
                    newPos.x -= offset;
                    i.transform.position = newPos;
                    i.transform.rotation = Quaternion.identity;
                    if (i.name.Contains("pyramid") || i.name.Contains("hemisphere")) { i.transform.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f); }
                    i.GetComponent<Rigidbody>().useGravity = true;
                    i.GetComponent<Rigidbody>().isKinematic = false;
                }
                break;
            case "circle":
                if (selectedItems.Count == 0) { return false; }
                float angle = 360.0f / (float) selectedItems.Count;
                float theta = 0.0f;
                foreach (GameObject i in selectedItems)
                {
                    Vector3 posC = new Vector3(0f,0f,0f);
                    posC.x = newPos.x + (0.5f * Mathf.Sin(theta * Mathf.Deg2Rad));
                    posC.y = i.transform.localScale.y;
                    posC.z = newPos.z + (0.5f * Mathf.Cos(theta * Mathf.Deg2Rad));

                    i.transform.position = posC;
                    i.transform.rotation = Quaternion.identity;
                    if (i.name.Contains("pyramid") || i.name.Contains("hemisphere")) { i.transform.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f); }
                    i.GetComponent<Rigidbody>().useGravity = true;
                    i.GetComponent<Rigidbody>().isKinematic = false;
                    theta += angle;
                }

                break;
            case "matrix":
                newPos.z -= 0.5f;
                int n = (int)Math.Ceiling(Math.Sqrt(selectedItems.Count));
                if (n == 1){
                    n++;
                }
                int index = 0;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (index > selectedItems.Count - 1) { return true; }
                        Vector3 nPos = new Vector3();
                        nPos.x = newPos.x + (0.5f * j);
                        nPos.y = selectedItems[index].transform.localScale.y;
                        nPos.z = newPos.z + (0.5f * i);         
                        selectedItems[index].transform.position = nPos;
                        selectedItems[index].transform.rotation = Quaternion.identity;
                        if (selectedItems[index].name.Contains("pyramid") || selectedItems[index].name.Contains("hemisphere")) { selectedItems[index].transform.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f); }
                        selectedItems[index].GetComponent<Rigidbody>().useGravity = true;
                        selectedItems[index].GetComponent<Rigidbody>().isKinematic = false;
                        index++;
                    }
                }
                break;
            default:
                return false;
        }
        return true;
    }
}
