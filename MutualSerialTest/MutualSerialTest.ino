// シリアル通信の輻湊テスト

int LedPin = LED_BUILTIN;
int pulse_width = 10000;
int i = 0;
int flag = 0;

void setup() {
  Serial.begin(115200); //シリアルポートを115200bpsで開く
  pinMode(LedPin, OUTPUT);
  //Serial.begin(921600);
}

void loop()
{
  String str = "";
  
  if (Serial.available())
  {
    while (Serial.available() > 0)
    {
      char key;
      key = Serial.read();
      str += key;
      ++i;
    }
    if (i > 10000) {
      flag = !flag;
      i = 0;
    }
    digitalWrite(LedPin, flag);
    Serial.println("from micon = "+pulse_width);
    //Serial.println("from micon");
  }
  
  //int num = str.toInt();
  //
  //毎loopごとにパルス数に見立てた整数を送る
  //Serial.print(num);
  //Serial.println("from micon = "+pulse_width);
  //Serial.println();
  //Serial.write("10000");
}

