/* 別スレッドでパルス幅を計算し、シリアル通信で送信する */
/* 注意：ここでいうパルス幅とは、立ち上がり時間（立ち下がり時間）のことを指している */


using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using System.Diagnostics;
using UnityEngine.Profiling;

using UnityEngine.UI;
using System;

public class SerialSend6 : MonoBehaviour
{
    // シリアル通信関係
    public SerialHandler serialHandler; // シリアル通信の核

    // キューブ関係
    public GameObject cube;
    Vector3 cube_position;

    // オブジェクト関連のフィールド
    public GameObject target; // トラッキング座標を有するオブジェクト

    // SynchronizationContext関係
    SynchronizationContext context;

    Stopwatch stopWatch;


    // パルス幅の最大・最小
    public int MAX_PULSEWIDTH;
    public int MIN_PULSEWIDTH;
    public GameObject pulsewidth_text_object; // テスト用テキスト

    // 座標系統
    private Vector3 pos; // 取得座標
    float x_i = 0f; // トラッキングx座標
    float x_imin1 = 0f; // 前のフレームでのトラッキングx座標
    float delta_x_i; // x座標の差分
    int w_i; // i番目のパルス幅
    int w_imin1; // i-1番目のパルス幅
    int pulse_width; // パルス幅
    int delta_w_i;
    int delta_w_i_inelse;
    float x_origin;
    float s_x_i, s_x_iplus1;


    float sum_x_i = 0f;


    // フラグ関係
    bool Flag_loop = true; // 別スレッドを実行し続けるか否か
    bool IsFirstExecution = true; // これが一番最初の実行であるか否か
    public bool IsSendStop; // シリアル通信で送るのをストップしているか否か
    bool IsActuatorStop = true; // アクチュエータが止まっているか否か


    // 数値関係
    int i = 0; // i = [0, n]
    public int n; // nフレームに1回座標を取得する
    public int N_send; // Nフレームに1回パルス幅を計算する
    public float step_max;
    public float z_max;
    public int pos_rate;
    int flame_count = 0;
    float ms_per_flame_i; // 実行開始からの時刻
    float ms_per_flame_imin1; // ms_per_flameの1フレーム前
    float delta_ms_per_flame_i; // ms_per_flameの前フレームとの差分



    void Start()
    {
        OtherThread();
    }

    void OnApplicationQuit()//アプリ終了時の処理（無限ループを解放）
    {
        Flag_loop = false;//無限ループフラグを下げる
        // serialHandler.OnDestroy(); // シリアル通信を閉じる
    }

    public void OtherThread()//無限ループ本体
    {
        context = SynchronizationContext.Current;
        stopWatch = new Stopwatch();

        Task.Run(() =>
        {
            // Profiler.BeginThreadProfiling("group", "name"); // プロファイラで別スレッドを記録するため

            while (Flag_loop)//無限ループフラグをチェック
            {
                try
                {
                    ++flame_count;
                    // UnityEngine.Debug.Log("Test from Unity"+flame_count+"\n");
                    // UnityEngine.Debug.Log("別スレッドで処理, count = "+flame_count+", ID = "+Thread.CurrentThread.ManagedThreadId);

                    // N回に１回のみパルス幅を計算するようにする
                    if (flame_count % N_send == 0) {
                        // 時間計測終了（座標取得ごとの時間間隔を測る）
                        // ここの書き方を改善するかもしれない
                        if (stopWatch.IsRunning && i == 0) {
                            stopWatch.Stop();
                            
                            ms_per_flame_imin1 = ms_per_flame_i;
                            ms_per_flame_i = (float)stopWatch.ElapsedMilliseconds;
                            if (ms_per_flame_i >= Int32.MaxValue) stopWatch.Reset();
                            // UnityEngine.Debug.Log(stopWatch.Elapsed.ToString());
                            UnityEngine.Debug.Log("RunTime(ms) = " + ms_per_flame_i);
                            // 時間計測開始
                            // stopWatch.Reset();
                            stopWatch.Start();
                        } else if (!stopWatch.IsRunning) {
                            stopWatch.Start();
                        }

                        Calculate_PulseWidth();
                    }

                    /*
                    context.Post(__ =>
                    {
                        UnityEngine.Debug.Log("contextでメインスレッド処理, ID = "+Thread.CurrentThread.ManagedThreadId);
                        
                    }, null);
                    */
                    
                    // if (flame_count % n == 0)
                        // serialHandler.Write("Test from Arduino, "+flame_count+"\n");

                }
                catch (System.Exception e)//例外をチェック
                {
                    UnityEngine.Debug.LogWarning(e);//エラーを表示
                }
            }
            // Profiler.EndThreadProfiling(); // ThreadProfilingが使ってたリソースを開放
        });
    }


    // パルス幅を計算する
    void Calculate_PulseWidth() 
    // void LateUpdate()
    {
        // トラッキングを行っていないとき
        if (IsSendStop) {
            IsFirstExecution = true;
            pulse_width = MAX_PULSEWIDTH;
            return;
        }

        // 座標取得フレームの時
        // トラック対象座標取得
        // pos = Input.mousePosition; // テストのため、マウス座標を用いる
        // if (i % n == 0) {
        // N回に１回のみパルス幅を計算するようにする

        // Calculate_PulseWidth();
        if (i == 0) {
            iequalszero();
        // 加減速処理
        } else {
            acceleration();
        }

        // 各種例外処理（速度超過やパルスを生成しないときなど）
        //
        if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
            if (delta_w_i > 0 ) pulse_width = MIN_PULSEWIDTH;
            else pulse_width = -MIN_PULSEWIDTH;
        }
        if (Mathf.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        //

        // シリアル通信で渡す
        serialHandler.Write(pulse_width.ToString());

        // 念のため表示
        UnityEngine.Debug.Log("pulse_width = "+pulse_width+", i = "+i+", delta_ms_per_flame_i = "+delta_ms_per_flame_i+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1);
        Text pulse_width_text = pulsewidth_text_object.GetComponent<Text> ();
        pulse_width_text.text = ""+pulse_width;
        ++i;
        if (i % n == 0) i = 0;
    }


    void iequalszero() {
        /*
        if (stopWatch.IsRunning) {
            stopWatch.Stop();
            ms_per_flame_i = (float)stopWatch.ElapsedMilliseconds;
            // UnityEngine.Debug.Log("RunTime(ms) = " + ms_per_flame_i);
        }

        // 時間計測開始
        stopWatch.Reset();
        stopWatch.Start();
        */
        // メインスレッドに戻してから座標取得
        //
        context.Post(__ =>
        {
            pos = target.transform.position;
            // UnityEngine.Debug.Log("メインスレッドで座標取得, pos = "+pos);
        }, null);
        //
        // pos = target.transform.position;


        // アクチュエータを動かし始めて一番最初の実行の時
        if (IsFirstExecution) {
            IsFirstExecution = false;
            x_i = -pos.z;
            x_imin1 = x_i;
            x_origin = x_i;
            s_x_i = 0;
            // UnityEngine.Debug.Log("pulse_width = "+pulse_width+", i = "+i+", ms_per_flame_i = "+ms_per_flame_i+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1);
            return;
        }

        // 各値の更新
        // アクチュエータが普通に動いているとき
        x_imin1 = x_i;
        w_imin1 = w_i;
        x_i = -pos.z;

        // UnityEngine.Debug.Log("x_i = "+x_i);

        i = 0;
        pulse_width = w_i;

        // ---- パルス幅の計算 ----
        /* 
        s_x_i = s_x_iplus1;
        s_x_iplus1 = ((((x_i - x_origin)*1000.0f) / 6.0f) * 1000.0f);
        float delta_s = s_x_iplus1 - s_x_i;

        // UnityEngine.Debug.Log("s_x_i = "+s_x_i+", s_x_iplus1 = "+s_x_iplus1);

        if (delta_s == 0) pulse_width = MAX_PULSEWIDTH;
        else pulse_width = (int)((50f*(float)n/7.1f)*1000.0f / delta_s)/2;
        */

        // 簡略化した式（２変数関数）で計算
        delta_x_i = x_i - x_imin1;
        delta_ms_per_flame_i = ms_per_flame_i - ms_per_flame_imin1;
        pulse_width = (int)((3f*delta_ms_per_flame_i) / (1000f*delta_x_i));
        // if (Mathf.Abs(delta_x_i) >= 0.001f) pulse_width = MAX_PULSEWIDTH;
        w_i = pulse_width;

        // 前のパルス幅との差分計算
        delta_w_i = w_i - w_imin1;
    }


    void acceleration() {
        // UnityEngine.Debug.Log("w_i = "+w_i);
        // i番目の時
        // if (Mathf.Abs(delta_w_i) <= 10f) pulse_width = w_i;
        if (delta_w_i == 0) pulse_width = w_i;
        // w_{i-1}とw_iが同符号の場合
        else if (w_imin1*w_i > 0) {
            pulse_width = (int)(w_imin1 + (float)(delta_w_i*i)/ (float)n);
        }
        // else if (w_imin1*w_i > 0) pulse_width = w_i;
        // w_{i-1}とw_iが異符号の場合
        else if (w_imin1*w_i < 0) {
            //
            if (w_imin1 == MAX_PULSEWIDTH) {
                w_imin1 = -MAX_PULSEWIDTH;
                delta_w_i_inelse = w_i - w_imin1;
                pulse_width = (int)(w_imin1 + (float)(delta_w_i_inelse*i) / (float)n);
                w_imin1 = MAX_PULSEWIDTH;
            } else if (w_i == MAX_PULSEWIDTH) {
                w_i = -MAX_PULSEWIDTH;
                delta_w_i_inelse = w_i - w_imin1;
                pulse_width = (int)(w_imin1 + (float)(delta_w_i_inelse*i) / (float)n);
                w_i = MAX_PULSEWIDTH;
            }
            //

            if (i > (n/2)) {
                // 正から負に行くとき      
                if (w_imin1 > 0) {
                    delta_w_i_inelse = w_i + MAX_PULSEWIDTH;
                    pulse_width = (int)(-MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / nijou( (float)n));
                    // UnityEngine.Debug.Log("if if i = "+i);
                // 負から正に行くとき
                } else {
                    delta_w_i_inelse = w_i - MAX_PULSEWIDTH;
                    pulse_width = (int)(MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / nijou( (float)n ));
                }
                
            } else {
                // 正から負に行くとき
                if (w_imin1 > 0) delta_w_i_inelse = MAX_PULSEWIDTH - w_imin1;
                // 負から正に行くとき                   
                else delta_w_i_inelse = -MAX_PULSEWIDTH - w_imin1;
                pulse_width = (int)(w_imin1 + (float)(delta_w_i_inelse*i / nijou(( (float)n ))));
            }
        }
    }

    float nijou(float x) {
        return x*x;
    }
}