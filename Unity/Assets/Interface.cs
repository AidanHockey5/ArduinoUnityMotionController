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
    CONNECTED
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
        string port = portDropdown.options[portDropdown.value].text;
        sp = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
        sp.Open();
        sp.ReadTimeout = timeOut;
        StartCoroutine(CheckPortForAck());
    }

    IEnumerator CheckPortForAck()
    {
        while(connectionState == ConnectionState.DISCOVER)
        {
            SendToArduino("DISCOVER");
            outputLog.text = "Send DISCOVER to port.";
            yield return new WaitForSeconds(0.5f);
            string reply;
            do
            {
                reply = CheckForRecievedData();
                yield return null;
            } while (reply == string.Empty);
            //string reply = CheckForRecievedData();
            outputLog.text = "Device reply: " + reply;
            if (reply == "ACKNOWLEDGE")
            {
                connectionState = ConnectionState.CONNECTED;
            }
            else
            {
                sp.Close();
                outputLog.text = "Connection failed.";
                break;
            }
        }
    }

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            sp.Close();
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
            sp.BaseStream.Flush();
            return inData;
        }
        catch { Debug.Log("NO DATA AVAILABLE"); return string.Empty; }
    }

}
