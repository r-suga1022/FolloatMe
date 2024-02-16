/* 別スレッドでパルス幅を計算し、シリアル通信で送信する */


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;


public class SerialSendNew : MonoBehaviour
{
    // オブジェクト関係
    // シリアル通信関係
    // SynchronizationContextを使っていたこともあった
    public SerialHandler _serialHandler;
    public OptitrackRigidBody _target;
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


    public void CalculatePulseWidth() {
        // 前のパルス幅送信から1ms以上経ってから次のパルス幅を送る
        TimeInOneFrameBefore = TimeInCurrentFrame;
        TimeInCurrentFrame = stopWatch.ElapsedMilliseconds;
        FrameInterval = TimeInCurrentFrame - TimeInOneFrameBefore;
        
        if (FrameInterval < 1) return;
        if (SendStop) {
            FirstExecution = true;
            pulse_width = MAX_PULSEWIDTH;
            return;
        }

        // パルス幅計算
        if (TrackingDone)
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

        // パルス幅送信
        _serialHandler.Write(pulse_width.ToString()+"\n");
        PulseWidthWasSent = true;
        // _record.pulsewidth_list.Add(pulse_width);
    }



    void iequalszero()
    {
        // トラッキングされた座標、時間を取得
        delta_t = _target.tracking_interval;
        TrackedPosition = _target.rbStatePosition;

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

        //UnityEngine.Debug.Log("IEqualsZero:pulse_width = "+pulse_width+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);

        TrackingDone = false;
        i = 0;
    }


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
                }
                else if (w_i == MAX_PULSEWIDTH)
                {
                    w_i = -MAX_PULSEWIDTH;
                }
                delta_w = w_i - w_imin1;
                pulse_width = (int)(w_imin1 + (float)(delta_w*i)/ (float)n);
            }
        }
        else
        {
            pulse_width = w_i;
        }

        ++i;

        CalculationException();
        //UnityEngine.Debug.Log("Acceleration:pulse_width = "+pulse_width+", w_imin1 = "+w_imin1+", w_i = "+w_i+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);
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