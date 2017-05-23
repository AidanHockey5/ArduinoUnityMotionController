#define STATUS 13
#define BUTTON 2

bool connectedToClient = false;
static bool isButtonDown = false;

void setup() 
{
  Serial.begin(115200);
  pinMode(STATUS, OUTPUT);
  pinMode(BUTTON, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(BUTTON), SendButton, CHANGE);
}

bool buttonDebounce = false;
void loop()
{
	while (!connectedToClient) //Wait for client hand-shake
	{
		if (Serial.available() > 0)
		{
			String incomming = Serial.readString();
			if (incomming == "DISCOVER")
			{
				Serial.println("ACKNOWLEDGE");
        digitalWrite(STATUS, HIGH);
				connectedToClient = true;
        isButtonDown = !digitalRead(BUTTON);
			}
		}
	}

	if (connectedToClient)
	{
    //Recieve commands
		if (Serial.available() > 0) 
		{
      String incomming = Serial.readString();
      if (incomming == "DISCOVER") //If the client decides to reconnect, do not ignore the DISCOVER command.
      {
        Serial.println("ACKNOWLEDGE");
        digitalWrite(STATUS, HIGH);
        connectedToClient = true;
        isButtonDown = !digitalRead(BUTTON);
      }
      else if(incomming == "TERMINATE")
      {
        digitalWrite(STATUS, LOW);
        connectedToClient = false;
      }
      else
      {
           ParseIncommingCommand(incomming); 
      }
		}

    //Send Commands


    
	}
}

void SendButton()
{
     static unsigned long last_interrupt_time = 0;
     unsigned long interrupt_time = millis();
     if (interrupt_time - last_interrupt_time > 10) //Debounce check
     {
        Serial.println(isButtonDown == false ? "BUTTON_DOWN" : "BUTTON_UP");
        isButtonDown = !isButtonDown;
     }
     last_interrupt_time = interrupt_time;
}


void ParseIncommingCommand(String inc) //Send commands in C# using SerialPort.Write, not SerialPort.WriteLine!
{									                      //Send C# commands in this format INPUTOUTPUT:PIN:HIGHLOW
	String delimiter = ":";
	Serial.print(millis()/1000);
	Serial.print("s RECEIVED COMMAND: ");
	Serial.println(inc);
	inc.toUpperCase();
	
	String cmd1 = GetValue(inc, ':', 0);
	String cmd2 = GetValue(inc, ':', 1);
	String cmd3 = GetValue(inc, ':', 2);

	int pin = cmd2.toInt();

	if (cmd1 == "INPUT")
	{
		pinMode(pin, INPUT);
	}
	else if (cmd1 == "OUTPUT")
	{
		pinMode(pin, OUTPUT);
	}
	if (cmd3 == "HIGH")
	{
		digitalWrite(pin, HIGH);
	}
	else if (cmd3 == "LOW")
	{
		digitalWrite(pin, LOW);
	}
}

String GetValue(String data, char separator, int index) //split strings by delimiters
{
	int found = 0;
	int strIndex[] = {
		0, -1 };
	int maxIndex = data.length() - 1;
	for (int i = 0; i <= maxIndex && found <= index; i++) {
		if (data.charAt(i) == separator || i == maxIndex) {
			found++;
			strIndex[0] = strIndex[1] + 1;
			strIndex[1] = (i == maxIndex) ? i + 1 : i;
		}
	}
	return found>index ? data.substring(strIndex[0], strIndex[1]) : "";
}
