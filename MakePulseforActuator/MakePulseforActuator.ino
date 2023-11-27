// ------------------------------------------------------------
// はじめに
// このコードは、オリエンタルモーター製の型番EASM4RNXE040AZAKの電動スライダーを
// 簡単に動作確認するためのコードです。矩形パルス波を入力することで、速度と方向が
// 制御できます。なお、入力方式は1パルス方式です。詳細はオンラインの説明書である
// HM-60334Jを参照ください。
// 変数の命名規則などはなく、読みづらかったらすみません。

// 操作方法
// Arduino IDEにこの説明文、コードをそのままコピー＆ペーストしてください。
// Arduino IDEからRaspberry Pi Pico/RP2040を導入し、Raspberry Pi Picoにコードを書き込んでください
// 

// 電気通信大学 情報理工学域4年　小泉研究室
// 菅原 陵央
// ------------------------------------------------------------


// ------ 広域変数 -----
//--- 各ピン番号指定 ---
// Raspberry Pi Pico
int InputPin = 6;
int OutputPlusPin = 16;
int OutputMinusPin = 19;
int OutputDirPlusPin = 22;
int OutputDirMinusPin = 27;


// --- タイマー割り込み関係 ---
struct repeating_timer st_tm1ms;

// --- フラグ関係 ---
// アクチュエータ動作
int check = 0; // パルスのON/OFF
int direction_flag = 1; // 1:正方向, -1:逆方向
int CanPulseGenerate = 1; // パルスを発生させるか否か
int DoesStop = 0; // アクチュエータが止まったか否か
int IsAccelerating = 0; // GKeyが押されて、加速処理中か否か
int IsDeccelerating = 0; // GKeyが押されて、減速処理中か否か


// 実行開始時
int ExecutionStart = true; // 実行開始時であるか否か

// --- 数値関係 ---
int default_intvl_us = 6000; // デフォルトのパルス幅
int pulse_width = 6000; // 現在のパルス幅
int step_count = 0; // 現在のステップ数（座標）
int MAX_PULSEWIDTH = 10000;
// ----------------------



// ------ setup関数 ------
void setup() {
  // 各種ピン設定
  pinMode(InputPin, INPUT);
  pinMode(OutputPlusPin, OUTPUT);
  pinMode(OutputMinusPin, OUTPUT);
  pinMode(OutputDirPlusPin, OUTPUT);
  pinMode(OutputDirMinusPin, OUTPUT);

  // シリアル通信設定
  Serial.begin(115200);

  // タイマー割り込みを使うとき
  add_repeating_timer_us(default_intvl_us, Generate_Pulse, NULL, &st_tm1ms);
}

// ------ loop関数 ------
void loop() {
  // 回転方向の指定（DIR入力）
  digitalWrite(OutputDirPlusPin, direction_flag);
  digitalWrite(OutputDirMinusPin, !direction_flag);

  // 現在のポジションを表示
  // Serial.print("Current position (step) = ");
  // Serial.println(step_count/2);


  // シリアル通信で文字列を受け取ったら
  if (Serial.available() > 0)
  {
    String str = "";
    while (Serial.available())    // 文字列を取得
    {
      char key = Serial.read();
      str += key;
    }
    pulse_width = str.toInt();    // 整数値に変換
    // if (DoesStop) DoesStop = false;
    direction_flag = (pulse_width > 0);
    DoesStop = (pulse_width >= MAX_PULSEWIDTH);
    //Serial.println("from_micon = "+pulse_width);
    //Serial.println("36000");
    //Serial.println();
  }
  //Serial.println("from_micon = "+pulse_width);
  Serial.println("36000");

  DoesStop = (pulse_width >= MAX_PULSEWIDTH);
  if (pulse_width > 0) Accelerate(pulse_width);
  else Accelerate(-pulse_width);
}




// ------ 各種関数定義 ------
// タイマー割り込みで実行される関数
// パルス波を発生させることが分かるように、この名前にする
bool Generate_Pulse(struct repeating_timer *t) {
  // アクチュエータを動かすときのみパルス波を発生させる
  if (DoesStop) return true;

  // パルス入力
  check = !check;
  digitalWrite(OutputPlusPin, check);
  digitalWrite(OutputMinusPin, !check);

  step_count += direction_flag;
  //Serial.print(step_count);
  //Serial.println();

  // アクチュエータの位置情報の送信処理
  if (step_count >= 30000) {
    //Serial.print("hensa");
    //Serial.println();
  }
  
  return true;
}


// 加減速処理を行う
void Accelerate(int dist_intvl_us) {
  // パルス幅指定
  st_tm1ms.delay_us = dist_intvl_us;
}