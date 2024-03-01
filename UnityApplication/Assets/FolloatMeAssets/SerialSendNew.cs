/* 別スレッドでパルス幅を計算し、シリアル通信で送信する */


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;


public class SerialSendNew : MonoBehaviour
{
    // オブジェクト関係
    // シリアル通信関係
    // SynchronizationContextを使っていたこともあった
    public SerialHandler _serialHandler;
    public List<OptitrackRigidBody> _targetlist_in_serialsend= new List<OptitrackRigidBody>();
    public OptitrackRigidBody _target;
    public int CurrentTargetNumber;
    public Record _record;
    Stopwatch stopWatch;

    // 座標系統
    public Vector3 TrackedPosition; // 取得座標
    float z_i = 0f; // 現在のフレームでのトラッキングz座標
    float z_imin1 = 0f; // 前のフレームでのトラッキングz座標
    float delta_z; // z座標の差分
    int pulse_width; // パルス幅
    int w_i, w_imin1, delta_w;
    public int n, i;
    public int ActuatorStep; // アクチュエータの現在位置（ステップ数）

    // 数値関係
    public int MAX_PULSEWIDTH; // パルス幅の最大
    public int MIN_PULSEWIDTH; // パルス幅の最小
    public float a; // パルス幅計算式の係数
    public float a_positive, a_negative;
    float delta_t; // トラッキングの時間間隔
    public float MoveThreshold;
    long TimeInOneFrameBefore = 0;
    long TimeInCurrentFrame = 0;
    long FrameInterval = 0;

    // フラグ関係
    bool ThreadExecution = true; // 別スレッドを実行し続けるか否か
    bool FirstExecution = true; // これが一番最初の実行であるか否か
    public bool SendStop; // シリアル通信で送るのをストップしているか否か
    bool TrackingDone = false; // トラッキングが完了したか否か
    bool PulseWidthWasSent = false; // パルス幅が送信されたか否か
    public bool Exception = false; // 例外があるか否か


    void Start()
    {
        Thread_1();
    }

    void OnApplicationQuit()//アプリ終了時の処理（無限ループを解放）
    {
        ThreadExecution = false;//無限ループフラグを下げる
    }

    void Thread_1()
    {
        stopWatch = new Stopwatch();
        stopWatch.Start();

        Task.Run(() =>
        {
            while (ThreadExecution)//無限ループフラグをチェック
            {
                try
                {
                    CalculatePulseWidth();
                    // UnityEngine.Debug.Log("thread ID = "+Thread.CurrentThread.ManagedThreadId.ToString());
                }
                catch (System.Exception e)//例外をチェック
                {
                    UnityEngine.Debug.LogWarning(e);//エラーを表示
                }
            }
        });
    }

    bool OnSending = false;
    bool SendStopDeccelerating = false;
    public int StepAtTopPosition, StepAtBottomPosition; // 最高地点と最低地点のステップ数
    public int WithinStepAtDeccelerationStart; // 現在位置が最高・最低地点からどれくらいの範囲に入ったら減速をはじめるか
    float ZWhenReachedToTop; // 最高地点からwithinの範囲に入ったときのZ座標
    float ZWhenReachedToBottom; // 最低地点からwithinの範囲に入ったときのZ座標
    public bool VelocityZero = false;
    bool TopOrBottomOperationDone = false;
    int FrameCountWhileMaxVelocity = 0;
    int pulse_width_oneframebefore;
    bool ReachedToTop = false;
    bool ReachedToBottom = false;
    public bool AKeyOn;

    bool Dassyutsushita = false;

    public int n_default;
    public int n_decceleration;

    public bool DebugOn = false;
    
    int BeforeActuatorStep;

    public void CalculatePulseWidth() {
        // 複数のRigidBodyがあることを想定
        _target = _targetlist_in_serialsend[CurrentTargetNumber];
        // 前のパルス幅送信から1ms以上経ってから次のパルス幅を送る
        TimeInOneFrameBefore = TimeInCurrentFrame;
        TimeInCurrentFrame = stopWatch.ElapsedMilliseconds;
        FrameInterval = TimeInCurrentFrame - TimeInOneFrameBefore;

        pulse_width_oneframebefore = pulse_width;

        // トラッキングされた座標、時間を取得
        delta_t = _target.tracking_interval;
        TrackedPosition = _target.rbStatePosition;
        
        if (FrameInterval < 1) return;

        // アクチュエータの端に到達したときの処理
        // 最高地点
        //
        if ((ActuatorStep != BeforeActuatorStep) && (ActuatorStep < (StepAtTopPosition - WithinStepAtDeccelerationStart)) || ((ActuatorStep > (StepAtBottomPosition + WithinStepAtDeccelerationStart))))
        {
            Dassyutsushita = false;
            //UnityEngine.Debug.Log("ToFalse");
        }
        //
        if (!SendStop && !Dassyutsushita && !ReachedToTop && (ActuatorStep >= (StepAtTopPosition - WithinStepAtDeccelerationStart)))
        //if (!SendStop && TrackedPosition.z <= ZWhenReachedToTop)
        {
            ReachedToTop = true;
            SendStop = true;
            ZWhenReachedToTop = TrackedPosition.z;
        } 
        else if (!SendStop && !Dassyutsushita && !ReachedToBottom && (ActuatorStep <= (StepAtBottomPosition + WithinStepAtDeccelerationStart)))
        //} else if (!SendStop && TrackedPosition.z >= ZWhenReachedToBottom)
        {
            ReachedToBottom = true;
            SendStop = true;
            ZWhenReachedToBottom = TrackedPosition.z;
            UnityEngine.Debug.Log("bottom at z = "+ZWhenReachedToBottom);
        }
        // アクチュエータの端から脱したときの処理
        //else if (ReachedToTop && ActuatorStep < (StepAtTopPosition - WithinStepAtDeccelerationStart))
        else if (ReachedToTop && (TrackedPosition.z > ZWhenReachedToTop))
        {
            if (pulse_width == MAX_PULSEWIDTH)
            {
                SendStop = false;
                Dassyutsushita = true;
                ReachedToTop = false;
                BeforeActuatorStep = ActuatorStep;
            }
        // 最低地点
        //} else if (ReachedToBottom && ActuatorStep < (StepAtBottomPosition - WithinStepAtDeccelerationStart))
        } else if (ReachedToBottom && (TrackedPosition.z < ZWhenReachedToBottom))
        {
            if (pulse_width == MAX_PULSEWIDTH)
            {
                UnityEngine.Debug.Log("exe2");
                SendStop = false;
                Dassyutsushita = true;
                ReachedToBottom = false;
                BeforeActuatorStep = ActuatorStep;
            }
            //ReachedToBottom = false;
        }
        //if (!SendStop && ((ActuatorStep < (StepAtTopPosition - WithinStepAtDeccelerationStart)) || (ActuatorStep > (StepAtBottomPosition + WithinStepAtDeccelerationStart))))
        /*
        if ( SendStop && ( (TrackedPosition.z > ZWhenReachedToTop) || (TrackedPosition.z < ZWhenReachedToBottom) ) )
        {
            ReachedToTop = false;
            ReachedToBottom = false;
            Dassyutsushita = true;
        }
        */
        
        //if (ReachedToBottom) UnityEngine.Debug.Log("Bottom z = "+TrackedPosition.z);

        // もしSendStopになったら、減速してから止まりたい
        if (!SendStop && !OnSending) {
            OnSending = true;
            delta_z = 0f;
            FirstExecution = true;
            n = n_default;
            pulse_width = MAX_PULSEWIDTH;
            UnityEngine.Debug.Log("n = "+n);
            //_serialHandler.Write("s"+ActuatorStep.ToString()+"\n");
            //return;
        } else if (SendStop && OnSending) {
            // 端に到達した場合ではないとき
            if (!ReachedToBottom && !ReachedToTop) AKeyOn = false;
            if (pulse_width == MAX_PULSEWIDTH)
            {
                //if (ReachedToBottom || ReachedToTop)
                    //UnityEngine.(Debug.Log("Deccelerating with top or bottom");
                //else
                    //UnityEngine.Debug.Log("Deccelerating with others");
                SendStopDeccelerating = false;
                OnSending = false;
                w_imin1 = w_i = MAX_PULSEWIDTH;
                delta_w = 0;
                return;
            }
        // if (OnSending) {
            if (!SendStopDeccelerating)
            {
                //UnityEngine.Debug.Log("execute if sendstopdeccelerating");
                //FirstExecution = true;
                w_imin1 = pulse_width;
                w_i = MAX_PULSEWIDTH;
                delta_w = w_i - w_imin1;
                i = 0;
                n = n_decceleration;
                // pulse_width = MAX_PULSEWIDTH;
            }
            //UnityEngine.Debug.Log("Deccelerating with others, sendstopdeccelerating = "+SendStopDeccelerating);
            SendStopDeccelerating = true;
            TrackingDone = false;
            // return;
        } else if (SendStop && !OnSending) {
            pulse_width = MAX_PULSEWIDTH;
            return;
        } else {
            SendStopDeccelerating = false;
        }

        // パルス幅計算
        if (TrackingDone && !VelocityZero)
        {
            iequalszero();
            pulse_width = w_imin1;
            //return;
        }
        else acceleration();
        if (_target.LatencyMeasuring)
        {
            //pulse_width = MAX_PULSEWIDTH;
        }

        /*
        if (pulse_width == MIN_PULSEWIDTH && pulse_width_oneframebefore == MIN_PULSEWIDTH);
        {
            ++FrameCountWhileMaxVelocity;
        } else if (pulse_width != MIN_PULSEWIDTH && pulse_width_oneframebefore == MIN_PULSEWIDTH)
        {
            
        }
        */

        // パルス幅送信
        _serialHandler.Write(pulse_width.ToString()+"\n");
        PulseWidthWasSent = true;
        // _record.pulsewidth_list.Add(pulse_width);
    }



    void iequalszero()
    {
        // トラッキングされた座標、時間を取得
        //delta_t = _target.tracking_interval;
        //TrackedPosition = _target.rbStatePosition;

        // トラッキングし始めて一番最初の実行の時
        if (FirstExecution) {
            // トラッキング座標が初期値のままだったらまだ計算しない
            // （いきなり最高速度で動き出すことを防ぐため）
            if (TrackedPosition.z == 0) return;
            FirstExecution = false;
            z_i = -TrackedPosition.z;
            z_imin1 = -TrackedPosition.z;
            pulse_width = MAX_PULSEWIDTH;
            w_i = pulse_width;
            w_imin1 = pulse_width;
            return;
        }

        z_imin1 = z_i;
        z_i = -TrackedPosition.z;

        // ---- パルス幅の計算 ----
        // 簡略化した式（２変数関数）で計算
        w_imin1 = pulse_width;
        delta_z = z_i - z_imin1;
        if (delta_z < 0) a = a_negative;
        else a = a_positive;
        pulse_width = (int)((a*delta_t) / (1000f*delta_z));
        if (Math.Abs(delta_z) <= MoveThreshold) pulse_width = MAX_PULSEWIDTH;
        CalculationException();

        w_i = pulse_width;
        delta_w = w_i - w_imin1;

        //if (DebugOn)
            //UnityEngine.Debug.Log("IEqualsZero:pulse_width = "+pulse_width+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);

        TrackingDone = false;
        i = 0;
    }


    float delta_w2;
    void acceleration()
    {    
        if (i <= n)
        {
            if (delta_w == 0) pulse_width = w_i;
            else if (w_imin1*w_i > 0)
            {
                pulse_width = (int)(w_imin1 + (float)(delta_w*i)/ (float)n);
            }
            else if (w_imin1*w_i < 0)
            {
                if (w_imin1 == MAX_PULSEWIDTH)
                {
                    w_imin1 = -MAX_PULSEWIDTH;
                    delta_w = w_i - w_imin1;
                    pulse_width = (int)(w_imin1 + (float)(delta_w*i)/ (float)(n));
                }
                else if (w_i == MAX_PULSEWIDTH)
                {
                    w_i = -MAX_PULSEWIDTH;
                    delta_w = w_i - w_imin1;
                    pulse_width = (int)(w_imin1 + (float)(delta_w*i)/ (float)(n));
                }
                else
                {
                    //UnityEngine.Debug.Log("negative");
                    if (i <= n/2) {
                        delta_w2 = (w_imin1 > 0) ? MAX_PULSEWIDTH - w_imin1 : -MAX_PULSEWIDTH - w_imin1;
                        pulse_width = (int)(w_imin1 + (delta_w2*i) / (int)(n/2));
                    } else {
                        delta_w2 = (w_i > 0) ? MAX_PULSEWIDTH - w_i : -MAX_PULSEWIDTH - w_i;
                        pulse_width = (int)(w_i + (delta_w2*(n - i))/ (int)(n/2));
                    }
                }
            }
        }
        else
        {
            pulse_width = w_i;
        }

        ++i;
        CalculationException();
        //if (DebugOn)
            UnityEngine.Debug.Log("Acceleration:pulse_width = "+pulse_width+", w_imin1 = "+w_imin1+", w_i = "+w_i+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);
    }

    public void SetWasTrackingDone(bool flag)
    {
        TrackingDone = flag;
    }

    void WaitForStabilization()
    {
        Thread.Sleep(100);
    }


    void CalculationException()
    {
        if (Math.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
            if (pulse_width >= 0 ) pulse_width = MIN_PULSEWIDTH;
            else pulse_width = -MIN_PULSEWIDTH;
        }
    }
}