// シリアル通信の輻湊テスト

void setup() {
  Serial.begin(115200); //シリアルポートを115200bpsで開く
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
    }
  }
  int num = str.toInt();
  //毎loopごとにパルス数に見立てた整数を送る
  //Serial.print(num);
  //Serial.println("10000");
  //Serial.println();
  Serial.write("10000");
}

