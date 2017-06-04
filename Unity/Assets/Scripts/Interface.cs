using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;

public enum ConnectionState
{
    WAIT,
    DISCOVER,
    CONNECTED,
    TERMINATED
}

public class Interface : MonoBehaviour
{
    static GameObject instance;
    static float zoomLevel = 0;
    public Transform playerTarget;
    Grapple playerGrapple;
    public Dropdown portDropdown;
    public Text outputLog;
    public Vector3 acclData = Vector3.zero;
    public Vector3 acclCalibrated = Vector3.zero;
    bool isCalibrated = false;
    Matrix4x4 calibrationMatrix;
    Vector3 wantedDeadZone = Vector3.zero;
    string[] stringDelimiters = new string[] { ":", "R", "Z", "COL" };
    SerialPort sp; 
    ushort timeOut = 1; //Important value. If not set, code will check for serial input forever.
    public ConnectionState connectionState = ConnectionState.WAIT;

    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
        {
            instance = this.gameObject;
            DontDestroyOnLoad(instance);
        }
    }

    void Start ()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        String[] ports = SerialPort.GetPortNames();
        portDropdown.AddOptions(ports.ToList());
        FindPlayer();
    }

    public void CheckPort()
    {
        connectionState = ConnectionState.DISCOVER;
        isCalibrated = false;
        outputLog.color = Color.white;
        string port = portDropdown.options[portDropdown.value].text;
        if (sp != null)
            sp.Close();
        sp = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
        sp.DtrEnable = false;
        sp.Open();
        sp.ReadTimeout = timeOut;
        sp.WriteTimeout = 1;
        StartCoroutine(CheckPortForAck());
    }

    IEnumerator CheckPortForAck()
    {
        while(connectionState == ConnectionState.DISCOVER)
        {
            float connectTime = 0;
            SendToArduino("DISCOVER");
            string reply;
            do
            {
                reply = CheckForRecievedData();
                connectTime += Time.deltaTime;
                outputLog.text = "Send DISCOVER to port. T+: " + connectTime.ToString("0.0");
                yield return null;
            } while (reply == string.Empty && connectTime < 5);
            //string reply = CheckForRecievedData();
            outputLog.text = "Device reply: " + reply;
            if (reply == "ACKNOWLEDGE")
            {
                connectionState = ConnectionState.CONNECTED;
                outputLog.color = Color.green;
            }
            else
            {
                sp.Close();
                outputLog.text = "Connection failed.";
                outputLog.color = Color.red;
                break;
            }
        }
    }

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseConnection();
        if (Input.GetKeyDown(KeyCode.A))
            Application.LoadLevel(Application.loadedLevel);
    }

    private void FixedUpdate() //Check for commands 60 times a second
    {
        if(connectionState == ConnectionState.CONNECTED)
        {
            string cmd = CheckForRecievedData();
            if (cmd == string.Empty)
                return;
            if(cmd.StartsWith("R"))//Recieved Accelerometer Command
            {
                acclData = ParseAccelerometerData(cmd);
                if(!isCalibrated)
                {
                    isCalibrated = true;
                    CalibrateAccelerometer();
                }
            }
            if (cmd == "D")
            {
                if (playerGrapple != null)
                    playerGrapple.FireGrapple();
                Debug.Log("BUTTON_DOWN");
            }

            if (cmd == "U")
            {
                if (playerGrapple != null)
                    playerGrapple.ReleaseGrapple();
                Debug.Log("BUTTON_UP");
            }
            if (cmd == "C")
            {
                CalibrateAccelerometer();
                Debug.Log("CALIBRATE");
            }
            if (cmd.StartsWith("COL"))
            {
                if (Camera.main != null)
                    ParseRGB(cmd);
                Debug.Log("COLOR");
            }
            if (cmd.StartsWith("Z"))
            {
                if (Camera.main != null)
                    AdjustZoom(cmd);
                Debug.Log("ZOOM");
            }
            Camera.main.orthographicSize = zoomLevel;
            acclCalibrated = GetAccelerometer(acclData);
            acclCalibrated = new Vector3(0, 0, -acclCalibrated.y);
            if(playerTarget != null)
                playerTarget.transform.rotation = Quaternion.Slerp(playerTarget.transform.rotation, Quaternion.Euler(acclCalibrated), Time.deltaTime*10);
        }
    }

    void ParseRGB(string cmd)
    {
        try
        {
            string[] splitResult = cmd.Split(stringDelimiters, StringSplitOptions.RemoveEmptyEntries);
            byte r = byte.Parse(splitResult[0]);
            byte g = byte.Parse(splitResult[1]);
            byte b = byte.Parse(splitResult[2]);
            Color32 color = new Color32(r, g, b, 1);
            Camera.main.GetComponent<SkyboxColor>().SetColor(color);
        }
        catch { Debug.Log("Malformed RGB Command"); }
    }

    public void SendColorToArduino(Color c)
    {
        if (connectionState != ConnectionState.CONNECTED)
            return;
        Color32 convertTo32 = c;
        string command = "C:" + convertTo32.r.ToString() + ":" + convertTo32.g.ToString() + ":" + convertTo32.b.ToString();
        SendToArduino(command);
    }

    public void RumbleMotor(float activeTime)
    {
        StartCoroutine(SendRumbleCommands(activeTime));
    }

    IEnumerator SendRumbleCommands(float activeTime)
    {
        SendToArduino("12:1:1"); //Set pin 12 as output and set high.
        yield return new WaitForSeconds(activeTime);
        SendToArduino("12:1:0"); //Set pin 12 as output and set low.
        yield return null;
    }

    float lastZoomValue = 0f;
    void AdjustZoom(string data)
    {
        try
        {
            string[] splitResult = data.Split(stringDelimiters, StringSplitOptions.RemoveEmptyEntries);
            zoomLevel = float.Parse(splitResult[0]);
        }
        catch { Debug.Log("Malformed Zoom Command"); }
    }

    Vector3 lastAccData = Vector3.zero;
    Vector3 ParseAccelerometerData(string data)
    {
        try
        {
            string[] splitResult = data.Split(stringDelimiters, StringSplitOptions.RemoveEmptyEntries);
            int x = int.Parse(splitResult[0]);
            int y = int.Parse(splitResult[1]);
            int z = int.Parse(splitResult[2]);
            lastAccData = new Vector3(x,y,z);
            return lastAccData;
        } catch {Debug.Log("Malformed Serial Transmisison"); return lastAccData; }
    }

    void CalibrateAccelerometer()
    {

        wantedDeadZone = acclData;
        Quaternion rotateQuaternion = Quaternion.FromToRotation(new Vector3(0f, 0f, 1f), wantedDeadZone);
        //create identity matrix ... rotate our matrix to match up with down vec
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotateQuaternion, new Vector3(1f, 1f, 1f));
        //get the inverse of the matrix
        calibrationMatrix = matrix.inverse;

    }

    Vector3 GetAccelerometer(Vector3 accelerator)
    {
        Vector3 accel = this.calibrationMatrix.MultiplyVector(accelerator);
        return accel;
    }

    public void SendToArduino(string cmd)
    {
        if (sp.IsOpen)
        {
            cmd = cmd.ToUpper();
            sp.Write(cmd);
        }
        else
            Debug.LogError("Serial Port: " + sp.PortName + " is unavailable");
    }

    void OnProcessExit(object sender, EventArgs e)
    {
        CloseConnection();
    }
    
    void CloseConnection()
    {
        SendToArduino("TERMINATE");
        outputLog.color = Color.yellow;
        outputLog.text = "Connection Terminated";
        connectionState = ConnectionState.TERMINATED;
        sp.Close();
    }

    public string CheckForRecievedData()
    {
        try
        {
            string inData = sp.ReadLine();
            int inSize = inData.Count();
            if (inSize > 0)
            {
                Debug.Log("ARDUINO->|| " + inData + " ||MSG SIZE:" + inSize.ToString());
            }
            inSize = 0;
            
            sp.BaseStream.Flush();
            sp.DiscardInBuffer();
            return inData;
        }
        catch { return string.Empty; }
    }

    private void OnLevelWasLoaded(int level)
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject tmp = GameObject.FindGameObjectWithTag("Player");
        if (tmp != null)
        {
            playerTarget = tmp.transform;
            playerGrapple = playerTarget.GetComponent<Grapple>();
        }
        else
            Debug.Log("No Player Object Found.");
    }
}
