#define rLED 5
#define gLED 4
#define bLED 3
#define STATUS 8

bool connectedToClient = false;

void setup() 
{
  // put your setup code here, to run once:
  Serial.begin(9600);
  pinMode(rLED, OUTPUT);
  pinMode(bLED, OUTPUT);
  pinMode(gLED, OUTPUT);
  pinMode(STATUS, OUTPUT);
}

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
				connectedToClient = true;
			}
		}
	}

	if (connectedToClient)
	{
		if (Serial.available() > 0)
		{
			ParseIncommingCommand(Serial.readString());
			/*
			if (incommingString == "RED")
				digitalWrite(rLED, HIGH);
			else if (incommingString == "GREEN")
				digitalWrite(gLED, HIGH);
			else if (incommingString == "BLUE")
				digitalWrite(bLED, HIGH);
			else if (incommingString == "OFF")
			{
				digitalWrite(rLED, LOW);
				digitalWrite(gLED, LOW);
				digitalWrite(bLED, LOW);
			}
		}
		else
			digitalWrite(STATUS, LOW);

		Serial.print("PHOTORESISTOR: ");
		Serial.println(analogRead(A1));
		*/
		}
	}
}

void ParseIncommingCommand(String inc) //Send commands in C# using SerialPort.Write, not SerialPort.WriteLine!
{									   //Send commands in this format INPUTOUTPUT:PIN:HIGHLOW
	String delimiter = ":";
	
	digitalWrite(STATUS, HIGH);
	Serial.print(millis()/1000);
	Serial.print("s RECEIVED COMMAND: ");
	Serial.println(inc);
	inc.toUpperCase();
	digitalWrite(STATUS, LOW);
	
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