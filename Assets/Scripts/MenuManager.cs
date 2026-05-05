using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Voice.Dictation;
using Meta.WitAi.TTS.Utilities;
using Meta.Voice;
using Oculus.Voice;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    private GameObject player;
    private AppDictationExperience speech2text;

    public GameObject spawnMenu;
    public GameObject mainMenu;
    public GameObject selectMenu;
    public GameObject colorMenu;
    public GameObject inspectMenu;

    public GameObject scaleMenu;
    public GameObject posingMenu;
    public GameObject questionsMenu;
    public GameObject devMenu;
    public GameObject menu;
    public GameObject tasksMenu;
    public GameObject collider;

    public MasterManager masterManage;
    public TCPClientDemo tcpClient;

    [SerializeField]
    private TaskManager taskManager;

    [SerializeField]
    private TTSSpeaker speaker;

    private bool menusOn = false;
    public bool changeTask = false;
    public Meta.WitAi.Configuration.WitDictationRuntimeConfiguration spanish;
    public Meta.WitAi.Configuration.WitDictationRuntimeConfiguration english;
    public bool experimentOn = false;
    public bool started = false;
    //conventional 
    //private bool start = false;
    
    //experimental
    private bool heard = true;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<OVRPlayerController>().gameObject;
        speech2text = FindObjectOfType<AppDictationExperience>();
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Primitive");

        foreach (GameObject block in objects)
        {
            block.GetComponent<StaticWireframeRenderer>().enabled = false;
            block.GetComponent<RayInteractable>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) && heard && started)
        {
            //speech2text.Activate();
            tcpClient.startHearing();
            tcpClient.WriteToTXT("Pressed Trigger" + "\n");
            heard = false;
        }

        if ((OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger)) && !heard && started)
        {
            //speech2text.Deactivate();
            tcpClient.stopHearing();
            tcpClient.WriteToTXT("Released Trigger" + "\n");
            heard = true;
        }

        //conventional
        //if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !start)
        //{
        //    taskManager.StartTask();

        //    start = true;
        //}

        if (OVRInput.GetUp(OVRInput.RawButton.Y) && !started)
        {
            taskManager.StartTask();
            started = true;
        }

        if (changeTask)
        {
            if (taskManager.currentTask == "T0")
            {
                //speaker.Speak("Starting experiment");
                taskManager.StartTask();
            }
            else
            {
                taskManager.FinishTask();
            }
            changeTask = !changeTask;
        }
    }

    public void turnMenu()
    {
        menusOn = !menusOn;
        if (menusOn)
        {
            Vector3 pos = player.transform.position;
            Vector3 dir = player.transform.forward;
            Vector3 spawnPos = pos + dir * 2; //spawn menu 5 m in front of user

            menu.transform.position = spawnPos;
            menu.transform.LookAt(pos);
            menu.transform.forward *= -1;

            mainMenu.SetActive(true);
            spawnMenu.SetActive(false);
            selectMenu.SetActive(false);
            inspectMenu.SetActive(false);
            colorMenu.SetActive(false);
            
            scaleMenu.SetActive(false);
            posingMenu.SetActive(false);
            devMenu.SetActive(false);

            changeToSpawnMenu();

            menu.GetComponent<PointableCanvas>().enabled = true;
            menu.GetComponent<RayInteractable>().enabled = true;
        }
        else {
            mainMenu.SetActive(false);
            spawnMenu.SetActive(false);
            selectMenu.SetActive(false);
            inspectMenu.SetActive(false);
            colorMenu.SetActive(false);
            
            scaleMenu.SetActive(false);
            devMenu.SetActive(false);
            posingMenu.SetActive(false);

            menu.GetComponent<PointableCanvas>().enabled = false;
            menu.GetComponent<RayInteractable>().enabled = false;
        }
    }

    public void changeToMainMenu()
    {
        mainMenu.SetActive(true);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(false);
        colorMenu.SetActive(false);
        inspectMenu.SetActive(false);
        
        scaleMenu.SetActive(false);
        devMenu.SetActive(false);
        posingMenu.SetActive(false);
    }
    public void changeToSpawnMenu()
    {
        mainMenu.SetActive(false);
        spawnMenu.SetActive(true);
        selectMenu.SetActive(false);
        colorMenu.SetActive(false);
        inspectMenu.SetActive(false);
        
        scaleMenu.SetActive(false);
        devMenu.SetActive(false);
        posingMenu.SetActive(false);
    }
    private void changeToDevMenu()
    {
        devMenu.SetActive(true);
    }
    public void changeToSelectMenu()
    {
        mainMenu.SetActive(false);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(true);
        inspectMenu.SetActive(false);
        colorMenu.SetActive(false);
        
        scaleMenu.SetActive(false);
        posingMenu.SetActive(false);
        devMenu.SetActive(false);
    }

    public void changeToColorMenu()
    {
        mainMenu.SetActive(false);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(false);
        inspectMenu.SetActive(false);
        colorMenu.SetActive(true);
        
        scaleMenu.SetActive(false);
        posingMenu.SetActive(false);
        devMenu.SetActive(false);
    }
    public void changeToSpeechMenu()
    {
        mainMenu.SetActive(false);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(false);
        inspectMenu.SetActive(false);
        colorMenu.SetActive(false);
        scaleMenu.SetActive(false);
        posingMenu.SetActive(false);
        devMenu.SetActive(false);
        menusOn = true;

        Vector3 pos = player.transform.position;
        Vector3 dir = player.transform.forward;
        Vector3 spawnPos = pos + dir * 1.0f; //spawn menu 2 m in front of user

        menu.transform.position = spawnPos;
    }

    public void changeToInspectMenu()
    {
        if (taskManager.currentTask != "T1" && taskManager.currentTask != "T2")
        {
            mainMenu.SetActive(false);
            spawnMenu.SetActive(false);
            selectMenu.SetActive(false);
            colorMenu.SetActive(false);
            inspectMenu.SetActive(true);
            
            scaleMenu.SetActive(false);
            posingMenu.SetActive(false);
            devMenu.SetActive(false);

            Vector3 pos = player.transform.position;
            Vector3 dir = player.transform.forward;
            Vector3 spawnPos = pos + dir * 1.0f; //spawn menu 2 m in front of user

            spawnPos.y -= 0.5f;
            menu.transform.position = spawnPos;
            menusOn = true;
        }
        //else speaker.Speak("Inspection mode currently not available");
    }
    public void changeToScaleMenu()
    {
        mainMenu.SetActive(false);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(false);
        inspectMenu.SetActive(false);
        colorMenu.SetActive(false);
        
        posingMenu.SetActive(false);
        scaleMenu.SetActive(true);
        devMenu.SetActive(false);
    }

    public void changeToPosingMenu()
    {
        mainMenu.SetActive(false);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(false);
        inspectMenu.SetActive(false);
        colorMenu.SetActive(false);
        
        scaleMenu.SetActive(false);
        posingMenu.SetActive(true);
        devMenu.SetActive(false);
    }

    public void changeToQuestionsMenu()
    {
        experimentOn = false;

        menu.SetActive(true);
        mainMenu.SetActive(false);
        spawnMenu.SetActive(false);
        selectMenu.SetActive(false);
        inspectMenu.SetActive(false);
        colorMenu.SetActive(false);
        
        scaleMenu.SetActive(false);
        posingMenu.SetActive(false);
        devMenu.SetActive(false);
        tasksMenu.SetActive(false);
        questionsMenu.SetActive(true);

        collider.GetComponent<BoxCollider>().size = new Vector3(6.0f, 5.0f, 0.01f);
        collider.GetComponent<BoxCollider>().center = new Vector3(0.0f, 1.0f, 0.0f);

        menu.GetComponent<PointableCanvas>().enabled = true;
        menu.GetComponent<RayInteractable>().enabled = true;
    }
    public void logDebug(string txt)
    {
        Debug.Log(txt);
    }

    public void changeLanguage(Meta.WitAi.Configuration.WitDictationRuntimeConfiguration newLanguage)
    {
        speech2text.RuntimeDictationConfiguration = newLanguage;
        speech2text.enabled = false;
        speech2text.enabled = true;
        //speaker.Speak("Language changed");
    }

    public void restartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}