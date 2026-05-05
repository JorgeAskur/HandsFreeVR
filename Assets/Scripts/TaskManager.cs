using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.TTS.Utilities;
using TMPro;
using UnityEngine;
using System.Diagnostics;
using System;

public class TaskManager : MonoBehaviour
{
    public string currentTask = "T0";
    public int variant;
    private int nTasks = 3;
    private int t = 1;
    private int r;
    public bool rec = false;
    [SerializeField]
    private GroupManager groupManager;
    [SerializeField]
    private MasterManager masterManager;
    [SerializeField]
    private MenuManager menuManage;

    [SerializeField]
    private TCPClientDemo tcpClient;

    private GameObject selectedObj;

    [SerializeField]
    private TTSSpeaker speaker;

    private GameObject player;

    public GameObject homeZone;

    public bool inGameZone = false;
    public bool taskDone = false;

    // T1
    public List<GameObject> targetObjT1;
    private string[] targetTypeT1 = { "cube", "cylinder", "sphere", "cube", "cylinder", "sphere", "hemisphere", "cube" };
    private string[] targetColorT1 = { "yellow", "blue", "yellow", "red", "green", "brown", "blue", "black" };

    [SerializeField]
    private GameObject textMenu;

    private string resultsT1;

    // T2
    public GameObject secondTask;
    public GameObject secondTaskStruct;

    //                                   12,       12,      12,        12,        12,           12,        12,       12, 12         
    private string[] targetColorT2 = { "", "yellow", "cyan", "brown",    "cyan",       "blue",        "",    "red", "" };
    private bool[] checkColorT2 = { false,     true,   true,    true,      true,         true,     false,    true, false };

    private string[] targetTypeT2 = {"pyramid", "sphere",    "", "sphere",    "", "hemisphere", "pyramid",   "cube", "cylinder" };
    private bool[] checkTypeT2 = {        true,     true, false,     true, false,         true,      true,     true,  true };

    private string resultsT2;

    // T3
    private string resultsT3;
    public GameObject thirdTask;
    public GameObject thirdTaskStruct;
    //                                12,        12,      12,       12,         12,       12,       12,    12,   12
    private string[] colorT3 = {         "", "green",  "red",  "black",         "",  "black",  "brown", "blue",  ""};
    private bool[] checkColT3 = {     false,    true,   true,     true,      false,     true,     true,   true, false };
    private string[] typeT3 = {  "cylinder",      "", "cube",       "", "cylinder",   "cube",       "",     "",  "sphere"};
    private bool[] checkTypeT3 = {   true,     false,   true,    false,       true,     true,     false,  false, true };

    private string[] shapeT3 = {  "circle", "matrix",  "row", "matrix",      "row", "circle", "matrix",  "row",  "circle"};
    [SerializeField]
    private GameObject marker;

    public string type;
    public string color;

    //Logging
    private Stopwatch clock;
    TimeSpan[] times = new TimeSpan[3];
    private OVRCameraRig cam;
    Vector3 ovrcamPos;
    Vector3 ovrcamRot;

    List<Vector3> posList;
    List<Vector3> rotList;
    List<Vector3> dirList;
    List<Vector3> leftHandList;
    List<Vector3> rightHandList;

    Vector3 lastcamPos;
    Vector3 lastcamRot;
    Vector3 lastcamDir;
    Vector3 lastleftHand;
    Vector3 lastrightHand;

    private bool recordPose = false;
    int[] answerQ = { 5, 1, 5, 5, 1 };

    private void Start()
    {
        player = FindObjectOfType<OVRPlayerController>().gameObject;
        cam = FindObjectOfType<OVRCameraRig>();

        posList = new List<Vector3>();
        rotList = new List<Vector3>();
        dirList = new List<Vector3>();
        leftHandList = new List<Vector3>();
        rightHandList = new List<Vector3>();
        r = variant;
    }

    public void StartTask()
    {
        Vector3 newPos = player.transform.position; 
        newPos.y = 0.001f;
        homeZone = Instantiate(homeZone, newPos, Quaternion.identity);

        tcpClient.WriteToTXT("Started experiment, Experimental Condition\n");
        //tcpClient.WriteToTXT("Started experiment, Conventional Condition");
        clock = new Stopwatch();

        changeToT3();

        clock.Start();
        recordPose = true;

        lastcamPos = cam.centerEyeAnchor.position;
        lastcamRot = cam.centerEyeAnchor.rotation.eulerAngles;
        lastcamDir = cam.centerEyeAnchor.forward;
        lastleftHand = cam.leftHandAnchor.position;
        lastrightHand = cam.rightHandAnchor.position;

        InvokeRepeating("takeData",0f,1.0f);

        menuManage.experimentOn = true;

    }
    public void FinishTask()
    {
        switch (currentTask)
        {
            case "T1":
                if (groupManager.selectedObjs.Count == 0)
                {
                    resultsT1 = "Task 1 completed\n";
                    resultsT1 += "Target block -- Name: " + targetTypeT1[variant] + " Color: " + targetColorT1[variant] + " Size: " + targetObjT1[variant].transform.localScale + '\n';
                    resultsT1 += " Time elapsed: " + clock.Elapsed.ToString() + '\n';

                    tcpClient.WriteToTXT(resultsT1);

                    changeToT2();

                    clock.Reset();
                    clock.Start();

                    return;
                }

                selectedObj = groupManager.selectedObjs[0];

                resultsT1 = "Task 1 completed\n";
                resultsT1 += "Target block -- Name: " + targetTypeT1[variant] + " Color: " + targetColorT1[variant] + " Size: " + targetObjT1[variant].transform.localScale + '\n';

                Vector3 scale = selectedObj.transform.localScale;

                if (!selectedObj.name.ToLower().Contains(targetTypeT1[variant].ToLower()))
                {
                    resultsT1 += " wrong block";
                }
                else
                    resultsT1 += " correct block";

                if (selectedObj.GetComponent<MeshRenderer>().material.color != targetObjT1[variant].GetComponent<MeshRenderer>().material.color)
                {
                    resultsT1 += " wrong color";
                }
                else
                    resultsT1 += " correct color";

                Vector3 error = (scale - targetObjT1[variant].transform.localScale);

                resultsT1 += " Scale error X:" + error.x.ToString() + " Y:" + error.y.ToString() + " Z:" + error.z.ToString() + '\n';
                resultsT1 += "Time elapsed: " + clock.Elapsed.ToString() + '\n';

                tcpClient.WriteToTXT(resultsT1);

                times[0] = clock.Elapsed;

                changeToT2();

                clock.Reset();
                clock.Start();

                break;
            case "T2":
                resultsT2 = "Task 1 completed ";
                resultsT2 += "Iteration " + t.ToString() + "\n";

                int errorCount = 0;
                int correctCount = 0;

                int count = 0;

                bool rightCol = true, rightType = true;

                resultsT2 += "Target blocks -- Name: " + targetTypeT2[variant] + " Color: " + targetColorT2[variant] + '\n';

                //
                GameObject parent = GameObject.Find("Target Surface").gameObject;
                int objCount = parent.transform.childCount;

                for (int i = 0; i < objCount; i++)
                {
                    GameObject obj = parent.transform.GetChild(i).gameObject;

                    if (obj.gameObject.tag == "Primitive" && (obj.gameObject.name.ToLower().Contains(type) || type == "") && (color == "" || (masterManager.colors[color].color == obj.GetComponent<MeshRenderer>().material.color)))
                    {
                        if (type == "sphere" && obj.gameObject.name.ToLower().Contains("hemisphere"))
                        {
                            continue;
                        }
                        count++;
                    }

                } 
                //

                //Get Target Object
                for (int i = 0; i < secondTaskStruct.transform.Find("User Surface").gameObject.transform.childCount; i++)
                {
                    GameObject child = secondTaskStruct.transform.Find("User Surface").gameObject.transform.GetChild(i).gameObject;
                    if (child.tag == "Unsorted") { continue; }
                    // Check if color needs to be checked, if true, check if target has any objs remaining with wrong color
                    if (checkColorT2[variant])
                    {
                        if (child.GetComponent<MeshRenderer>().material.color == masterManager.colors[targetColorT2[variant]].color) { rightCol = true; }
                        else { rightCol = false; }
                    }

                    // Check if type needs to be checked, if true, check if target has any objs remaining with wrong type
                    if (checkTypeT2[variant])
                    {
                        if (child.name.Contains(targetTypeT2[variant])) { rightType = true; }
                        else { rightType = false; }

                    }

                    if (rightCol && rightType) { correctCount++; }
                    else { errorCount++; }
                }

                //resultsT2 += "You had " + errorCount.ToString() + " errors and " + correctCount.ToString() + " of " + numOfItems[variant].ToString() + " correct blocks." + '\n';
                resultsT2 += "Time elapsed: " + clock.Elapsed.ToString() + '\n';
                UnityEngine.Debug.LogError("Number " + count.ToString());

                times[1] = clock.Elapsed;
                tcpClient.WriteToTXT(resultsT2);

                if (t == nTasks)
                {
                    t = 1;
                    variant = r;
                    stopExperiment();
                }
                else
                {
                    changeIteration();
                    changeToT2();
                }

                clock.Reset();
                clock.Start();
                break;
            case "T3":
                resultsT3 = "Task 2 completed ";
                resultsT3 += "Iteration " + t.ToString() + "\n";
                groupManager.selectedObjs.Clear();

                resultsT3 += "Target blocks -- Name: " + typeT3[variant] + " Color: " + colorT3[variant] + " Shape: " + shapeT3[variant] + '\n';
                // foreach item, check if items have the desired attributes, if do add to selection
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Unaligned"))
                {
                    bool rightT = true;
                    bool rightC = true;

                    if (checkTypeT3[variant])
                    {
                        if (obj.name.Contains(typeT3[variant]))
                        {
                            rightT = true;
                        }
                        else
                        {
                            rightT = false;
                        }
                    }

                    if (checkColT3[variant])
                    {
                        if (masterManager.colors[colorT3[variant]].color == obj.GetComponent<MeshRenderer>().material.color)
                        {
                            rightC = true;
                        }
                        else
                        {
                            rightC = false;
                        }
                    }

                    if (rightC && rightT)
                    {
                        groupManager.addToSelection(obj);
                    }
                }

                // finish everything                 
                resultsT3 += "Time elapsed: " + clock.Elapsed.ToString() + '\n';

                tcpClient.WriteToTXT(resultsT3);

                times[2] = clock.Elapsed;

                //speaker.Speak(output);

                if (t == nTasks)
                {
                    t = 1;
                    variant = r;
                    Destroy(thirdTaskStruct);
                    changeToT2();
                }
                else
                {
                    changeIteration();
                    changeToT3();
                }
                clock.Reset();
                clock.Start();
                break;
            default:
                UnityEngine.Debug.Log("error NO REMAINING TASKS");
                break;
        }
    }

    public void changeToT2()
    {
        taskDone = false;
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject obj in markers)
        {
            Destroy(obj);
        }
        currentTask = "T2";

        //Clear selection
        groupManager.selectedObjs.Clear();
        string T2;
        switch (variant)
        {
            case 6:
                T2 = "Task: Press the left trigger and say  \"Grab the pyramids\" then release it. \nOnce the objects are floating, press the left trigger and say \"Add them to the box\" then release it.";
                break;
            case 7:
                T2 = "Task: Press the left trigger and say  \"Give me red cubes\" then release it. \nOnce the objects are floating, press the left trigger and say \"Place them inside the box\" then release it.";
                break;
            case 8:
                T2 = "Task: Press the left trigger and say  \"Select the cylinders\" then release it. \nOnce the objects are floating, press the left trigger and say \"Move them to box\" then release it.";
                break;
            default:
                if (checkTypeT2[variant])
                {
                    //T2 = "Task: Grab and move all the " + targetColorT2[variant] + " " + targetTypeT2[variant] + "s to the box."; 
                    T2 = "Task: Use voice commands to select and move all the " + targetColorT2[variant] + " " + targetTypeT2[variant] + "s to the box.";
                    //T2 = "Task: Press the left trigger and say \"select all the " + targetColorT2[variant] + " " + targetTypeT2[variant] + "s\" then release it. \nOnce the objects are floating, press the left trigger and say \"Add them to the box\" then release it.";
                }
                else
                {
                    //T2 = "Task: Grab and move all the " + targetColorT2[variant] + " objects to the box.";
                    T2 =  "Task: Use voice commands to select and move all the " + targetColorT2[variant] + " objects to the box.";
                    //T2 = "Task: Press the left trigger and say  \"select all the " + targetColorT2[variant] + " objects\" then release it. \nOnce the objects are floating, press the left trigger and say \"Add them to the box\" then release it.";
                }
                break;
        }
        
        System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.None;
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[ ]{2,}", options);
        T2 = regex.Replace(T2, " ");
        textMenu.GetComponent<TextMeshProUGUI>().text = T2;

        type = targetTypeT2[variant];
        color = targetColorT2[variant];

        //Destroy selected object
        Destroy(selectedObj);

        //spawn second task
        Vector3 pos = homeZone.transform.position;
        Vector3 spawnPos = pos; 
        
        spawnPos.y = 0;
        spawnPos.z += 0.5f;

        secondTaskStruct.transform.rotation = Quaternion.identity;
        secondTaskStruct.transform.position = spawnPos;
        secondTaskStruct = Instantiate(secondTask);
    }

    public void changeToT3()
    {
        taskDone = false;
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject obj in markers)
        {
            Destroy(obj);
        }
        currentTask = "T3";
        groupManager.selectedObjs.Clear();
        string T3;
        switch (variant)
        {
            case 6:
                T3 = "Task: Press the left trigger and say  \"Brown objects\" then release it. \nOnce the objects are floating, press the left trigger and say \"Put them in a grid\" then release it.";
                break;
            case 7:
                T3 = "Task: Press the left trigger and say  \"Give me blue items\" then release it. \nOnce the objects are floating, press the left trigger and say \"Position them in a line\" then release it.";
                break;
            case 8:
                T3 = "Task: Press the left trigger and say  \"Select balls\" then release it. \nOnce the objects are floating, press the left trigger and say \"Order them in a round formation\" then release it.";
                break;
            default:
                if (checkTypeT3[variant])
                {
                    //T3 = "Task: Grab all the " + colorT3[variant] + " " + typeT3[variant] + "s and arrange them in a " + shapeT3[variant] + ". Use the crosses on the floor.";
                    T3 = "Task: Use voice commands to select all the " + colorT3[variant] + " " + typeT3[variant] + "s and arrange them in a " + shapeT3[variant] + ". Use the crosses on the floor.";
                    //T3 = "Task: Press the left trigger and say  \"select all the " + colorT3[variant] + " " + typeT3[variant] + "s\" then release it. \nOnce the objects are floating, press the left trigger and say \"Put them in a " + shapeT3[variant] + "\" then release it.";
                }
                else
                {
                    //T3 = "Task: Grab all the " + colorT3[variant] + " objects and arrange them in a " + shapeT3[variant] + ". Use the crosses on the floor."; 
                    T3 = "Task: Use voice commands to select all the " + colorT3[variant] + " objects and arrange them in a " + shapeT3[variant] + ". Use the crosses on the floor.";
                    //T3 = "Task: Press the left trigger and say  \"select all the " + colorT3[variant] + " objects \" then release it. \nOnce the objects are floating, press the left trigger and say \"Put them in a " + shapeT3[variant] + "\" then release it.";
                }
                break;
        }
        

        System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.None;
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[ ]{2,}", options);
        T3 = regex.Replace(T3, " ");

        textMenu.GetComponent<TextMeshProUGUI>().text = T3;

        type = typeT3[variant];
        color = colorT3[variant];

        Vector3 spawnPos = homeZone.transform.position;

        spawnPos.y = 0.0f;
        spawnPos.z += 0.5f;

        thirdTaskStruct.transform.rotation = Quaternion.identity;
        thirdTaskStruct.transform.position = spawnPos;
        thirdTaskStruct = Instantiate(thirdTask);

        Vector3 newPos;
        newPos = thirdTaskStruct.transform.Find("User Surface").gameObject.transform.position;

        int count = 0;
        count = countCorrectT3();

        float offset = 0f;
        GameObject newObj;
        newPos.z -= 0.5f;
        switch (shapeT3[variant])
        {
            case "row": 
                offset = 0f;
                for (int i = 0; i < count; i++)
                {
                    newObj = Instantiate(marker);
                    newPos.y = 0.01f;

                    newObj.transform.position = newPos;
                    newObj.transform.rotation = Quaternion.identity;

                    newPos.x += newObj.transform.localScale.x * 1.5f;
                    offset += newObj.transform.localScale.x * 1.5f;
                }
                offset /= 2.0f;
                foreach (GameObject i in GameObject.FindGameObjectsWithTag("Marker"))
                {
                    newPos.y = 0.01f;
                    newPos = i.transform.position;
                    newPos.x -= offset;
                    i.transform.position = newPos;
                    i.transform.rotation = Quaternion.identity;
                }
                break;
            case "circle":
                float angle = 360.0f / (float)count;
                float theta = 0.0f;
                for (int i = 0; i < count; i++)
                {
                    newObj = Instantiate(marker);
                    Vector3 posC = new Vector3(0f, 0f, 0f);
                    posC.x = newPos.x + (0.5f * Mathf.Sin(theta * Mathf.Deg2Rad));
                    posC.y = 0.01f;
                    posC.z = newPos.z + (0.5f * Mathf.Cos(theta * Mathf.Deg2Rad));

                    newObj.transform.position = posC;
                    newObj.transform.rotation = Quaternion.identity;
                    theta += angle;
                }

                break;
            case "matrix":
                newPos.z -= 0.5f;
                int n = (int)Math.Ceiling(Math.Sqrt(count));
                if (n == 1)
                {
                    n++;
                }
                int index = 0;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (index > count - 1) { return; }
                        newObj = Instantiate(marker);
                        Vector3 nPos = new Vector3();
                        nPos.x = newPos.x + (0.5f * j);
                        nPos.y = 0.01f;
                        nPos.z = newPos.z + (0.5f * i);
                        newObj.transform.position = nPos;
                        newObj.transform.rotation = Quaternion.identity;
                        index++;
                    }
                }
                break;
            default:
                break;
        }
    }

    void changeIteration()
    {
        if (currentTask == "T2")
        {
            GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
            foreach (GameObject obj in markers)
            {
                Destroy(obj);
            }
            Destroy(secondTaskStruct);
        }
        else if (currentTask == "T3")
        {
            Destroy(thirdTaskStruct);
            GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

            foreach (GameObject obj in markers)
            {
                Destroy(obj);
            }
        }
        variant++;
        t++;

    }
    void stopExperiment()
    {
        groupManager.selectedObjs.Clear();

        string TF = "The experiment has finalized. \n You can now remove the headset.";
        recordPose = false;

        tcpClient.WriteToTXT("Position, Rotation and Forward direction during experiment in meters");

        int n = Math.Min(rotList.Count, posList.Count);
        n = Math.Min(n, dirList.Count);
        for (int i = 0; i < n; i++)
        {
            tcpClient.WriteToTXT(posList[i].ToString() + " " + rotList[i].ToString() + " " + dirList[i].ToString() + "\n");
        }

        n = Math.Min(leftHandList.Count, rightHandList.Count);

        tcpClient.WriteToTXT("Left and Right hand Positions during experiment in meters");
        for (int i = 0; i < n; i++)
        {
            tcpClient.WriteToTXT("Left " + leftHandList[i].ToString() + " Right " + rightHandList[i].ToString() + "\n");
        }

        tcpClient.WriteToTXT("Overall Movement in meters : " + ovrcamPos.ToString() + " Overall Rotation: " + ovrcamRot.ToString());

        textMenu.GetComponent<TextMeshProUGUI>().text = TF;

        currentTask = "TF";

        //destroy all selected items
        Destroy(secondTaskStruct);

        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

        foreach (GameObject obj in markers)
        {
            Destroy(obj);
        }
        //change to questions menu, after 
        menuManage.changeToQuestionsMenu();

        menuManage.started = false;
        //RecordEverything();
    }

    private int countCorrectT3()
    {
        int count = 0;
        int n = thirdTaskStruct.transform.Find("Target Surface").gameObject.transform.childCount;
        for (int i = 0; i < n; i++)
        {
            GameObject obj = thirdTaskStruct.transform.Find("Target Surface").gameObject.transform.GetChild(i).gameObject;
            if (obj.tag != "Unaligned") { continue; }
            bool rightT = true;
            bool rightC = true;

            if (checkTypeT3[variant])
            {
                if (obj.name.ToLower().Contains(typeT3[variant]))
                {
                    if (obj.name.ToLower().Contains("hemisphere") && typeT3[variant] == "sphere")
                    {
                        rightT = false;
                    }
                    else
                    {
                        rightT = true;
                    }
                }
                else
                {
                    rightT = false;
                }
            }

            if (checkColT3[variant])
            {
                if (masterManager.colors[colorT3[variant]].color == obj.GetComponent<MeshRenderer>().material.color)
                {
                    rightC = true;
                }
                else
                {
                    rightC = false;
                }
            }

            if ( rightC && rightT)
            {
                count++;
            }
        }
        UnityEngine.Debug.LogError("Count: " + count.ToString());
        return count;
    }

    public void setQ1(int x) { answerQ[0] = 6 - (x + 1); }
    public void setQ2(int x) { answerQ[1] = x+1; }
    public void setQ3(int x) { answerQ[2] = 6 - (x + 1); }
    public void setQ4(int x) { answerQ[3] = 6 - (x + 1); }
    public void setQ5(int x) { answerQ[4] = x+1; }
    public void RecordEverything()
    {
        // record answers from questions 
        UnityEngine.Debug.Log("Q1 answer = " + answerQ[0].ToString());
        UnityEngine.Debug.Log("Q2 answer = " + answerQ[1].ToString());
        UnityEngine.Debug.Log("Q3 answer = " + answerQ[2].ToString());
        UnityEngine.Debug.Log("Q4 answer = " + answerQ[3].ToString());
        UnityEngine.Debug.Log("Q5 answer = " + answerQ[4].ToString());
        tcpClient.closeTXT();
        Application.Quit();
    }

    void takeData()
    {
        if (recordPose)
        {
            posList.Add(lastcamPos);
            rotList.Add(lastcamRot);
            dirList.Add(lastcamDir);
            leftHandList.Add(lastleftHand);
            rightHandList.Add(lastrightHand);

            Vector3 deltaPos = cam.centerEyeAnchor.position - lastcamPos;
            deltaPos.x = Math.Abs(deltaPos.x);
            deltaPos.y = Math.Abs(deltaPos.y);
            deltaPos.z = Math.Abs(deltaPos.z);
            ovrcamPos += deltaPos;

            Vector3 deltaRot = cam.centerEyeAnchor.rotation.eulerAngles - lastcamRot;
            deltaRot.x = Math.Abs(deltaRot.x);
            deltaRot.y = Math.Abs(deltaRot.y);
            deltaRot.z = Math.Abs(deltaRot.z);
            ovrcamRot += deltaRot;

            lastcamPos = cam.centerEyeAnchor.position;
            lastcamRot = cam.centerEyeAnchor.rotation.eulerAngles;
            lastcamDir = cam.centerEyeAnchor.forward; 
            lastleftHand = cam.leftHandAnchor.position;
            lastrightHand = cam.rightHandAnchor.position;
        }
    }

    void Update()
    {
        if (taskDone)
        {
            if (inGameZone)
            {
                taskDone = false;
                textMenu.GetComponent<TextMeshProUGUI>().text = "Well done, Task Completed";
                FinishTask();
                //Invoke("FinishTask", 1.5f);
            }
            else
            {
                textMenu.GetComponent<TextMeshProUGUI>().text = "Task Completed, go to the home zone (checkerboard on floor) ";
            }
        }
        if (rec)
        {
            RecordEverything();
        }
    }
}