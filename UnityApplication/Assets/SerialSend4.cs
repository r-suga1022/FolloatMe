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

public class SerialSend4 : MonoBehaviour
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
    
    bool IsRecording = false; // パルス幅をファイルに記録しているか否か
    bool wast_tracking_done = false;
    int RecordCount = 0; // 記録ファイル数


    // 数値関係
    int i = 0; // i = [0, n]
    public int n; // nフレームに1回座標を取得する
    public int N_send; // Nフレームに1回パルス幅を計算する
    public float step_max;
    public float z_max;
    public int pos_rate;
    int flame_count = 0;
    float ms_per_flame_i = 0; // 実行開始からの時刻
    float ms_per_flame_imin1 = 0; // ms_per_flameの1フレーム前
    float delta_ms_per_flame_i = 0; // ms_per_flameの前フレームとの差分

    // タイマー
    Stopwatch stopWatch;



    void Start()
    {
        stopWatch = new Stopwatch();
    }


    // パルス幅を計算する
    public void Calculate_PulseWidth() 
    // void LateUpdate()
    // void FixedUpdate()
    // void Update()
    {
        if (!wast_tracking_done) return;

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
        iequalszero();
        /*
        if (i == 0) {
            iequalszero();
        // 加減速処理
        } else {
            acceleration();
        }
        */

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

        _record.pulsewidth_list.Add(pulse_width);

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (IsRecording)
            {
                ++RecordCount;
                // _record.LogSave(_record.pulsewidth_list, "pulsewidth_normal"+RecordCount, true);
                // _record.LogSave(_record.pulsewidth_list, "pulsewidth_afterFlag"+RecordCount, true);
                _record.LogSave(_record.pulsewidth_list, "pulsewidth_afterEvent"+RecordCount, true);
            }
            IsRecording = !IsRecording;
        }

        // 念のため表示
        
        // UnityEngine.Debug.Log("pulse_width = "+pulse_width+", i = "+i+", delta_ms_per_flame_i = "+delta_ms_per_flame_i+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1);
        /*
        Text pulse_width_text = pulsewidth_text_object.GetComponent<Text> ();
        pulse_width_text.text = ""+pulse_width;
        */
        UnityEngine.Debug.Log("奥行き方向にキャラクターを移動");
        wast_tracking_done = false;
        ++i;
        if (i % n == 0) i = 0;
    }


    void iequalszero() {
        //
        if (stopWatch.IsRunning) {
            stopWatch.Stop();
            ms_per_flame_imin1 = ms_per_flame_i;
            ms_per_flame_i = (float)stopWatch.ElapsedMilliseconds;
            // UnityEngine.Debug.Log("RunTime(ms) = " + ms_per_flame_i);
            stopWatch.Start();
        }
        
        // 座標取得
        pos = target.transform.position;


        // アクチュエータを動かし始めて一番最初の実行の時
        if (IsFirstExecution) {
            IsFirstExecution = false;
            x_i = -pos.z;
            x_imin1 = x_i;
            x_origin = x_i;
            s_x_i = 0;
            // UnityEngine.Debug.Log("pulse_width = "+pulse_width+", i = "+i+", ms_per_flame_i = "+ms_per_flame_i+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1);
            stopWatch.Start();
            w_i = w_imin1 = MAX_PULSEWIDTH;
            pulse_width = MAX_PULSEWIDTH;
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

        // 簡略化した式（２変数関数）で計算
        delta_x_i = x_i - x_imin1;
        delta_ms_per_flame_i = ms_per_flame_i - ms_per_flame_imin1;
        pulse_width = (int)((3f*delta_ms_per_flame_i) / (1000f*delta_x_i));
        if (Math.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        if (Math.Abs(delta_x_i) <= 0.0001f) pulse_width = MAX_PULSEWIDTH;
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

    public void SetWasTrackingDone(bool flag)
    {
        wast_tracking_done = flag;
    }
}