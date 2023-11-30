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

    // キューブ関係
    public GameObject cube;
    Vector3 cube_position;

    // オブジェクト関連のフィールド
    public GameObject target; // トラッキング座標を有するオブジェクト

    public Record _record;

    // パルス幅の最大・最小
    public int MAX_PULSEWIDTH;
    public int MIN_PULSEWIDTH;

    // 座標系統
    private Vector3 pos; // 取得座標
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
    bool was_tracking_done = false;


    int RecordCount = 0; // 記録ファイル数

    float ms_per_flame_i = 0; // 実行開始からの時刻
    float ms_per_flame_imin1 = 0; // ms_per_flameの1フレーム前
    float delta_ms_per_flame_i = 0; // ms_per_flameの前フレームとの差分

    public Text PulseWidthText;

    // タイマー
    Stopwatch stopWatch;

    SynchronizationContext context;


    void Start()
    {
        Thread_1();
    }

    void OnApplicationQuit()//アプリ終了時の処理（無限ループを解放）
    {
        Flag_loop = false;//無限ループフラグを下げる
    }


    void Thread_1()
    {
        context = SynchronizationContext.Current;
        stopWatch = new Stopwatch();

        Task.Run(() =>
        {
            while (Flag_loop)//無限ループフラグをチェック
            {
                try
                {
                    //if (was_tracking_done) iequalszero();
                    CalculatePulseWidth();
                }
                catch (System.Exception e)//例外をチェック
                {
                    UnityEngine.Debug.LogWarning(e);//エラーを表示
                }
            }
        });
    }


    void CalculatePulseWidth() {
        if (!was_tracking_done) return;

        // 時間関係
        if (stopWatch.IsRunning) {
            stopWatch.Stop();
            ms_per_flame_imin1 = ms_per_flame_i;
            ms_per_flame_i = (float)stopWatch.ElapsedMilliseconds;
        }
        stopWatch.Start();

        //UnityEngine.Debug.Log("done");

        // トラッキングを行っていないとき
        /*
        if (IsSendStop) {
            IsFirstExecution = true;
            pulse_width = MAX_PULSEWIDTH;
            return;
        }
        */

        
        // 座標取得
        context.Post(__ =>
        {
            //pos = target.transform.position;
            pos = Input.mousePosition;
            pos.z = pos.x;
        }, null);

        // アクチュエータを動かし始めて一番最初の実行の時
        if (IsFirstExecution) {
            IsFirstExecution = false;
            x_i = -pos.z;
            pulse_width = MAX_PULSEWIDTH;
            w_i = w_imin1 = pulse_width;
            //return;
        }


        // 各値の更新
        // アクチュエータが普通に動いているとき
        x_imin1 = x_i;
        x_i = -pos.z;

        // ---- パルス幅の計算 ----
        // 簡略化した式（２変数関数）で計算
        w_imin1 = pulse_width;
        delta_x_i = x_i - x_imin1;
        delta_ms_per_flame_i = ms_per_flame_i - ms_per_flame_imin1;
        pulse_width = (int)((3f*delta_ms_per_flame_i) / (1000f*delta_x_i));
        if (Math.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        if (Math.Abs(delta_x_i) <= 0.0001f) pulse_width = MAX_PULSEWIDTH;
        if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
            if (pulse_width >= 0 ) pulse_width = MIN_PULSEWIDTH;
            else pulse_width = -MIN_PULSEWIDTH;
        }
        w_i = pulse_width;
        delta_w_i = w_i - w_imin1;
    

        // シリアル通信で渡す
        serialHandler.Write(pulse_width.ToString()+"\n");
        PulseWidthWasSent = true;
        UnityEngine.Debug.Log("pulse_width = "+pulse_width+", delta_ms_per_flame_i = "+delta_ms_per_flame_i+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1);
        //_record.pulsewidth_list.Add(pulse_width);
        //PulseWidthText.text = pulse_width.ToString();
        

        was_tracking_done = false;
        PulseWidthWasSent = false;


        /*
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (IsRecording)
            {
                ++RecordCount;
                // _record.LogSave(_record.pulsewidth_list, "pulsewidth_normal"+RecordCount, true);
                //_record.LogSave(_record.pulsewidth_list, "pulsewidth_afterFlag"+RecordCount, true);
                //_record.LogSave(_record.pulsewidth_list, "pulsewidth_afterEvent"+RecordCount, true);
                _record.LogSave(_record.pulsewidth_list, "pulsewidth_noisecheck"+RecordCount, true);
            }
            IsRecording = !IsRecording;
        }
        */
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
                    // pulse_width = (int)(-MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / nijou( (float)n));
                    pulse_width = (int)(-MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / (float)n);
                    // UnityEngine.Debug.Log("if if i = "+i);
                // 負から正に行くとき
                } else {
                    delta_w_i_inelse = w_i - MAX_PULSEWIDTH;
                    // pulse_width = (int)(MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / nijou( (float)n ));
                    pulse_width = (int)(MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / (float)n);
                }
                
            } else {
                // 正から負に行くとき
                if (w_imin1 > 0) delta_w_i_inelse = MAX_PULSEWIDTH - w_imin1;
                // 負から正に行くとき                   
                else delta_w_i_inelse = -MAX_PULSEWIDTH - w_imin1;
                // pulse_width = (int)(w_imin1 + (float)(delta_w_i_inelse*i / nijou(( (float)n ))));
                pulse_width = (int)(w_imin1 + (float)(delta_w_i_inelse*i / (float)n));
            }
        }
    }

    public void SetWasTrackingDone(bool flag)
    {
        was_tracking_done = flag;
    }
}