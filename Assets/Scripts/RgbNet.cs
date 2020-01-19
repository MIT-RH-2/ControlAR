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

    private WebSocket ws;
    private UdpClient udp;

    public int CurFrame = 0;

    public IEnumerator SendFrameChunks(byte[] frameFull) {
        const int numChunks = 40;
        int chunkSize = frameFull.Length / numChunks;

        byte[][] chunks = new byte[numChunks][];
        for (int i = 0; i < frameFull.Length; i++) {
            int start = chunkSize * i;
            int end = chunkSize * (i + 1) - 1;

            if (i == frameFull.Length - 1) {
                end = frameFull.Length;
            }

            chunks[i] = this.GetFrameChunk(frameFull, start, end);
            Debug.Log("Chunks length: " + chunks[i].Length);
            this.sendUdp(chunks[i]);

            yield return new WaitForSeconds(0.02f);//1f / (float) numChunks);
        }
    }

    private byte[] GetFrameChunk(byte[] full, int start, int end) {
        byte[] chunk = new byte[end - start];
        for (int i = start; i < end; i++) {
            chunk[i - start] = full[i];
        }
        return chunk;
    }

    ///// LIFECYCLE /////

    void Awake() {
        RgbNet.Shared = this;
    }

    void Start() {
        // Start networking
        StartCoroutine(this.openWs());
        this.openUdp();

        // Test networking
        //StartCoroutine(this.testWsUdp());
    }

    ///// NETWORKING /////

    private IEnumerator testWsUdp() {
        // this.sendWs("test");
        while (true) {
            this.sendUdp("test");
            Debug.Log("UDP sent");
            yield return new WaitForSeconds(0.25f);
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
                ControlArModel.Shared.TransformObject(msg);
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
}
