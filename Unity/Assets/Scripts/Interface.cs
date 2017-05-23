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
    public Dropdown portDropdown;
    public Text outputLog;
    SerialPort sp; 
    ushort timeOut = 10; //Important value. If not set, code will check for serial input forever.
    float inputUpdateRate = 1.5f; //How often should we check for new incomming serial data?
    public ConnectionState connectionState = ConnectionState.WAIT;
    // Use this for initialization
    void Start ()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        String[] ports = SerialPort.GetPortNames();
        portDropdown.AddOptions(ports.ToList());
        //sp.Open();
        //sp.ReadTimeout = timeOut;

        //InvokeRepeating("CheckForRecievedData", inputUpdateRate, inputUpdateRate);
    }

    public void CheckPort()
    {
        connectionState = ConnectionState.DISCOVER;
        outputLog.color = Color.white;
        string port = portDropdown.options[portDropdown.value].text;
        if (sp != null)
            sp.Close();
        sp = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
        
        sp.Open();
        sp.ReadTimeout = timeOut;
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
    }

    private void FixedUpdate() //Check for commands 60 times a second
    {
        if(connectionState == ConnectionState.CONNECTED)
        {
            CheckForRecievedData();
        }
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

}
