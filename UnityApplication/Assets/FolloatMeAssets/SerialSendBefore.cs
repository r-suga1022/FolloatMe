// --------------------------------------------------------------------------------
// UnityからArduino IDEに、シリアル通信で何かを送信するコード
// ここでパルス幅を計算し、Raspberry Pi Picoに送信する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UnityEngine.UI;

public class SerialSendBefore : MonoBehaviour
{
    public int MAX_PULSEWIDTH;
    public int MIN_PULSEWIDTH;

    // オブジェクト関連のフィールド
    public GameObject target; // トラッキング座標を有するオブジェクト
    public SerialHandler serialHandler; // シリアル通信の核
    public GameObject pulsewidth_text_object; // テスト用テキスト

    // 座標系統
    private Vector3 pos;
    float x_i = 0f; // トラッキングx座標
    float x_imin1 = 0f; // 前のフレームでのトラッキングx座標
    float delta_x_i; // x座標の差分
    int w_i; // i番目のパルス幅
    int w_imin1; // i-1番目のパルス幅
    int pulse_width; // パルス幅
    int delta_w_i;
    int delta_w_i_inelse;

    float sum_x_i = 0f;


    // フラグ関係
    bool IsFirstExecution = true; // これが一番最初の実行であるか否か
    public bool IsSendStop = false; // シリアル通信で送るのをストップしているか否か
    bool IsActuatorStop = true; // アクチュエータが止まっているか否か


    // 数値関係
    int i = 0; // i = [0, n]
    public int n; // nフレームに1回座標を取得する
    int current_waittime = 0;
    int waittime = 0;

    public float step_max;
    public float z_max;

    public int pos_rate;

    public int w_i_threshold;

    bool IsFirstExecution2 = true;
    public int followdelay;

    int tinypulse_framecount = 0;
    public int tinypulse_framecount_threshold;
    int rapidpulse_framecount = 0;
    public int rapidpulse_framecount_threshould;
    int count = 0; // このフレームが実行された回数
    int frame_intvl = 0; // フレーム間隔


    float x_origin;
    int s_x_i;
    int s_x_iplus1;

    // void FixedUpdate() {
    // void Update() {
    // トラッキング座標が更新されてから（Update）実行されることを保証するため
    void LateUpdate() {

        // トラッキングを行っていないとき
        if (IsSendStop) {
            IsFirstExecution = true;
            pulse_width = MAX_PULSEWIDTH;
            
        } else {
            // トラック対象座標取得
            // pos = Input.mousePosition; // テストのため、マウス座標を用いる
            // チャタリング対策のため、中央値フィルタを用いる
            pos = target.transform.position;
            x_i = -pos.z;
            Debug.Log("x_i = "+x_i);

            // 座標取得フレームの時
            if (i % n == 0) {
                // 各値の更新
                // アクチュエータを動かし始めて一番最初の実行の時
                if (IsFirstExecution) {
                    x_i = -pos.z;
                    x_origin = x_i;
                    w_i = w_imin1 = pulse_width = MAX_PULSEWIDTH;
                    IsFirstExecution = false;
                    return;
                }
                // アクチュエータが止まっているとき（pulse_width = MAX_PULSEWIDTH）
                // トラッキング出来なかったとき、座標がワープしないようにする
                /*
                } else if (IsActuatorStop) {
                    x_i = pos.z;
                    w_imin1 = w_i;
                    Debug.Log("IsActuatorStop is working.");
                */
                // アクチュエータが普通に動いているとき
                x_imin1 = x_i;
                w_imin1 = w_i;

                i = 0;
                pulse_width = w_i;

                // ---- パルス幅の計算 ----
                // // x座標の差分
                // delta_x_i = x_i - x_imin1;

                // // ステップ数の変化に変換
                // float delta_step = (delta_x_i / z_max) * step_max;

                // // 周波数に変換（0.01sごとに実行するため）
                // float f = delta_step / 0.01f;

                // if (Mathf.Abs(f) <= 0.05f) w_i = MAX_PULSEWIDTH;
                
                // // パルス幅に変換
                // w_i = (int)(1000000f / f);


                s_x_i = s_x_iplus1;
                // s_x_i = serialReceive.received_data;
                s_x_iplus1 = (int)((((x_i - x_origin)*1000.0f) / 6.0f) * 2500.0f);
                int delta_s = s_x_iplus1 - s_x_i;

                if (delta_s == 0) w_i = MAX_PULSEWIDTH;
                else w_i = (int)(Time.deltaTime*10000000.0f / delta_s);

                pulse_width = w_i;

                // パルス幅が急に小さくなった時（チャタリング対策）
                /*
                // if (Mathf.Abs((float)w_i) >= (float)w_i_threshold && w_i*w_imin1 > 0)
                if (Mathf.Abs((float)w_i) >= (float)w_i_threshold)
                // if (Mathf.Abs(delta_step) <= 5f)
                {
                    w_i = w_imin1;
                    ++tinypulse_framecount;
                }
                //
                if (Mathf.Abs((float)w_i) <= MIN_PULSEWIDTH) 
                {                
                    w_i = w_imin1;
                    ++rapidpulse_framecount;
                }

                // if (Mathf.Abs(delta_x_i) <= 0.5f) w_i = w_imin1;

                //
                // 一定数小さい変化が続いたら、パルスを止める（チャタリングと区別するため）
                /*
                if (tinypulse_framecount >= tinypulse_framecount_threshold) {
                    tinypulse_framecount = 0;
                    w_i = MAX_PULSEWIDTH;
                    IsActuatorStop = true;
                }
                */
                /*
                if (rapidpulse_framecount >= rapidpulse_framecount_threshould) {
                    rapidpulse_framecount = 0;
                    // if (w_i > 0) w_i = MIN_PULSEWIDTH;
                    // else w_i = -MIN_PULSEWIDTH;
                }
                */

                // if (w_i < MAX_PULSEWIDTH) IsActuatorStop = false;
                
                // 前のパルス幅との差分計算
                delta_w_i = w_i - w_imin1;
                /*
                if (Mathf.Abs((float)delta_w_i) >= 50f) {
                    w_i = w_imin1;
                    delta_w_i = 0;
                }
                */
            } 
            
            // 加減速処理フレームの時
            else {
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
                            pulse_width = (int)(-MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / (float)n );
                            // Debug.Log("if if i = "+i);
                        // 負から正に行くとき
                        } else {
                            delta_w_i_inelse = w_i - MAX_PULSEWIDTH;
                            pulse_width = (int)(MAX_PULSEWIDTH + (float)(delta_w_i_inelse*(i-n/2)) / (float)n );
                        }
                        
                    } else {
                        // 正から負に行くとき
                        if (w_imin1 > 0) delta_w_i_inelse = MAX_PULSEWIDTH - w_imin1;
                        // 負から正に行くとき                   
                        else delta_w_i_inelse = -MAX_PULSEWIDTH - w_imin1;
                        pulse_width = (int)(w_imin1 + (float)(delta_w_i_inelse*i / (float)n ));
                    }
                }
                // Debug.Log("delta_w_i = "+delta_w_i);
                // Debug.Log("delta_w_i_inelse = "+delta_w_i_inelse);
                // Debug.Log("w_imin1 = "+w_imin1+", w_i = "+w_i);
            }

            // 各種例外処理（速度超過やパルスを生成しないときなど）
            // if (Mathf.Abs((float)delta_w_i) <= 20f) pulse_width = w_imin1;
            // if (Mathf.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;
            /*
            if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
                if (pulse_width > 0) pulse_width = MIN_PULSEWIDTH;
                else pulse_width = -MIN_PULSEWIDTH;
            }
            */
            ++i;
        }


        // シリアル通信で渡す
        serialHandler.Write(pulse_width.ToString());

        // 念のため表示
        Debug.Log("pulse_width = "+pulse_width+", i = "+(i-1));
        // Debug.Log("w_i = "+w_i+", w_imin1 = "+w_imin1);
        // if (i % n == 0) {
            // Text pulsewidth_text = pulsewidth_text_object.GetComponent<Text>();
            // pulsewidth_text.text = ""+pulse_width;
        // }
        
        /*
        ++count;
        if (count >= int.MaxValue) count = 0;
        Text pulsewidth_text = pulsewidth_text_object.GetComponent<Text>();
        pulsewidth_text.text = ""+count;
        */ 
    }


    float nijou(float x) {
        return x*x;
    }

}
