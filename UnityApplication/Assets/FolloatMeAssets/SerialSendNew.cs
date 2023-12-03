/* 別スレッドでパルス幅を計算し、シリアル通信で送信する */
/* 注意：ここでいうパルス幅とは、立ち上がり時間（立ち下がり時間）のことを指している */


using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using System.Diagnostics;
using UnityEngine.Profiling;
using System;
using UnityEngine.UI;

public class SerialSendNew : MonoBehaviour
{
    // シリアル通信関係
    public SerialHandler serialHandler; // シリアル通信の核

    // オブジェクト関連のフィールド
    public OptitrackRigidBody target; // トラッキング座標を有するオブジェクト

    public Record _record;

    // パルス幅の最大・最小
    public int MAX_PULSEWIDTH;
    public int MIN_PULSEWIDTH;

    // 座標系統
    public Vector3 TrackedPosition; // 取得座標
    float x_i = 0f;
    float x_imin1 = 0f; // 前のフレームでのトラッキングx座標
    float delta_x_i; // x座標の差分
    int pulse_width; // パルス幅
    int w_i, w_imin1, delta_w_i, delta_w_i_inelse;
    public int n, i;

    // フラグ関係
    bool Flag_loop = true; // 別スレッドを実行し続けるか否か
    bool IsFirstExecution = true; // これが一番最初の実行であるか否か
    public bool IsSendStop; // シリアル通信で送るのをストップしているか否か
    bool PulseWidthWasSent = false;
    bool Accelerated = false;
    bool IsRecording = false; // パルス幅をファイルに記録しているか否か
    bool TrackingDone = false;
    bool PulseWidthWasCalculated = false;
    public bool MousePrototyping;

    public bool Exception = false;


    int RecordCount = 0; // 記録ファイル数

    float ms_per_flame_i = 0; // 実行開始からの時刻
    float ms_per_flame_imin1 = 0; // ms_per_flameの1フレーム前
    float delta_ms_per_flame_i = 0; // ms_per_flameの前フレームとの差分

    float delta_t;

    public float a; // パルス幅計算式の係数

    public Text PulseWidthText;

    // タイマー
    Stopwatch stopWatch;

    SynchronizationContext context;


    public int ActuatorStep;


    void Start()
    {
        Thread_1();
    }

    void OnApplicationQuit()//アプリ終了時の処理（無限ループを解放）
    {
        Flag_loop = false;//無限ループフラグを下げる
    }


    long TimeInOneFrameBefore = 0;
    long TimeInCurrentFrame = 0;
    long FrameInterval = 0;
    void Thread_1()
    {
        context = SynchronizationContext.Current;
        stopWatch = new Stopwatch();
        stopWatch.Start();

        Task.Run(() =>
        {
            while (Flag_loop)//無限ループフラグをチェック
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
        TimeInCurrentFrame = stopWatch.ElapsedMilliseconds;
        FrameInterval = TimeInCurrentFrame - TimeInOneFrameBefore;
        // UnityEngine.Debug.Log("FrameInterval = "+FrameInterval);
        if (FrameInterval < 1) return;
        TimeInOneFrameBefore = TimeInCurrentFrame;
        
        if (Exception) return;

        if (IsSendStop) {
            IsFirstExecution = true;
            pulse_width = MAX_PULSEWIDTH;
            return;
        }

        if (IsFirstExecution) FirstExecution();

        MeasureTime();

        if (TrackingDone) 
        {
            iequalszero();
            return;
        }
        else acceleration();

        // シリアル通信で渡す
        // これでは、iequalszeroで計算されたパルスを渡した後、加減速でいったんもとのパルス幅に戻り、もう一度計算されたパルス幅に戻るようになってしまう。
        // ここを直す。
        serialHandler.Write(pulse_width.ToString()+"\n");
        // serialHandler.Write("10000\n");
        // UnityEngine.Debug.Log("PulseWidth = "+pulse_width);
        PulseWidthWasSent = true;
        _record.pulsewidth_list.Add(pulse_width);
        //PulseWidthText.text = pulse_width.ToString();
        // PulseWidthText.text = ActuatorStep.ToString();
    }



    void iequalszero()
    {

        // トラッキングされた座標、時間を取得
        delta_t = target.tracking_interval;
        TrackedPosition = target.rbStatePosition;


        //
        // トラッキングし始めて一番最初の実行の時
        if (IsFirstExecution) {
            IsFirstExecution = false;
            x_i = x_imin1 = -TrackedPosition.z;
            pulse_width = MAX_PULSEWIDTH;
            w_i = w_imin1 = pulse_width;
            return;
        }
        //


        x_imin1 = x_i;
        x_i = -TrackedPosition.z;

        // ---- パルス幅の計算 ----
        // 簡略化した式（２変数関数）で計算
        w_imin1 = pulse_width;
        delta_x_i = x_i - x_imin1;
        delta_ms_per_flame_i = ms_per_flame_i - ms_per_flame_imin1;
        pulse_width = (int)((a*delta_t) / (1000f*delta_x_i));

        if (Math.Abs(delta_x_i) <= 0.0001f) pulse_width = MAX_PULSEWIDTH;
        CalculationException();


        w_i = pulse_width;
        delta_w_i = w_i - w_imin1;

        UnityEngine.Debug.Log("IEqualsZero:pulse_width = "+pulse_width+", delta_t = "+delta_t+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1+", tracking = "+TrackingDone);

        TrackingDone = false;
        i = 0;
    }


    int BlankCount = 0;
    void acceleration()
    {    
        if (i <= n)
        {
            if (delta_w_i == 0) pulse_width = w_i;
            else if (w_imin1*w_i > 0)
            {
                pulse_width = (int)(w_imin1 + (float)(delta_w_i*i)/ (float)n);
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
                delta_w_i = w_i - w_imin1;
                pulse_width = (int)(w_imin1 + (float)(delta_w_i*i)/ (float)n);
            }
        }
        else
        {
            pulse_width = w_i;
        }
        ++i;

        CalculationException();
        UnityEngine.Debug.Log("Acceleration:pulse_width = "+pulse_width+", delta_ms_per_flame = "+delta_ms_per_flame_i+", w_imin1 = "+w_imin1+", w_i = "+w_i+", delta_t = "+delta_t+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1+", tracking = "+TrackingDone);
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


    void MeasureTime()
    {
        if (stopWatch.IsRunning) {
            stopWatch.Stop();
            ms_per_flame_imin1 = ms_per_flame_i;
            ms_per_flame_i = (float)stopWatch.ElapsedMilliseconds;
        }
        stopWatch.Start();
    }

    void FirstExecution()
    {
        IsFirstExecution = false;
        pulse_width = w_i = w_imin1 = MAX_PULSEWIDTH;
        
        WaitForStabilization();
    }
}