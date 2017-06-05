#include <Wire.h>
#define STATUS 13
#define BUTTON 2
#define CALIBRATE_BUTTON 3
#define POT A0
#define RUMBLE 12

#define    MPU9250_ADDRESS            0x68
#define    MAG_ADDRESS                0x0C
#define    GYRO_FULL_SCALE_250_DPS    0x00  
#define    GYRO_FULL_SCALE_500_DPS    0x08
#define    GYRO_FULL_SCALE_1000_DPS   0x10
#define    GYRO_FULL_SCALE_2000_DPS   0x18
#define    ACC_FULL_SCALE_2_G        0x00  
#define    ACC_FULL_SCALE_4_G        0x08
#define    ACC_FULL_SCALE_8_G        0x10
#define    ACC_FULL_SCALE_16_G       0x18

#define ROTATION_COMMAND 'R'
#define ZOOM_COMMAND 'Z'
#define BUTTON_DOWN_COMMAND 'D'
#define BUTTON_UP_COMMAND 'U'
#define CALIBRATE_COMMAND 'C'
#define COLOR_COMMAND "COL"

bool connectedToClient = false;
static bool isButtonDown = false;
int shortBuzzTime = 25;
unsigned long buzzMillis = 0; 

struct Vector3
{
  int16_t x;
  int16_t y;
  int16_t z;
};
byte RGBPins[3] = {11,10,9};
Vector3 localRGBColor = {255,255,255};
Vector3 gameRGBColor = {255,255,255};
float redBias = 1;
float greenBias = 0.25;
float blueBias = 0.25;

//Equality operator overloads for Vector3
inline bool operator==(const Vector3& lhs, const Vector3& rhs){ if(lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z)return true; else return false; }
inline bool operator!=(const Vector3& lhs, const Vector3& rhs){ return !(lhs == rhs); }

void setup() 
{
  Wire.begin();
  Serial.begin(115200);
  pinMode(STATUS, OUTPUT);
  pinMode(RUMBLE, OUTPUT);
  pinMode(POT, INPUT);
  pinMode(BUTTON, INPUT_PULLUP);
  pinMode(CALIBRATE_BUTTON, INPUT_PULLUP);
  for(int i = 0; i<3; i++) pinMode(RGBPins[i], OUTPUT);
  attachInterrupt(digitalPinToInterrupt(BUTTON), SendButton, CHANGE);
  attachInterrupt(digitalPinToInterrupt(CALIBRATE_BUTTON), SendCalibrate, LOW);
  // Configure gyroscope range
  I2CwriteByte(MPU9250_ADDRESS,27,GYRO_FULL_SCALE_2000_DPS);
  // Configure accelerometers range
  I2CwriteByte(MPU9250_ADDRESS,28,ACC_FULL_SCALE_16_G);
}

void I2Cread(uint8_t Address, uint8_t Register, uint8_t Nbytes, uint8_t* Data)
{
  // Set register address
  Wire.beginTransmission(Address);
  Wire.write(Register);
  Wire.endTransmission();
  
  // Read Nbytes
  Wire.requestFrom(Address, Nbytes); 
  uint8_t index=0;
  while (Wire.available())
  Data[index++]=Wire.read();
}

void I2CwriteByte(uint8_t Address, uint8_t Register, uint8_t Data)
{
  // Set register address
  Wire.beginTransmission(Address);
  Wire.write(Register);
  Wire.write(Data);
  Wire.endTransmission();
}

bool buttonDebounce = false;
int16_t rgbCount = 250;
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
    
    bool isBuzzing = buzzMillis+shortBuzzTime > millis();
    if(!isBuzzing)
    {
        SendGyroData();
        SendPotValue();
    }
    digitalWrite(RUMBLE, isBuzzing); 

    if(localRGBColor != gameRGBColor)
    {
      FadeColors();
    }

    if(rgbCount >= 300)
    {
       GenerateColor();
       rgbCount = 0;
    }

    
    delay(16.2); //Delay to match 60 updates per second. Otherwise, the serial bus would be flooded with commands.
    isButtonDown = !digitalRead(BUTTON); //Even though the button is on an interupt system, the pin still needs to be checked to ensure the interupt has the correct button state.
    rgbCount++;
	}

}

void FadeColors()
{
  StepInt(localRGBColor.x, gameRGBColor.x*redBias, 5);
  StepInt(localRGBColor.y, gameRGBColor.y*greenBias, 5);
  StepInt(localRGBColor.z, gameRGBColor.z*blueBias, 5);
  analogWrite(RGBPins[0], localRGBColor.x); //Red
  analogWrite(RGBPins[1], localRGBColor.y); //Green
  analogWrite(RGBPins[2], localRGBColor.z); //Blue
}

void StepInt(int16_t &a, int16_t b, uint16_t rate)
{
  if(a == b)
    return;
  if(b > a)
    a+=rate;
  else
    a-=rate;
  if(a>=255) a = 255;
  if(a<=0) a = 0;
}

int lastPtValue = 1;
void SendPotValue()
{
  int ptValue = analogRead(POT);
  int z = map(ptValue, 0, 1024, 15, 60);
  if(z != lastPtValue)
  {
      Serial.print(ZOOM_COMMAND); 
      Serial.print(':');
      Serial.print(z);
      Serial.println("");
      lastPtValue = z;
  }
}

Vector3 lastAccl;
void SendGyroData()
{
    // Read accelerometer and gyroscope
    uint8_t Buf[14];
    I2Cread(MPU9250_ADDRESS,0x3B,14,Buf);

    Vector3 accl;
    // Create 16 bits values from 8 bits data
    // Accelerometer
    accl.x=-(Buf[0]<<8 | Buf[1]);
    accl.y=-(Buf[2]<<8 | Buf[3]);
    accl.z= Buf[4]<<8 | Buf[5];
    
    accl.x = map(accl.x, -3000, 3000, -360, 360); 
    accl.y = map(accl.y, -3000, 3000, -360, 360); 
    accl.z = map(accl.z, -3000, 3000, -360, 360); 
    // Gyroscope
    int16_t gx=-(Buf[8]<<8 | Buf[9]);
    int16_t gy=-(Buf[10]<<8 | Buf[11]);
    int16_t gz=Buf[12]<<8 | Buf[13];

    if(VectorDistance(accl, lastAccl) > 5) //Compress accl data before sending
    {
         // Accelerometer Out
        Serial.print(ROTATION_COMMAND);
        Serial.print (accl.x,DEC); 
        Serial.print (":");
        Serial.print (accl.y,DEC);
        Serial.print (":");
        Serial.print (accl.z,DEC);  
        Serial.println(""); 
        lastAccl = accl;
    }
    //delay(16.6); //Delay for 60 updates per second.
}

float VectorDistance(Vector3 a, Vector3 b)
{
    return sqr( pow((b.x-a.x),2) + pow((b.y-a.y),2) + pow((b.z-a.z),2));
}

int sqr(int x) //Fast square Root
{
    int s, t;
    s = 1;  t = x;
    while (s < t) {
        s <<= 1;
        t >>= 1;
    }
    do {
        t = s;
        s = (x / s + s) >> 1;
    } while (s < t);
    return t;
}

void SendButton()
{
     static unsigned long last_interrupt_time = 0;
     unsigned long interrupt_time = millis();
     if (interrupt_time - last_interrupt_time > 10) //Debounce check
     {
        Serial.println(isButtonDown == false ? BUTTON_DOWN_COMMAND : BUTTON_UP_COMMAND);
        buzzMillis = interrupt_time;
        isButtonDown = !isButtonDown;
     }
     last_interrupt_time = interrupt_time;
}

void SendCalibrate()
{
     static unsigned long last_interrupt_time2 = 0;
     unsigned long interrupt_time = millis();
     if (interrupt_time - last_interrupt_time2 > 10) //Debounce check
     {
        Serial.println(CALIBRATE_COMMAND);
     }
     last_interrupt_time2 = interrupt_time;
}

void ParseIncommingCommand(String inc) //Send commands in C# using SerialPort.Write, not SerialPort.WriteLine!
{									                      //Send C# commands in this format PIN:INPUTOUTPUT:HIGHLOW
                                        //EX:    12:1:1 (Set pin 12 to output and set to high)
	//Serial.print(millis()/1000);
	//Serial.print("s RECEIVED COMMAND: ");
	//Serial.println(inc);	
	String cmd1 = GetValue(inc, ':', 0);
	String cmd2 = GetValue(inc, ':', 1);
	String cmd3 = GetValue(inc, ':', 2);
  
  int pin = cmd1.toInt();
  pinMode(pin, cmd2 == "0" ? INPUT : OUTPUT); 
  pinMode(pin, cmd3 == "1" ? HIGH : LOW); 
}

void GenerateColor()
{
  randomSeed(millis());
  gameRGBColor.x = random(0,255);
  gameRGBColor.y = random(0,255);
  gameRGBColor.z = random(0,255);
  Serial.print(COLOR_COMMAND); Serial.print(gameRGBColor.x); 
  Serial.print(":"); Serial.print(gameRGBColor.y); 
  Serial.print(":"); Serial.print(gameRGBColor.z); Serial.println("");
}

String GetValue(String data, char separator, int index) //split strings by delimiters
{
	int found = 0;
	int strIndex[] = {0, -1 };
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
