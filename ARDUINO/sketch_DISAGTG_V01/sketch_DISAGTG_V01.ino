#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
#define OLED_RESET    -1
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

const int buttonPin1 = 2;
const int buttonPin2 = 3;
const int buttonPin3 = 4;

String Name = "";
int points = 0;
int number = 0;
String pw = "x";
byte serialInputTyp=0;

void setup() {
  Serial.begin(9600);
  pinMode(buttonPin1, INPUT_PULLUP);
  pinMode(buttonPin2, INPUT_PULLUP);
  pinMode(buttonPin3, INPUT_PULLUP);

  if(!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
    Serial.println(F("SSD1306 allocation failed"));
    for(;;);
  }
  //Display-Text
  display.clearDisplay();
  display.setTextSize(2);
  display.setTextColor(SSD1306_WHITE);
  display.setCursor(0, 0);
  display.print("WARTEN");
  
  display.display();
  
}

void loop() {
  if (Serial.available() > 0) {
    serialInputTyp = Serial.read();
    if(serialInputTyp == 2) //STX
    {     
    
      Name = Serial.readStringUntil(',');
      points = Serial.readStringUntil(',').toFloat();
      number = Serial.readStringUntil(',').toInt();
      pw = Serial.readStringUntil(',');
      updateDisplay();
    }
    Serial.println("ACK");
  }

  if (digitalRead(buttonPin1) == LOW) {
    Serial.println("Button.1");
  }
  if (digitalRead(buttonPin2) == LOW) {
    Serial.println("Button.2");
  }
  if (digitalRead(buttonPin3) == LOW) {
    Serial.println("Button.3");
  }
  
  
  
}



void updateDisplay() {
  display.clearDisplay();
  display.setTextSize(2);
  display.setTextColor(SSD1306_WHITE);
  display.setCursor(0, 0);
  display.print("Name: ");
  display.println(Name);
  display.print("Ring: ");
  display.println(points);
  display.print("Schuss: ");
  display.println(number);
  if(pw=="W"){
    display.println("WERTUNG");
  }
  else{
    display.println("PROBE");
  }  
  display.display();
}
