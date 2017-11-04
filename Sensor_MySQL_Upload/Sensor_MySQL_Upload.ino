#include <ArduinoSTL.h>
#include <SoftwareSerial.h>
#define sensor1Pin (14) //moisture
#define sensor2Pin (15) //moislight
#define sensor3Pin (16) //smoke
#define sensor4Pin (17) //temp
#define sensor5Pin (18)
#define sensor6Pin (19)
SoftwareSerial esp8266(10, 11); // RX, TX
//*-- IoT Information
#define HOST "192.168.43.212" // localhost
#define DEBUG true
String sensor1 = "sensor1";
String sensor2 = "sensor2";
String sensor3 = "sensor3";
String sensor4 = "sensor4";
String sensor5 = "sensor5";
String sensor6 = "sensor6";

void setup()
{
    Serial.begin(9600);
    esp8266.begin(9600);
    sendCommand("AT+RST\r\n", 2000, DEBUG);
    delay(500);
    sendCommand("AT+CWMODE=1\r\n", 1000, DEBUG);
    delay(500);
    sendCommand("AT+CWJAP=\"SHAME_ON_YOU\",\"0971534218\"\r\n", 3000, DEBUG);
    delay(9000);
    sendCommand("AT+CIFSR\r\n", 1000, DEBUG); // get ip address
    delay(500);
    sendCommand("AT+CIPMUX=0\r\n", 1000, DEBUG); // configure for multiple connections
    delay(500);
}

void loop()
{
    float sensor1Value = light(sensor1Pin);
    float sensor2Value = moisture(sensor2Pin);
    float sensor3Value = temp(sensor3Pin);
    SentOnCloud(String(sensor1Value, 2), String(sensor2Value, 2), String(sensor3Value, 2));
    delay(10000);
}

void SentOnCloud(String value1, String value2, String value3)
{
    String cmd = "AT+CIPSTART=\"TCP\",\"";
    cmd += HOST;
    cmd += "\",80";
    sendData(cmd);

    if (esp8266.find("Error"))
    {
        Serial.print("RECEIVED: Error\nExit1");
        return;
    }

    cmd = "GET /post.php?";
    cmd += "&s_1=";
    cmd += value1;
    cmd += "&s_2=";
    cmd += value2;
    cmd += "&s_3=";
    cmd += value3;
    cmd += " HTTP/1.1\r\nHost: localhost\r\n\r\n";
    esp8266.print("AT+CIPSEND=");
    esp8266.println(cmd.length());

    if (esp8266.find(">"))
    {
        Serial.print(">");
        Serial.print(cmd);
        esp8266.print(cmd);
    }
    else
    {
        esp8266.print("AT+CIPCLOSE");
    }

    if (esp8266.find("OK"))
    {
        Serial.println("RECEIVED: OK");
    }
    else
    {
        Serial.println("RECEIVED: Error\nExit2");
    }
}

String sendCommand(String command, const int timeout, boolean debug)
{
    String response = "";
    esp8266.print(command); // send the read character to the esp8266
    long int time = millis();

    while ((time + timeout) > millis())
    {
        while (esp8266.available())
        {
            // The esp has data so display its output to the serial window
            char c = esp8266.read(); // read the next character.
            response += c;
        }
    }

    if (debug)
    {
        Serial.print(response);
    }

    return response;
}

void sendData(String cmd)
{
    Serial.println("SEND: ");
    Serial.println(cmd);
    esp8266.println(cmd);
}

float moisture(int pin)
{
    int x = 0;
    float y, z, a, b, averageMois = 0;
    int mi = 240;
    int ma = 1024;
    int range = ma - mi;
    float A[10];

    for (int n = 0; n < 10; n++)
    {
        A[n] = 0;
    }

    for (int i = 0; i < 10; i++)
    {
        x = analogRead(pin);
        y = x - mi;
        z = 85 * y;
        a = z / range;
        b = 85 - a;
        A[i] = b;
        delay(200); // read once per 0.2 sec
    }

    averageMois = average(A);
    return averageMois;
}


float light(int pin)
{
    int a = 0;
    float b, c, averageLight = 0;
    int range = 1000;
    float A[10];

    for (int n = 0; n < 10; n++)
    {
        A[n] = 0;
    }

    for (int i = 0; i < 10; i++)
    {
        a = analogRead(pin);
        b = range / 100;
        c = a / b;
        A[i] = c;
        delay(200); // read once per 0.2 sec
    }

    averageLight = average(A);
    return averageLight;
}

float smoke(int pin)
{
    int a, b = 0;
    int mi = 30;
    int ma = 280;
    int range = ma - mi;
    float averageSmoke = 0;
    float A[10];

    for (int i = 0; i < 10; i++)
    {
        a = analogRead(pin);
        b = (a - mi) * 100 / range;
        A[i] = b;
        delay(200); // read once per 0.2 sec
    }

    averageSmoke = average(A);
    return averageSmoke;
}

float temp(int pin)
{
    int a = 0;
    float b, averageTemp = 0;
    float A[10];

    for (int n = 0; n < 10; n++)
    {
        A[n] = 0;
    }

    for (int i = 0; i < 10; i++)
    {
        a = analogRead(pin);
        b = (a * 125) >> 8;
        A[i] = b;
        delay(200);
    }

    averageTemp = average(A);
    return averageTemp;
}

float average(float value[10])
{
    float AVE;
    float compare[10];

    for (int i = 0; i < 10; i++)
    {
        compare[i] = 999;
    }

    for (int x = 0; x < 10; x++)
    {
        for (int y = 0; y < 10; y++)
        {
            if (value[y] < compare[x])
            {
                compare[x] = value[y];
            }
        }
    }

    for (int z = 1; z < 9; z++)
    {
        AVE += compare[z];
    }

    AVE /= 8;
    return AVE;
}