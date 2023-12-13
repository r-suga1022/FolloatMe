const int XPin = 26;
const int YPin = 27;
const int ZPin = 28;
const int SleepPin = 15;
const int LEDPin = 16;
const int VCCPin = 21;

int x, y, z;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  analogReadResolution(12);

  pinMode(LEDPin, OUTPUT);

  digitalWrite(LEDPin, HIGH);
}

void loop() {
  // put your main code here, to run repeatedly:
  x = analogRead(XPin);
  y = analogRead(YPin);
  z = analogRead(ZPin);
  //Serial.print(x, DEC); Serial.print(" ");
  //Serial.print(y, DEC); Serial.print(" ");
  //Serial.println(z, DEC);
  Serial.println("x = "+String(x)+", y = "+String(y)+", z = "+String(z));
  //Serial.print(x, DEC); Serial.print(y, DEC); Serial.print(z,DEC); Serial.println();
  //delay(500);
}
