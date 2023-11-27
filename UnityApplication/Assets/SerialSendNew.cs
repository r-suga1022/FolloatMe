/* 別スレッドでパルス幅を計算し、シリアル通信で送信する */
/* 注意：ここでいうパルス幅とは、立ち上がり時間（立ち下がり時間）のことを指している */


using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using System.Diagnostics;
using UnityEngine.Profiling;
using System;

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
    float x_i = 0f; // トラッキングx座標
    float x_imin1 = 0f; // 前のフレームでのトラッキングx座標
    float delta_x_i; // x座標の差分
    int pulse_width; // パルス幅
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

    float ms_per_flame_i = 0; // 実行開始からの時刻
    float ms_per_flame_imin1 = 0; // ms_per_flameの1フレーム前
    float delta_ms_per_flame_i = 0; // ms_per_flameの前フレームとの差分

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
                    iequalszero();
                }
                catch (System.Exception e)//例外をチェック
                {
                    UnityEngine.Debug.LogWarning(e);//エラーを表示
                }
            }
        });
    }


    void iequalszero() {
        if (!wast_tracking_done) return;

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
            pos = target.transform.position;
            UnityEngine.Debug.Log("メインスレッドで座標取得, pos = "+pos);
        }, null);

        if (stopWatch.IsRunning) {
            stopWatch.Stop();
            ms_per_flame_imin1 = ms_per_flame_i;
            ms_per_flame_i = (float)stopWatch.ElapsedMilliseconds;
            // UnityEngine.Debug.Log("RunTime(ms) = " + ms_per_flame_i);
            stopWatch.Start();
        }

        // アクチュエータを動かし始めて一番最初の実行の時
        if (IsFirstExecution) {
            IsFirstExecution = false;
            x_i = -pos.z;
            x_imin1 = x_i;
            x_origin = x_i;
            s_x_i = 0;
            pulse_width = MAX_PULSEWIDTH;
            stopWatch.Start();
            return;
        }

        // 各値の更新
        // アクチュエータが普通に動いているとき
        x_imin1 = x_i;
        x_i = -pos.z;

        // ---- パルス幅の計算 ----
        // 簡略化した式（２変数関数）で計算
        delta_x_i = x_i - x_imin1;
        delta_ms_per_flame_i = ms_per_flame_i - ms_per_flame_imin1;
        pulse_width = (int)((3f*delta_ms_per_flame_i) / (1000f*delta_x_i));
        if (Math.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
        if (Math.Abs(delta_x_i) <= 0.0001f) pulse_width = MAX_PULSEWIDTH;
        if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
            if (pulse_width >= 0 ) pulse_width = MIN_PULSEWIDTH;
            else pulse_width = -MIN_PULSEWIDTH;
        }

        // シリアル通信で渡す
        serialHandler.Write(pulse_width.ToString());

        UnityEngine.Debug.Log("execute");



        _record.pulsewidth_list.Add(pulse_width);

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

        // 念のため表示
        
        UnityEngine.Debug.Log("pulse_width = "+pulse_width+", delta_ms_per_flame_i = "+delta_ms_per_flame_i+", delta_x_i = "+delta_x_i+", x_i = "+x_i+", x_imin1 = "+x_imin1);
        wast_tracking_done = false;
    }

    public void SetWasTrackingDone(bool flag)
    {
        wast_tracking_done = flag;
    }
}