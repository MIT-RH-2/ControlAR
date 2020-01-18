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

    // Singleton //
    public static RgbNet Shared;

    [SerializeField]
    private string Host = "127.0.0.1";

    [SerializeField]
    private int Port = 8001;
    
    [SerializeField]
    private float SmoothingTime = 0.25f;

    [SerializeField]
    private GameObject TargetObject;

    private Vector3 TargetAngles;

    private WebSocket ws;
    private UdpClient udp;

    ///// LIFECYCLE /////

    void Awake() {
        RgbNet.Shared = this;
    }

    void Start() {
        // Null checks
        if (this.TargetObject == null) {
            Debug.LogError("Target object is null! Please assign it in the Inspector GUI.");
        }

        if (this.TargetObject.GetComponent<Rigidbody>() == null) {
            Debug.LogError("Target object does not have a Rigidbody attached. " +
                "Please attach the Rigidbody component in the Inspector GUI.");
        }

        // Start networking
        StartCoroutine(this.openWs());
        this.openUdp();

        // Test networking
        StartCoroutine(this.testWsUdp());

        // Start object motion
        // this.ApplyContinuousForce();
        StartCoroutine(this.SmoothTransform());
    }

    ///// NETWORKING /////

    private IEnumerator testWsUdp() {
        // this.sendWs("test");
        while (true) {
            this.sendUdp("test");
            Debug.Log("UDP sent");
            yield return new WaitForSeconds(this.SmoothingTime);
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
                this.TransformObject(msg);
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

    public void sendUdp(byte[] data) {
        string s = Encoding.ASCII.GetString(data, 0, data.Length);
        this.sendUdp(s);
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

    ///// TRANSFORM & ANIMATION /////

    private void ApplyContinuousForce() {
        Rigidbody rb = this.TargetObject.GetComponent<Rigidbody>();
        rb.AddForce(this.TargetObject.transform.forward, ForceMode.Force);
    }

    private void TransformObject(string transformStr) {
        string[] arr = transformStr.Split(',');
        float rotX = float.Parse(arr[0]);
        float rotY = float.Parse(arr[1]);
        float rotZ = float.Parse(arr[2]);

        this.TargetAngles = new Vector3(rotX, rotY, rotZ);
    }

    private IEnumerator SmoothTransform() {
        while (this.TargetObject != null) {

            float t = 0f;
            while (t < this.SmoothingTime) {
                this.TargetObject.transform.eulerAngles = Vector3.Slerp(
                    this.TargetObject.transform.eulerAngles,
                    this.TargetAngles,
                    t / this.SmoothingTime
                );
                Debug.Log(t);
                Debug.Log(this.TargetObject.transform.eulerAngles);
                t += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }

        }
    }
}
