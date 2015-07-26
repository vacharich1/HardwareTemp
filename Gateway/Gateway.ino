/* Display */
#include <LiquidCrystal.h>
LiquidCrystal lcd(12, 11, 5, 4, 3, 2);

/* Variables */
String text = "Message";
char tempchar;
bool changed;
bool twoGPUs = false;

/* Data */
int CPU_Load;
int CPU_Temp;

int GPU1_Load;
int GPU1_Temp;

int GPU2_Load;
int GPU2_Temp;


void setup() {
  Serial.begin(115200);
  lcd.begin(16, 2);
  lcd.print(text);
}

void loop() {

  delay(100);
  
  Serial.print("@PC");
  
  while (Serial.available() == 0) {
    delay(10);
  }

  if ( Serial.available() ) {
    text = ""; //Flush the old messsage
    while ( Serial.available() ) {
      tempchar = Serial.read();
      text.concat(tempchar); //Add each character to the text string
    }
    changed = true;
  }


  if (changed) {
    Parse_Data(text);
    Display();
    changed = false;
  }

}


void Parse_Data(String input) {

  int index = 0;
  String temp;

  //CPU

  while (input.charAt(index) != '/') {
    temp.concat(input.charAt(index));
    index++;
  }
  CPU_Load = temp.toInt();

  temp = "";
  index++;

  while (input.charAt(index) != '#') {
    temp.concat(input.charAt(index));
    index++;
  }
  CPU_Temp = temp.toInt();

  temp = "";
  index++;





  //GPU1

  while (input.charAt(index) != '/') {
    temp.concat(input.charAt(index));
    index++;
  }
  GPU1_Load = temp.toInt();

  temp = "";
  index++;

  while (input.charAt(index) != '#') {
    temp.concat(input.charAt(index));
    index++;
  }
  GPU1_Temp = temp.toInt();

  temp = "";
  index++;




  //GPU2
  if (input.charAt(index) != '!') {
    twoGPUs = true;
    index++;

    while (input.charAt(index) != '/') {
      temp.concat(input.charAt(index));
      index++;
    }
    GPU2_Load = temp.toInt();

    temp = "";
    index++;

    while (input.charAt(index) != '#') {
      temp.concat(input.charAt(index));
      index++;
    }
    GPU2_Temp = temp.toInt();

    temp = "";
    index++;

  }
  else {
    twoGPUs = false;
  }

}


void Display() {
  lcd.clear();
  
  lcd.setCursor(0, 0);
  lcd.print("CPU: L:");
  lcd.print(CPU_Load);
  lcd.print(" T:");
  lcd.print(CPU_Temp);
  
  lcd.setCursor(0, 1);
  lcd.print("GPU:");
  lcd.print(GPU1_Load);
  lcd.print("/");
  lcd.print(GPU1_Temp);
  
  if (twoGPUs) {
    lcd.print("#");
    lcd.print(GPU2_Load);
    lcd.print("/");
    lcd.print(GPU2_Temp);
  }
}


void Debug_Return() {
  Serial.println(CPU_Load);
  Serial.println(CPU_Temp);
  Serial.println(GPU1_Load);
  Serial.println(GPU1_Temp);
  Serial.println(GPU2_Load);
  Serial.println(GPU2_Temp);
}

