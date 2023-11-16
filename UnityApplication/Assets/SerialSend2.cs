// --------------------------------------------------------------------------------
// UnityからArduino IDEに、シリアル通信で何かを送信するコード
// ここでパルス幅を計算し、Raspberry Pi Picoに送信する

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

using UnityEngine.UI;

public class SerialSend2 : MonoBehaviour
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
    bool Flag_loop = true; // 別スレッドを実行し続けるか否か
    bool IsFirstExecution = true; // これが一番最初の実行であるか否か
    public bool IsSendStop; // シリアル通信で送るのをストップしているか否か
    bool IsActuatorStop = true; // アクチュエータが止まっているか否か


    // 数値関係
    int i = 0; // i = [0, n]
    public int n; // nフレームに1回座標を取得する
    int current_waittime = 0;
    int waittime = 0;

    public float step_max;
    public float z_max;

    public int pos_rate;

    int count = 0; // このフレームが実行された回数
    int frame_intvl = 0; // フレーム間隔

    // SynchronizationContext context;

    void Start() {
        // SynchronizationContext.Current;
        Thread_1();
    }

    
    void OnApplicationQuit()//アプリ終了時の処理（無限ループを解放）
    {
        Flag_loop = false;//無限ループフラグを下げる
    }


    public void Thread_1()//無限ループ本体
    {
        Task.Run(() =>
        {
            while (Flag_loop)//無限ループフラグをチェック
            {
                try
                {
                    // トラッキングを行っていないとき
                    if (IsSendStop) {
                        IsFirstExecution = true;
                        pulse_width = MAX_PULSEWIDTH;
                        return;
                    }

                    // Debug.Log("in");
                    // 座標取得フレームの時
                    // トラック対象座標取得
                    // pos = Input.mousePosition; // テストのため、マウス座標を用いる
                    pos = target.transform.position;

                    // 各値の更新
                    // アクチュエータを動かし始めて一番最初の実行の時
                    if (IsFirstExecution) {
                        IsFirstExecution = false;
                        x_i = -pos.z;
                        return;
                    }
                    // アクチュエータが普通に動いているとき
                    x_imin1 = x_i;
                    w_imin1 = w_i;

                    x_i = -pos.z;
                    Debug.Log("x_i = "+x_i);

                    // i = 0;
                    pulse_width = w_i;

                    // ---- パルス幅の計算 ----
                    // x座標の差分
                    delta_x_i = x_i - x_imin1;

                    // ステップ数の変化に変換
                    float delta_step = (delta_x_i / z_max) * step_max;

                    // 周波数に変換（0.01sごとに実行するため）
                    float f = delta_step / 0.01f;
                    
                    // パルス幅に変換
                    if (Mathf.Abs(f) < 0.001f) w_i = MAX_PULSEWIDTH;
                    else w_i = (int)(1000000f / f);
                    pulse_width = w_i;

                    // 前のパルス幅との差分計算
                    delta_w_i = w_i - w_imin1;

                    // 各種例外処理（速度超過やパルスを生成しないときなど）
                    if (Mathf.Abs((float)pulse_width) <= (float)MIN_PULSEWIDTH) {
                        if (delta_w_i > 0 ) pulse_width = MIN_PULSEWIDTH;
                        else pulse_width = -MIN_PULSEWIDTH;
                    }
                    if (Mathf.Abs((float)pulse_width) >= MAX_PULSEWIDTH) pulse_width = MAX_PULSEWIDTH;


                    // // シリアル通信で渡す
                    // serialHandler.Write(pulse_width.ToString());

                    // 念のため表示
                    Debug.Log("pulse_width = "+pulse_width);
                    // Debug.Log("yes");
                    // Debug.Log("delta_x_i = "+delta_x_i);
                    // Debug.Log("w_imin1 = "+w_imin1+", w_i = "+w_i);
                }
                catch (System.Exception e)//例外をチェック
                {
                    Debug.LogWarning(e);//エラーを表示
                }
            }
        });
    }
}