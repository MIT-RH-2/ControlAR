using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Teleportal;

public class ControlArModel : MonoBehaviour
{
    
    // Singleton reference //
    public static ControlArModel Shared;

    [SerializeField]
    private float RotationSendInterval = 0.25f;

    [SerializeField]
    private float SmoothingTime = 0.25f;


    // tldr; stream and return phoneVect (euclidean angles) and ignore the x direction, read chat to see why

    // short answer is that the orientation requested by Weston means that the x direction is completely arbitrary with respect to inital
    // bootup of the app. this has to happen in one of them and it is x in ours. So just use y and z rotation to manipulate the paper airplane.
    // I'll send a screenshot showing what those rotations are in real life. This package is good to be downloaded onto an android and run, and the UI is passable haha although
    // Weston can look over it if he likes. The variable ' phoneVect ' is what you want to stream if you want euclidean. return phoneQuat if you want Quaternion
    private Quaternion phoneQuat;
    private Vector3 phoneVect;
    private Quaternion TargetRot;

    private TPObject tpo;
    private ConstantForce forceComponent;

    // only used on mobile (force "sending" device)
    private bool networkForceEnabled = false;

    ///// LIFECYCLE /////

    void Awake() {
        this.tpo = TPObject.get(this);
        this.forceComponent = this.GetComponent<ConstantForce>();
    }

    void Start() {
        // Subscribe listener (all platforms -- desktop, mobile, Magic Leap)
        this.tpo.Subscribe("accel", this.TransformObject);
        this.tpo.Subscribe("force", this.OnForce);

        // Start sending accelerometer data (iOS/Android only)
        if (Teleportal.TPDeviceInfo.IsMobile) {
            this.tpo.locked = true; // local->server transform lock
            Input.gyro.enabled = true;
            this.StartCoroutine(this.SendAccelRepeated(this.RotationSendInterval));
        }
        
        // Login automatically (Magic Leap only)
        else {
            #if PLATFORM_LUMIN
                this.StartCoroutine(this.AutoLogin());
            #endif
        }

        // Start object motion
        StartCoroutine(this.SmoothTransform());
        //this.ApplyContinuousForce(); // now handled by Constant Force behaviour (see Inspector GUI)
    }

    private IEnumerator AutoLogin() {
        // Wait for connection to Teleportal network
        while (!Teleportal.Teleportal.tp.IsConnected()) {
            yield return new WaitForSeconds(0.1f);
        }

        Teleportal.Teleportal.tp.SetUsername("mitrealityhack-ml");
        Teleportal.AuthModule.Shared.AuthGUI.SetActive(false);
        Teleportal.AuthModule.Shared.Authenticate();
    }

    ///// DEVICE MOTION /////

    private IEnumerator SendAccelRepeated(float interval) {
        Quaternion deviceQuat;
        Vector3 deviceEuler;
        
        // Wait for connection to Teleportal network
        while (!Teleportal.Teleportal.tp.IsConnected()) {
            yield return new WaitForSeconds(0.1f);
        }

        while (Teleportal.Teleportal.tp.IsConnected()) {
            // Get rotation
            deviceQuat = Input.gyro.attitude;
            deviceQuat = this.GyroToUnity(deviceQuat);
            deviceEuler = deviceQuat.eulerAngles;

            // Send via network
            string deviceStr = deviceQuat.x + "," + deviceQuat.y + "," + deviceQuat.z + "," + deviceQuat.w;
            tpo.SetState("accel", deviceStr);
            Debug.Log("SEND: " + deviceStr);

            if (Input.touchCount > 0 && !this.networkForceEnabled) {
                tpo.SetState("force", "1");
                this.networkForceEnabled = true;
            } else if (Input.touchCount == 0 && this.networkForceEnabled) {
                tpo.SetState("force", "0");
                this.networkForceEnabled = false;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    // from https://docs.unity3d.com/ScriptReference/Gyroscope.html //
    private Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    ///// MODEL ANIMATION /////

    private void OnForce(string newValue) {
        this.forceComponent.enabled = newValue == "1";
    }

    public void TransformObject(string transformStr) {
        string[] arr = transformStr.Split(',');
        float rotX = float.Parse(arr[0]);
        float rotY = float.Parse(arr[1]);
        float rotZ = float.Parse(arr[2]);
        float rotW = float.Parse(arr[3]);

        this.TargetRot = new Quaternion(-rotX, rotZ, rotY, rotW);
        this.TargetRot = this.GyroToUnity(this.TargetRot);
    }

    private IEnumerator SmoothTransform() {
        while (true) {

            float t = 0f;
            while (t < this.SmoothingTime) {
                this.transform.rotation = Quaternion.Slerp(
                    this.transform.rotation,
                    this.TargetRot,
                    t / this.SmoothingTime
                );
                // Debug.Log(t);
                // Debug.Log(this.transform.eulerAngles);
                t += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }

        }
    }

    private void ApplyContinuousForce() {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.AddForce(this.transform.forward, ForceMode.Force);
    }

}
