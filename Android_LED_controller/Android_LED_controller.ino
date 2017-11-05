#include <SoftwareSerial.h>
#define DEBUG true

SoftwareSerial esp8266(5, 6);

void setup()
{
    Serial.begin(9600);
    esp8266.begin(9600);
    sendCommand("AT+RST\r\n", 2000, DEBUG); // reset module
    sendCommand("AT+CWMODE=1\r\n", 1000, DEBUG); // configure as access point
    /***************************************************************/
    sendCommand("AT+CWJAP=\"SHAME_ON_YOU\",\"0971534218\"\r\n", 3000, DEBUG);
    delay(10000);
    sendCommand("AT+CIFSR\r\n", 1000, DEBUG); // get ip address
    sendCommand("AT+CIPMUX=1\r\n", 1000, DEBUG); // configure for multiple connections
    sendCommand("AT+CIPSERVER=1,80\r\n", 1000, DEBUG); // turn on server on port 80
    delay(500);

    for (int i = 0; i < 6; i++)
    {
        int pinNum = i + 7;
        pinMode(pinNum, OUTPUT);
        digitalWrite(pinNum, LOW);
    }

    delay(500);
    Serial.println("Server Ready");
}

void loop()
{
    if (esp8266.available()) // 檢查ESP是否有傳送
    {
        if (esp8266.find("+IPD,"))
        {
            delay(1000); // wait for the serial buffer to fill up (read all the serial data)
            // get the connection id so that we can then disconnect
            int connectionId = esp8266.read() - 48; //  ASCII 48 十進制 為 0
            String content;

            if (esp8266.find("pin=")) // advance cursor to "pin="
            {
                String pin = esp8266.readString();

                if (pin.indexOf(",") == -1 && pin.indexOf("%") == -1)
                {
                    int pinNumber = atoi(pin.c_str());

                    if (pinNumber < 7 || pinNumber > 12)
                    {
                        content = "THIS PIN NOT IN SERVICE";
                    }
                    else
                    {
                        digitalWrite(pinNumber, !digitalRead(pinNumber)); // toggle pin
                        String pinState[6];

                        for (int i = 0; i < 6; i++)
                        {
                            int pinNum = i + 7;

                            if (digitalRead(pinNum))
                            {
                                pinState[i] = "\"PIN" + String(pinNum) + "\"" + ":\"ON\"";
                            }
                            else
                            {
                                pinState[i] = "\"PIN" + String(pinNum) + "\"" + ":\"OFF\"";
                            }
                        }

                        content = "{" + pinState[0] + "," + pinState[1] + "," + pinState[2] + "," + pinState[3] + "," + pinState[4] + "," + pinState[5] + "}";
                        delay(100);
                    }
                }
                else if (pin.indexOf(",") == -1 && pin.indexOf("%") == 2)
                {
                    String pinState[6];

                    for (int i = 0; i < 6; i++)
                    {
                        int pinNum = i + 7;

                        if (digitalRead(pinNum))
                        {
                            pinState[i] = "\"PIN" + String(pinNum) + "\"" + ":\"ON\"";
                        }
                        else
                        {
                            pinState[i] = "\"PIN" + String(pinNum) + "\"" + ":\"OFF\"";
                        }
                    }

                    content = "{" + pinState[0] + "," + pinState[1] + "," + pinState[2] + "," + pinState[3] + "," + pinState[4] + "," + pinState[5] + "}";
                    delay(100);
                }
                else
                {
                    char PIN[128];
                    strcpy(PIN, pin.c_str());
                    char* index = ",";
                    char* pch;
                    pch = strtok(PIN, index);

                    while (pch != NULL)
                    {
                        digitalWrite(atoi(pch), !digitalRead(atoi(pch)));
                        pch = strtok(NULL, index);
                    }

                    String pinState[6];

                    for (int i = 0; i < 6; i++)
                    {
                        int pinNum = i + 7;

                        if (digitalRead(pinNum))
                        {
                            pinState[i] = "\"PIN" + String(pinNum) + "\"" + ":\"ON\"";
                        }
                        else
                        {
                            pinState[i] = "\"PIN" + String(pinNum) + "\"" + ":\"OFF\"";
                        }
                    }

                    content = "{" + pinState[0] + "," + pinState[1] + "," + pinState[2] + "," + pinState[3] + "," + pinState[4] + "," + pinState[5] + "}";
                    delay(100);
                }
            }

            sendHTTPResponse(connectionId, content);
            // make close command
            String closeCommand = "AT+CIPCLOSE=";
            closeCommand += connectionId; // append connection id
            closeCommand += "\r\n";
            sendCommand(closeCommand, 1000, DEBUG); // close connection
        }
    }
}

String sendData(String command, const int timeout, boolean debug)
{
    String response = "";
    int dataSize = command.length();
    char data[dataSize];
    command.toCharArray(data, dataSize);
    esp8266.write(data, dataSize); // send the read character to the esp8266

    if (debug)
    {
        Serial.println("\r\n====== HTTP Response From Arduino ======");
        Serial.write(data, dataSize);
        Serial.println("\r\n========================================");
    }

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

void sendHTTPResponse(int connectionId, String content)
{
    // build HTTP response
    String httpResponse;
    String httpHeader;
    // HTTP Header
    httpHeader = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=UTF-8\r\n";
    httpHeader += "Content-Length: ";
    httpHeader += content.length();
    httpHeader += "\r\n";
    httpHeader += "Connection: close\r\n\r\n";
    httpResponse = httpHeader + content + " "; // There is a bug in this code: the last character of "content" is not sent, I cheated by adding this extra space
    sendCIPData(connectionId, httpResponse);
}

void sendCIPData(int connectionId, String data)
{
    String cipSend = "AT+CIPSEND=";
    cipSend += connectionId;
    cipSend += ",";
    cipSend += data.length();
    cipSend += "\r\n";
    sendCommand(cipSend, 2000, DEBUG);
    sendData(data, 2000, DEBUG);
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
        Serial.println(response);
    }

    return response;
}