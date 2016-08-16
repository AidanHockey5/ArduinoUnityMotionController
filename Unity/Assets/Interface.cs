using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;

public class Interface : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM6", 9600, Parity.None, 8, StopBits.One);
    ushort timeOut = 1; //Important value. If not set, code will check for serial input forever.
    float inputUpdateRate = 1.5f; //How often should we check for new incomming serial data?
    // Use this for initialization
    void Start ()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        sp.Open();
        sp.ReadTimeout = timeOut;
        InvokeRepeating("CheckForRecievedData", inputUpdateRate, inputUpdateRate);
    }
	
	// Update is called once per frame
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
            CheckForRecievedData();
        }
        else
            Debug.LogError("Serial Port: " + sp.PortName + " is unavailable");
    }

    void OnProcessExit(object sender, EventArgs e)
    {
        sp.Close();
    }
    
    public void CheckForRecievedData()
    {
        try
        {
            string inData = sp.ReadLine();
            int inSize = inData.Count();
            if (inSize > 0)
                Debug.Log("ARDUINO->|| " + inData + " ||MSG SIZE:" + inSize.ToString());
            sp.BaseStream.Flush();
        }
        catch { Debug.Log("NO DATA AVAILABLE"); }
    }

}
