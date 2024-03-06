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
    public CharacterOperation _character;
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

    public bool MousePrototyping;
    Vector3 BeforeTrackedPosition;


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



    /*
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
    */

    // 毎回のloopで実行
    public void CalculatePulseWidth() {
        // 複数のRigidBodyがあることを想定
        _target = _targetlist_in_serialsend[CurrentTargetNumber];

        // 前のパルス幅送信から1ms以上経ってから次のパルス幅を送る
        TimeInOneFrameBefore = TimeInCurrentFrame;
        TimeInCurrentFrame = stopWatch.ElapsedMilliseconds;
        FrameInterval = TimeInCurrentFrame - TimeInOneFrameBefore;
        if (FrameInterval < 1) return;


        // pulse_width_oneframebefore = pulse_width;


        // トラッキングされた座標、時間を取得
        if (!MousePrototyping)
        {
            delta_t = _target.tracking_interval;
            TrackedPosition = _target.rbStatePosition;
        }
        else
        {
            BeforeTrackedPosition = TrackedPosition;
            delta_t = 1000f/120f;
            //UnityEngine.Debug.Log("TrackingDone = "+TrackingDone);
            TrackedPosition = _character.xvec_i;
            TrackedPosition.z = TrackedPosition.x*0.0001f;
            TrackingDone = (TrackedPosition.x != BeforeTrackedPosition.x);
        }

        // アクチュエータの端に到達したときの処理
        WhenReachedToEdge();

        // もしSendStopになったら、減速してから止まりたい
        WhenSendStopChanged();
        
        // パルス幅計算
        if (TrackingDone && !VelocityZero)
        {
            // トラッキング結果に基づいてパルス幅計算
            iequalszero();
            pulse_width = w_imin1;
        }
        // 加減速処理
        else acceleration();        

        // パルス幅送信
        _serialHandler.Write(pulse_width.ToString()+"\n");
        PulseWidthWasSent = true;
        UnityEngine.Debug.Log("PulseWidth = "+pulse_width+", n = "+n+", SendStop = "+SendStop+", OnSending = "+OnSending);
    }


    // トラッキング結果に基づいてパルス幅計算
    void iequalszero()
    {
        // トラッキングされた座標、時間を取得
        //delta_t = _target.tracking_interval;
        //TrackedPosition = _target.rbStatePosition;

        // トラッキングし始めて一番最初の実行の時
        if (FirstExecution) {
            // トラッキング座標が初期値のままだったらまだ計算しない
            // （いきなり最高速度で動き出すことを防ぐため）
            UnityEngine.Debug.Log("FirstExecution");
            if (TrackedPosition.z == 0) return;
            FirstExecution = false;
            z_i = -TrackedPosition.z;
            z_imin1 = -TrackedPosition.z;
            if (!DeccelerationToAcceleration) pulse_width = MAX_PULSEWIDTH;
            DeccelerationToAcceleration = false;
            w_i = pulse_width;
            w_imin1 = pulse_width;
            return;
        }

        if (SendStopAccelerating) return;

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

        UnityEngine.Debug.Log("IEqualsZero:pulse_width = "+pulse_width+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);

        TrackingDone = false;
        i = 0;
    }


    float delta_w2;
    // 加減速処理
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
        UnityEngine.Debug.Log("Acceleration:pulse_width = "+pulse_width+", w_imin1 = "+w_imin1+", w_i = "+w_i+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);
    }

    public void SetWasTrackingDone(bool flag)
    {
        TrackingDone = flag;
    }

    void CalculationException()
    {
        if (Math.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
            if (pulse_width >= 0 ) pulse_width = MIN_PULSEWIDTH;
            else pulse_width = -MIN_PULSEWIDTH;
        }
    }


    bool OnSending = false;
    bool SendStopDeccelerating = false;
    bool SendStopAccelerating = false;
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
    bool DeccelerationToAcceleration = false;

    // アクチュエータが端に到達した時の処理
    void WhenReachedToEdge()
    {
        if ((ActuatorStep != BeforeActuatorStep) && (ActuatorStep < (StepAtTopPosition - WithinStepAtDeccelerationStart)) || ((ActuatorStep > (StepAtBottomPosition + WithinStepAtDeccelerationStart))))
        {
            Dassyutsushita = false;
        }
        // 最高地点の範囲に到達したら
        if (!SendStop && !Dassyutsushita && !ReachedToTop && (ActuatorStep >= (StepAtTopPosition - WithinStepAtDeccelerationStart)))
        {
            ReachedToTop = true;
            SendStop = true;
            ZWhenReachedToTop = TrackedPosition.z;
        }
        // 最低地点の範囲に到達したら
        else if (!SendStop && !Dassyutsushita && !ReachedToBottom && (ActuatorStep <= (StepAtBottomPosition + WithinStepAtDeccelerationStart)))
        {
            ReachedToBottom = true;
            SendStop = true;
            ZWhenReachedToBottom = TrackedPosition.z;
            UnityEngine.Debug.Log("bottom at z = "+ZWhenReachedToBottom);
        }
        // アクチュエータの端から脱したときの処理
        // 最高地点から
        else if (ReachedToTop && (TrackedPosition.z > ZWhenReachedToTop))
        {
            // if (pulse_width == MAX_PULSEWIDTH)
            // もともとこのifの中に入れていたが、それをやめた
            SendStop = false;
            Dassyutsushita = true;
            ReachedToTop = false;
            BeforeActuatorStep = ActuatorStep;
        // 最低地点から
        }
        else if (ReachedToBottom && (TrackedPosition.z < ZWhenReachedToBottom))
        {
            // if (pulse_width == MAX_PULSEWIDTH)
            // もともとこのifの中に入れていたが、それをやめた
            UnityEngine.Debug.Log("exe2");
            SendStop = false;
            Dassyutsushita = true;
            ReachedToBottom = false;
            BeforeActuatorStep = ActuatorStep;
        }
    }


    // もしSendStopになったら、減速してから止まりたい
    void WhenSendStopChanged()
    {
        // 動いている && 送っていない（動き始めたばかりのとき）
        if (!SendStop && !OnSending) {
            // 端から脱出した時の加速（問題があったら消す）
            //
            if (!SendStopAccelerating && Dassyutsushita)
            // if (!SendStopAccelerating)
            {
                AKeyOn = true;
                SendStopAccelerating = true;
                if (pulse_width == MAX_PULSEWIDTH)
                {
                    // 一応符号を確認
                    // if (TrackedPosition.z > ZWhenReachedToTop) pulse_width = -1000;
                    if (w_i < 0) pulse_width = -1000;
                    else pulse_width = 1000;
                }
                w_imin1 = w_i = pulse_width;
                n = n_decceleration;
                FirstExecution = true;
                i = 0;
            // 端から脱出した場合ではないとき
            } else if (!SendStopAccelerating) {
                UnityEngine.Debug.Log("WhenAcceleration");
                SendStopAccelerating = true;
                // OnSending = true;
                delta_z = 0f;
                FirstExecution = true;
                // n = n_default;
                n = n_decceleration;
                i = 0;
                // pulse_width = MAX_PULSEWIDTH; // これでは減速中の時対処できない
                w_imin1 = w_i = pulse_width;
            }
            // if (i == n-1 && Dassyutsushita)
            if (i == n-1)
            {
                SendStopAccelerating = false;
                OnSending = true;
                delta_z = 0f;
                // FirstExecution = true;
                n = n_default;
                i = 0;
                // pulse_width = MAX_PULSEWIDTH; // これでは減速中の時対処できない
                w_imin1 = w_i = pulse_width;
            }

        
        // 動いていない && 送っている（止まるときの減速中）
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
                i = 0;
                return;
            }
            if (!SendStopDeccelerating)
            {
                w_imin1 = pulse_width;
                // w_iを変更
                // w_i = MAX_PULSEWIDTH;
                if (pulse_width > 0) w_i = 1000;
                else w_i = -1000;
                delta_w = w_i - w_imin1;
                i = 0;
                n = n_decceleration;
            }
            if (Mathf.Abs((float)pulse_width) >= 1000f)
            {
                pulse_width = MAX_PULSEWIDTH;
                UnityEngine.Debug.Log("aaaa");
                i = 0;
                w_imin1 = w_i = MAX_PULSEWIDTH;
                n = n_default;
            }
            //UnityEngine.Debug.Log("Deccelerating with others, sendstopdeccelerating = "+SendStopDeccelerating);
            SendStopDeccelerating = true;
            TrackingDone = false;
            // return;

        
        // 動いていない && 送っていない
        } else if (SendStop && !OnSending) {
            pulse_width = MAX_PULSEWIDTH;
            i = 0;
            return;

        
        // 動いている && 送っている
        } else {
            // SendStopで減速中にSendStopが切り替わり、加速に転じたとき
            if (SendStopDeccelerating)
            {
                // UnityEngine.Debug.Log("減速から加速に転じた");
                DeccelerationToAcceleration = true;
                FirstExecution = true;
                SendStopDeccelerating = false;
            }
            SendStopDeccelerating = false;
            SendStopAccelerating = false;
            n = n_default;
        }
    }


    // 最高速度を超えたときの処理
    void WhenExceededMaxVelocity()
    {
        // ひょっとしたらこれが最高速度を超えたときに間に合わせる処理につながるか
        if (pulse_width == MIN_PULSEWIDTH && pulse_width_oneframebefore == MIN_PULSEWIDTH);
        {
            ++FrameCountWhileMaxVelocity;
        } 
        //else if (pulse_width != MIN_PULSEWIDTH && pulse_width_oneframebefore == MIN_PULSEWIDTH)
        //{
            
        //}
    }
}