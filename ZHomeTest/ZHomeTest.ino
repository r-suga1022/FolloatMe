int ZHomePin = 28;

void setup() {
  // put your setup code here, to run once:
  pinMode(ZHomePin, OUTPUT);
  Serial.begin(115200);
}

void loop() {
  // put your main code here, to run repeatedly:
  if (Serial.available() > 0)
  {
    char Key = Serial.read();
    if (Key == 'm')
    {
      Serial.println(Key);
      digitalWrite(ZHomePin, HIGH);
      delay(100);
      digitalWrite(ZHomePin, LOW);
    }
  }
}
