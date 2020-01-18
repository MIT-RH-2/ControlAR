// MIT Reality Hack 2020 project //
// Weston Bell-Geddes, Jay Hesslink, Thomas Suarez //
// Jan 17-19, 2020 //

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class RgbNet : MonoBehaviour
{

    [SerializeField]
    private string Host = "127.0.0.1";

    [SerializeField]
    private int Port = 8000;

    private WebSocket ws;
    private UdpClient udp;

    void Start() {
        StartCoroutine(this.openWs());
        this.openUdp();

        StartCoroutine(this.testWsUdp());
    }

    private IEnumerator testWsUdp() {
        // this.sendWs("test");
        while (true) {
            this.sendUdp("test");
            Debug.Log("UDP sent");
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator openWs() {
        string url = "ws://" + this.Host + ":" + this.Port;
        this.ws = new WebSocket(new System.Uri(url));

        Debug.Log("WebSocket opening...");
        yield return StartCoroutine(this.ws.Connect());
        Debug.Log("WebSocket opened!");

        while (true) {
            string msg = this.ws.RecvString();
            if (msg != null) {
                Debug.Log("WS recv: " + msg);
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
    
    private void openUdp() {
        Debug.Log("UDP opening...");

        this.udp = new UdpClient(this.Port + 1);
        try {
            this.udp.Connect(this.Host, this.Port);
            Debug.Log("UDP opened!");
        } catch (Exception e) {
            Debug.LogError("UDP error: " + e.Message);
        }
    }
    
    private void sendWs(string data) {
        // Null check
        if (this.ws == null) {
            Debug.LogWarning("WebSocket is null or not connected!");
            return;
        }

        this.ws.SendString(data);
    }

    private void sendUdp(string data) {
        // Null check
        if (this.udp == null) {
            Debug.LogWarning("UDP is null or not connected!");
            return;
        }

        try {
            byte[] b = Encoding.ASCII.GetBytes(data);
            this.udp.Send(b, b.Length);
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    void Update() {
        
    }
}
