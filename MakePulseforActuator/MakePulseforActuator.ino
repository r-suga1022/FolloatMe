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
//int InputPin = 6;
//int OutputPlusPin = 16;
//int OutputMinusPin = 19;
//int OutputDirPlusPin = 22;
//int OutputDirMinusPin = 27;
int OutputPlusPin = 0;
int OutputMinusPin = 3;
int OutputDirPlusPin = 6;
int OutputDirMinusPin = 10;
int LEDPin = 15;
int FrameCountPin = 14;


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
int FrameCheck = 0; // 1フレームにつきON（OFF）


// 実行開始時
int ExecutionStart = true; // 実行開始時であるか否か

// --- 数値関係 ---
int default_intvl_us = 6000; // デフォルトのパルス幅
int pulse_width = 10000; // 現在のパルス幅
int step_count = 0; // 現在のステップ数（座標）
int MAX_PULSEWIDTH = 10000;

int count = 0;
int LEDValue = 0;
// ----------------------

int WidthList[10000];
int WidthRecording = 0;
int MaxWidthNumber = 10000;
int i = 0;



// ------ setup関数 ------
void setup() {
  // 各種ピン設定
  //pinMode(InputPin, INPUT);
  pinMode(OutputPlusPin, OUTPUT);
  pinMode(OutputMinusPin, OUTPUT);
  pinMode(OutputDirPlusPin, OUTPUT);
  pinMode(OutputDirMinusPin, OUTPUT);
  pinMode(FrameCountPin, OUTPUT);

  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(LEDPin, OUTPUT);

  // シリアル通信設定
  Serial.begin(115200);
  //Serial.begin(921600);

  // タイマー割り込みを使うとき
  add_repeating_timer_us(default_intvl_us, Generate_Pulse, NULL, &st_tm1ms);
}


// ------ loop関数 ------
void loop() {
  //if (i >= MaxWidthNumber) return;
  FrameCheck = !FrameCheck;
  //digitalWrite(FrameCountPin, FrameCheck);

  // 回転方向の指定（DIR入力）
  digitalWrite(OutputDirPlusPin, direction_flag);
  digitalWrite(OutputDirMinusPin, !direction_flag);

  // シリアル通信で文字列を受け取ったら
  if (Serial.available() > 0)
  {
    /*
    String str = "";
    while (Serial.available())    // 文字列を取得
    {
      char key = Serial.read();
      str += key;
    }
    */
    
    String data = Serial.readStringUntil('\n');
    if (data[0] == 's') Serial.println("Sdesu");
    pulse_width = data.toInt();    // 整数値に変換
    /*
    WidthList[i] = pulse_width;
    ++i;
    if (i >= MaxWidthNumber) SendWidthList();
    */

    //if (count++ > 100) {LEDValue = !LEDValue; count = 0; }
    //digitalWrite(LED_BUILTIN, LEDValue);
    // if (DoesStop) DoesStop = false;
    direction_flag = (pulse_width > 0);
    DoesStop = (pulse_width >= MAX_PULSEWIDTH);
    //digitalWrite(LEDPin, !DoesStop);
    //Serial.println("from micon = "+pulse_width);
    Serial.println(step_count/2);
  }
  //Serial.println("from_micon = "+pulse_width);
  //Serial.println("36000");

  DoesStop = (pulse_width >= MAX_PULSEWIDTH);
  if (pulse_width > 0) Accelerate(pulse_width);
  else Accelerate(-pulse_width);
}


void SendWidthList()
{
  Serial.println("Exception!");
  delay(1000);
  int j;
  for (j = 0; j < MaxWidthNumber; ++j)
  {
    Serial.println(String(WidthList[j])+", j = "+j);
    // Serial.println(String(WidthList[j]));
    // Serial.flush();
    delay(6);
  }
}


// ------ 各種関数定義 ------
// タイマー割り込みで実行される関数
bool Generate_Pulse(struct repeating_timer *t) {
  // アクチュエータを動かすときのみパルス波を発生させる
  if (DoesStop) return true;

  // パルス入力
  check = !check;
  digitalWrite(OutputPlusPin, check);
  digitalWrite(OutputMinusPin, !check);

  if (direction_flag) step_count += 1;
  else step_count -= 1;
  
  return true;
}


// 加減速処理を行う
void Accelerate(int dist_intvl_us) {
  // パルス幅指定
  st_tm1ms.delay_us = dist_intvl_us;
}