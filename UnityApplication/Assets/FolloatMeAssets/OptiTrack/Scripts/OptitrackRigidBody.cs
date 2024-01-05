/* 
Copyright © 2016 NaturalPoint Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. 
*/

using System;
using UnityEngine;
using System.Diagnostics;

using UnityEngine.Events;

using System.Threading;
using System.Threading.Tasks;


/// <summary>
/// Implements live tracking of streamed OptiTrack rigid body data onto an object.
/// </summary>
public class OptitrackRigidBody : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public Int32 RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]


    public bool NetworkCompensation = true;
    public Record _record;
    public SerialSendNew _serialsend;

    Stopwatch stopWatch;
    float tracking_time_n, tracking_time_n_1;
    public float tracking_interval;
    bool Recording = false;


    bool cantracking = false;
    
    public bool PositionChanged = false;

    public bool TrackingDone = false;

    public bool PositionChangePositive = false;

    bool LoopFlag = true;

    public bool MousePrototyping;

    public bool LatencyMeasuring;

    public UnityEvent EventMethod;

    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if ( this.StreamingClient == null )
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if ( this.StreamingClient == null )
            {
                UnityEngine.Debug.LogError( GetType().FullName + ": Streaming client not set, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                this.enabled = false;
                return;
            }
        }

        this.StreamingClient.RegisterRigidBody( this, RigidBodyId );

        stopWatch = new Stopwatch();
        stopWatch.Start();
        tracking_time_n_1 = 0f;


        //Thread_1();
    }



#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        //UpdatePose();
    }

    void OnApplicationQuit()
    {
        LoopFlag = false;
    }
#endif


    //void FixedUpdate()
    
    void Update()
    {
        UpdatePose();

        _serialsend.SetWasTrackingDone(TrackingDone);
    }
    


    public Vector3 rbStatePosition;
    public Quaternion rbStateOrientation;
    public Vector3 BeforePosition = Vector3.zero; // 変えるかもしれない

    public float PositionChangeThreshold;
    void UpdatePose()
    {
        BeforePosition = rbStatePosition;
        if (MousePrototyping)
        {
            rbStatePosition = Input.mousePosition;
            rbStatePosition.z = rbStatePosition.x;
        }
        else
        {
            OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState( RigidBodyId, NetworkCompensation);
            rbStatePosition = rbState.Pose.Position;
            rbStateOrientation = rbState.Pose.Orientation;
        }

        // OptiTrackのノイズ対策（座標の変化量がある閾値以上であれば、動いたとみなす）
        bool XChanged = Mathf.Abs(BeforePosition.x - rbStatePosition.x) > PositionChangeThreshold;
        bool YChanged = Mathf.Abs(BeforePosition.y - rbStatePosition.y) > PositionChangeThreshold;
        bool ZChanged = Mathf.Abs(BeforePosition.z - rbStatePosition.z) > PositionChangeThreshold;
        bool XTrackingDone = (BeforePosition.x != rbStatePosition.x);
        bool YTrackingDone = (BeforePosition.y != rbStatePosition.y);
        bool ZTrackingDone = (BeforePosition.z != rbStatePosition.z);
        bool XChangePositive = (rbStatePosition.x - BeforePosition.x) > PositionChangeThreshold;
        bool ZChangePositive = (rbStatePosition.z - BeforePosition.z) > PositionChangeThreshold;
        PositionChanged = XChanged || YChanged || ZChanged;
        TrackingDone = XTrackingDone || YTrackingDone || ZTrackingDone;
        PositionChangePositive = XChangePositive || ZChangePositive;
        //PositionChanged = ZChanged;
        //UnityEngine.Debug.Log("before = "+BeforePosition.z+", rbstate = "+rbStatePosition.z);

        // if ( rbState != null && PositionChanged)
        if (PositionChanged)
        {
            tracking_time_n_1 = tracking_time_n;
            tracking_time_n = stopWatch.ElapsedMilliseconds;
            //tracking_interval = tracking_time_n - tracking_time_n_1;
            tracking_interval = 1f/120f*1000f;
        }

        // UnityEngine.Debug.Log("Rigidbody:tracking_interval = "+tracking_interval+", position changed = "+PositionChanged+", before.z = "+BeforePosition.z+", after.z = "+rbStatePosition.z);
        //EventMethod?.Invoke();
    }
}
