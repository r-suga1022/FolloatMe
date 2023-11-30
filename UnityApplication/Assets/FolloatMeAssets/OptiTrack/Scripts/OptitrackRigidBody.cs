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
    Vector3 tracked_position;
    bool Recording = false;


    bool cantracking = false;

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
        UpdatePose();
    }
#endif


    void FixedUpdate()
    //void Update()
    {
        // UnityEngine.Debug.Log(cantracking);
        UpdatePose();

        _serialsend.SetWasTrackingDone(true);

        tracking_time_n_1 = tracking_time_n;
        TimeSpan elapsed = stopWatch.Elapsed;
        tracking_time_n = elapsed.Seconds + elapsed.Milliseconds / 1000f;
        tracking_interval = tracking_time_n - tracking_time_n_1;
        // UnityEngine.Debug.Log("Measurement: Interval = "+tracking_interval.ToString());

        _record.tracking_interval_list.Add( tracking_interval );
        _record.tracked_position_list.Add( tracked_position.z );

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Recording)
            {
                _record.LogSave(_record.tracking_interval_list, "tracking_interval2", false);    
                _record.LogSave(_record.tracked_position_list, "tracked_position2", false);
            }
            Recording = !Recording;
        }
    }

    public UnityEvent EventMethod;


    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState( RigidBodyId, NetworkCompensation);
        if ( rbState != null )
        {
            tracked_position = rbState.Pose.Position;
            this.transform.localPosition = rbState.Pose.Position;
            this.transform.localRotation = rbState.Pose.Orientation;
        }

        EventMethod?.Invoke();
    }
}
