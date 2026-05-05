using System;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.UI;
using UnityTCPClient.Assets.Scripts;

public class TCPClientDemo : Singleton<TCPClientDemo>
{
    // make sure you have the server IP address here
    // If both machines are connected to the same WIFT,
    // then you can directly use the IPv4 address found in the setting.
    // If you connect your headset to the computer via hotspot (which i recommand),
    // you can use the *real* local address, something like a "192.168.137.1".
    // If you would like to have a remote connection, then you need to 
    // find the global IP address of your computer and ensure the ports are open in your router.
    // It's pretty complicated so I don't put the tutorial here
    public string serverIPAddress; 

    [SerializeField]
    private int port = 65432; // should be the same port as the server and make sure it's open in your router
    // might need to close the firewall or add configurations to use the Unity editor.

    [SerializeField]
    public TcpClient client; // for information transfer
    private Thread listenerThread; // for connecting
    private Dispatcher dispatcher; // for memory access
    private bool applicationFinished; // to close the threads when application is closed
    [SerializeField]
    private MasterManager commands; // for information transfer
    private Stopwatch stopwatch;
    private StreamWriter file;
    private string fileName;

    public bool sendInfo = false;
    public string data = "";

    [SerializeField]
    private GameObject uploadIndicator;
    [SerializeField]
    private GameObject textCommand;
    [SerializeField]
    private Sprite micOn;
    [SerializeField]
    private Sprite micOff;
    [SerializeField]
    private GameObject microphoneIndicator;

    private AudioClip userAudio;
    private GameObject speaker;
    private string mic = "";
    private long startOfClip = 0;
    private long endOfClip = 0;

    // create Singleton object before threads are created. 
    void Awake()
    {
        dispatcher = Dispatcher.Instance;
    }
    void Start()
    {
        applicationFinished = false;
        try
        {
            client = new TcpClient(serverIPAddress, port); // trying to connect to the server
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e + "\n");
        }
        listenerThread = new Thread(ConnectClient);
        listenerThread.Start();

        speaker = GameObject.Find("Speaker (1)");
        fileName = "logConventional_" + DateTime.Now.ToString();
        fileName = fileName.Replace("/", "");
        fileName = fileName.Replace(" ", "");
        fileName = fileName.Replace(".", "");
        fileName = fileName.Replace(":", "");
        fileName = "/" + fileName + ".txt";
        string path = (Application.persistentDataPath + fileName);
        UnityEngine.Debug.Log(path);
        using (FileStream f = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        using (StreamWriter file = new StreamWriter(f))
        {
            file.WriteLine("[" + DateTime.Now.ToString() + "] " + "Start of log file");
        }

        string[] devices = Microphone.devices;
        mic = devices[0];
        userAudio = Microphone.Start(mic, true, 200, 16000);
    }


    private void ConnectClient() // You can directly use this
    {
        try
        {
            NetworkStream stream = client.GetStream();

            while (!applicationFinished)
            {
                byte[] buffer = new byte[10000000];
                int byteCount = stream.Read(buffer, 0, buffer.Length);
                byte[] dataTemp = new byte[byteCount];
                Buffer.BlockCopy(buffer, 0, dataTemp, 0, byteCount);
                dispatcher.Enqueue(() => HandleMessage(dataTemp));
            }
        }
        catch (Exception e)
        {
            dispatcher.Enqueue(() => PrintToDebugText(e));
        }

        Console.Read();
    }

    private void HandleMessage(byte[] dataTemp)
    {
        StartTimer();
        uploadIndicator.SetActive(false);
        if (!System.Text.Encoding.ASCII.GetString(dataTemp).Contains("("))
        {
            if (System.Text.Encoding.ASCII.GetString(dataTemp).Replace(".", "") == "idonotunderstand")
            {
                textCommand.GetComponent<TextMeshProUGUI>().color = new Color(255f, 0f, 0f);
                WriteToTXT("Command: " + System.Text.Encoding.ASCII.GetString(dataTemp) + "\n");
            }
            else
            {
                WriteToTXT("STT: " + System.Text.Encoding.ASCII.GetString(dataTemp).Replace("STT:", "") + "\n");
            }
            textCommand.GetComponent<TextMeshProUGUI>().text = System.Text.Encoding.ASCII.GetString(dataTemp).Replace("STT:", "");
        }
        else
        {
            WriteToTXT("Command: " + System.Text.Encoding.ASCII.GetString(dataTemp) + "\n");
            //understood
            if (commands.userIntention(System.Text.Encoding.ASCII.GetString(dataTemp)))
            {
                WriteToTXT("Command executed \n");
                textCommand.GetComponent<TextMeshProUGUI>().color = new Color(0f, 255f, 0f);
                
            }
            //not understood
            else
            {
                WriteToTXT("Command not executed \n");
                textCommand.GetComponent<TextMeshProUGUI>().color = new Color(255f, 0f, 0f);
            }
        }
        Invoke("EmptySTT", 2.0f);
        StopTimer("TCP/IP response");
    }
    private void EmptySTT()
    {
        textCommand.GetComponent<TextMeshProUGUI>().text = "";
        textCommand.GetComponent<TextMeshProUGUI>().color = new Color(255f, 255f, 255f);
    }
    public void PrintToDebugText(Exception e) // for debugging
    {
        UnityEngine.Debug.LogError(e + "\n");
    }

    public void SendString(string message) // you can directly use this for sending string
    {
        StartTimer();
        uploadIndicator.SetActive(true);
        SendMessage(System.Text.Encoding.ASCII.GetBytes(message));  
        StopTimer("TCP/IP sending");
    }
    public void StartTimer()
    {
        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    public void StopTimer(String source)
    {
        stopwatch.Stop();
        try
        {
            string path = (Application.persistentDataPath + fileName);
            using (FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write))
            using (StreamWriter file = new StreamWriter(f))
            {
                file.WriteLine("[" + DateTime.Now.ToString() + "] "  + source + "," + stopwatch.Elapsed.TotalMilliseconds.ToString() + "ms");
            }
        }
        catch (Exception e)
        {
            PrintToDebugText(e);
        }
    }

    public void SendMessage(byte[] message) // you can directly use this for sending byte[]
    {
        try
        {
            StartTimer();
            uploadIndicator.SetActive(true);
            client.GetStream().Write(message, 0, message.Length);
            StopTimer("TCP/IP sending");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e + "\n");
        }
    }

    public void SendVector3(Vector3 vector)
    {
        // I suggest converting the vectors ahead and use the SendMessage Function instead
        // Here is an example of converting Vector3 to byte[]

        byte[] message = new byte[1 * sizeof(float)];
        int cursor = 0;
        Array.Copy(BitConverter.GetBytes(vector.x), 0, message, cursor, sizeof(float));
        Array.Copy(BitConverter.GetBytes(vector.y), 0, message, cursor += sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(vector.z), 0, message, cursor += sizeof(float), sizeof(float));
        SendMessage(message);
    }

    public void WriteToTXT(string a)
    {
        try
        {
            string path = (Application.persistentDataPath + fileName);
            using (FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write))
            using (StreamWriter file = new StreamWriter(f))
            {
                file.WriteLine("[" + DateTime.Now.ToString() + "] " + a);
            }
        }
        catch (Exception e)
        {
            PrintToDebugText(e);
        }
    }

    public void startHearing()
    {
        try
        {
            microphoneIndicator.GetComponent<Image>().sprite = micOn;
            userAudio = Microphone.Start(mic, false, 20, 16000);
            startOfClip = 0;
        }
        catch(Exception e)
        {
            PrintToDebugText(e);
            WriteToTXT(e.ToString() + "\n");
        }
        
    }

    public void stopHearing()
    {
        long n = 0;
        if (Microphone.IsRecording(mic))
        {
            try
            {
                if (userAudio.length < 0.5f) { return; }
                microphoneIndicator.GetComponent<Image>().sprite = micOff;
                endOfClip = Microphone.GetPosition(mic);
                n = endOfClip - startOfClip;
                float[] d = new float[n];
                
                bool res = userAudio.GetData(d, (int)startOfClip);
                if (d.Length < 10) { return; }
                AudioClip voice = AudioClip.Create("Voice", d.Length, userAudio.channels, userAudio.frequency, false);

                voice.SetData(d, 0);

                Microphone.End(mic);
                
                speaker.GetComponent<AudioSource>().clip = voice;
                //speaker.GetComponent<AudioSource>().Play();
                string fName = "Test.wav";
                SavWav.Save(fName, voice);
                var filepath = Path.Combine(Application.persistentDataPath, fName);
                byte[] data = File.ReadAllBytes(filepath);

                SendMessage(data);
                SendString("EOF_MARKER");
            }
            catch (Exception e)
            {
                WriteToTXT(e.ToString()+ "\n");
                WriteToTXT("startofC " + startOfClip.ToString());
                WriteToTXT("endofC " + endOfClip.ToString());
                WriteToTXT("SAMPLES: " + n);
            }
        }
        else
        {
            try {
                if(userAudio.length < 0.5f) { return; }
                speaker.GetComponent<AudioSource>().clip = userAudio;
                //speaker.GetComponent<AudioSource>().Play();
                string fName = "Test.wav";
                SavWav.Save(fName, userAudio);
                var filepath = Path.Combine(Application.persistentDataPath, fName);
                byte[] data = File.ReadAllBytes(filepath);

                SendMessage(data);
                SendString("EOF_MARKER");
            }
            catch (Exception e)
            {
                WriteToTXT(e.ToString() + "\n");

                UnityEngine.Debug.LogWarning(e);

            }
        }
    }

    public void closeTXT()
    {
        string path = (Application.persistentDataPath + fileName);
        using (FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write))
        using (StreamWriter file = new StreamWriter(f))
        {
            file.WriteLine("[" + DateTime.Now.ToString() + "] " + "Ended experiment");
        }
    }

    private void Update()
    {
        if (sendInfo && data != "")
        {
            SendString(data);
            sendInfo = false;
        }
        else if (sendInfo && data == "")
        {
            sendInfo = false;
        }
    }

    public void OnApplicationQuit()
    {
        UnityEngine.Debug.Log("onapplicationquit");
        applicationFinished = true;
        client.Close();
        listenerThread.Join(); 
    }
}
 