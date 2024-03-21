/* 別スレッドでパルス幅を計算し、シリアル通信で送信する */


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;


public class SerialSend : MonoBehaviour
{
    // オブジェクト関係
    // シリアル通信関係
    // SynchronizationContextを使っていたこともあった
    public SerialHandler _serialHandler;
    public List<OptitrackRigidBody> _targetlist_in_serialsend= new List<OptitrackRigidBody>();
    public OptitrackRigidBody _target;
    public int CurrentTargetNumber;
    Stopwatch stopWatch;

    // 座標系統
    public Vector3 TrackedPosition; // 取得座標
    float z_i = 0f; // 現在のフレームでのトラッキングz座標
    float z_imin1 = 0f; // 前のフレームでのトラッキングz座標
    float delta_z; // z座標の差分
    int pulse_width; // パルス幅
    int w_i, w_imin1, delta_w;
    public int n, i;
    public int n_default;
    public int n_decceleration;
    public int ActuatorStep; // アクチュエータの現在位置（ステップ数）
    public int TopStep; // アクチュエータの上端のステップ数
    public int BottomStep; // アクチュエータの下端のステップ数

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

        pulse_width = MAX_PULSEWIDTH;
        w_imin1 = w_i = pulse_width;

        Task.Run(() =>
        {
            while (ThreadExecution)//無限ループフラグをチェック
            {
                try
                {
                    // 1フレーム = 1ms
                    if (!PassedOneFrameBefore(1.0f)) continue;
                    FrameLoop();
                    //UnityEngine.Debug.Log("thread ID = "+Thread.CurrentThread.ManagedThreadId.ToString());
                }
                catch (System.Exception e)//例外をチェック
                {
                    UnityEngine.Debug.LogWarning(e);//エラーを表示
                }
            }
        });
    }

    // 1フレーム内で行う処理
    public void FrameLoop() {
        // 複数のRigidBodyがあることを想定
        _target = _targetlist_in_serialsend[CurrentTargetNumber];

        // トラッキングされた座標、時間間隔を取得
        delta_t = _target.tracking_interval;
        TrackedPosition = _target.rbStatePosition;

        // パルス幅計算
        CalculatePulseWidth();

        // 例外処理
        CalculationException();
        UnityEngine.Debug.Log("PulseWidth = "+pulse_width);

        // パルス幅送信
        _serialHandler.Write(pulse_width.ToString()+"\n");
    }





    // -------------------------------------
    // ---- FrameLoopで呼ばれる関数の定義 ----
    // -------------------------------------
    float delta_w2;
    void CalculatePulseWidth()
    {
        // ---- パルス幅の計算 ----
        // 簡略化した式（２変数関数）で計算
        //TrackingDone = _target.TrackingDone;
        if (TrackingDone && !ThereIsSomeException)
        {
            z_imin1 = z_i;
            z_i = -TrackedPosition.z;

            w_imin1 = pulse_width;
            delta_z = z_i - z_imin1;
            if (delta_z < 0) a = a_negative;
            else a = a_positive;
            pulse_width = (int)((a*delta_t) / (1000f*delta_z));
            if (Math.Abs(delta_z) <= MoveThreshold) pulse_width = MAX_PULSEWIDTH;

            // 各種値のセット
            w_i = pulse_width;
            delta_w = w_i - w_imin1;
            if (TrackingDone) i = 0;
            UnityEngine.Debug.Log("IEqualsZero:pulse_width = "+pulse_width+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone);
            TrackingDone = false;
        }

        // 加減速処理
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
        UnityEngine.Debug.Log("Acceleration:pulse_width = "+pulse_width+", w_imin1 = "+w_imin1+", w_i = "+w_i+", delta_t = "+delta_t+", delta_z = "+delta_z+", z_i = "+z_i+", z_imin1 = "+z_imin1+", tracking = "+TrackingDone+"i = "+i);
    }

    // --- 例外処理 ---
    bool ThereIsSomeException = false;
    void CalculationException()
    {
        // トラッキングし始めて一番最初の実行の時
        if (FirstExecution) {
            ExceptionInFirstExecution();
        }

        // RigidBodyが認識対象から外れた時など
        if (!_target.Active) {
            ExceptionInOutOfRecognition();
        }

        // アクチュエータの端に到達した時
        ExceptionInReachedEdge();

        // パルス幅の値に関する例外が発生した時
        ExceptionInPulseWidthValue();

        // 各種例外処理で使ったフラグを元に戻す処理
        TurnExceptionFlagsDefaultState();
    }



    // 最初の実行時はtargetが不安定のため、工夫が必要
    private int FirstTrackingDoneCount = 0;
    private void ExceptionInFirstExecution()
    {
        if (PulseWidthAbsoluteIsSmallerThan(MAX_PULSEWIDTH)) {
            pulse_width = MAX_PULSEWIDTH;
            w_i = w_imin1 = pulse_width;
            delta_w = 0;
            z_imin1 = z_i;
            delta_z = 0f;

            ++FirstTrackingDoneCount;
            if (FirstTrackingDoneCount >= 2) FirstExecution = false;
        }
    }


    // targetが認識対象から外れた時の例外処理
    // 速度ゼロに向かって減速するように
    private bool OnTheWayOfOutOfRecognitionExecution = false;
    private void ExceptionInOutOfRecognition()
    {
        if (OnTheWayOfOutOfRecognitionExecution) return;
        //w_imin1 = pulse_width;
        w_i = MAX_PULSEWIDTH;
        delta_w = w_i - w_imin1;
        z_imin1 = z_i;
        delta_z = 0f;
        i = 0;
        n = N_Decceleration();

        OnTheWayOfOutOfRecognitionExecution = true;
        UnityEngine.Debug.Log("Inactive, n = "+n);
    }

    // アクチュエータの端に到達したとき
    private int ReachedToEdgeFlag = 0; // トップなら1、ボトムなら2
    private float ZValueWhenReachedToEdge; // 端に到達したときのRigidBodyのz座標
    private void ExceptionInReachedEdge()
    {
        bool NowTop = ReachedToTop();
        bool NowBottom = ReachedToBottom();
        if (NowTop || NowBottom)
        {
            w_i = MAX_PULSEWIDTH;
            delta_w = w_i - w_imin1;
            z_imin1 = z_i;
            delta_z = 0f;
            i = 0;
            n = N_Decceleration();
            ZValueWhenReachedToEdge = z_i;

            if (NowTop) ReachedToEdgeFlag = 1;
            if (NowBottom) ReachedToEdgeFlag = 2;
            ThereIsSomeException = true;
        }
        return;
    }
    private int N_Decceleration() {
        float w = Mathf.Abs(pulse_width);
        n_decceleration = (int)(
            ( (MIN_PULSEWIDTH - MAX_PULSEWIDTH) / (MAX_PULSEWIDTH - MIN_PULSEWIDTH) )*(w - MAX_PULSEWIDTH) + MIN_PULSEWIDTH
        );
        return n_decceleration;
    }


    // パルス幅の値が最小値・最大値を超えたなどの例外が発生したとき
    private void ExceptionInPulseWidthValue()
    {
        if (Math.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
            if (pulse_width >= 0 ) pulse_width = MIN_PULSEWIDTH;
            else pulse_width = -MIN_PULSEWIDTH;
        }
    }

    // 例外処理に使ったフラグたちをデフォルトに戻す
    private void TurnExceptionFlagsDefaultState()
    {
        // RigidBodyが認識対象に戻ったとき
        if (TurnedToActive()) {
            if (!PulseWidthAbsoluteIsSmallerThan(MAX_PULSEWIDTH)) return;
            n = n_default;
            z_imin1 = z_i;
            w_imin1 = w_i = pulse_width = MAX_PULSEWIDTH;
            delta_w = 0;

            // ノイズに対処するため
            ++FirstTrackingDoneCount;
            if (FirstTrackingDoneCount >= 2) OnTheWayOfOutOfRecognitionExecution = false;

            UnityEngine.Debug.Log("Turned To Active");
        }

        // アクチュエータの端から脱したとき
        if (true) {
            
        }
    }

    private bool TurnedToActive() {
        return _target.Active && OnTheWayOfOutOfRecognitionExecution;
    }

    private bool ReachedToTop() {
        return (ActuatorStep >= TopStep);
    }

    private bool ReachedToBottom() {
        return (ActuatorStep <= BottomStep);
    }





    // 前のフレームからt_ms[ms]経っているか否か
    bool PassedOneFrameBefore(float t_ms)
    {
        TimeInOneFrameBefore = TimeInCurrentFrame;
        TimeInCurrentFrame = stopWatch.ElapsedMilliseconds;
        FrameInterval = TimeInCurrentFrame - TimeInOneFrameBefore;

        return FrameInterval >= t_ms;
    }


    public void SetWasTrackingDone(bool flag)
    {
        TrackingDone = flag;
    }


    bool PulseWidthAbsoluteIsSmallerThan(int w2)
    {
        return Mathf.Abs(pulse_width) < (float)w2;
    }
}