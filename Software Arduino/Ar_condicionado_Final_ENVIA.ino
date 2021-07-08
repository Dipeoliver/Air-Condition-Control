/*
      An IR LED must be connected to Arduino PWM pin 3.
*/

const int LM35 = A0; // Define o pino que lera a saída do LM35
float temperatura; // Variável que armazenará a temperatura medida
#include <IRremote.h>

IRsend irsend;

void setup()
{
  Serial.begin(9600); // inicializa a comunicação serial
}

void loop() {

  temperatura = (float(analogRead(LM35)) * 5 / (1023)) / 0.01;
  if (temperatura >= 26)
  {
    Serial.print(temperatura);
    // DISPARAR INFRA RED

    int khz = 38; // 38kHz carrier frequency for the NEC protocol
    unsigned int irSignal[] = {8712, 4000, 560, 1504, 512, 496, 488, 528, 484, 532, 512, 1512, 536, 476, 484, 532, 484, 524, 484, 532, 484, 528, 484, 532, 512, 500, 484, 528, 536, 476, 484, 532, 512, 500, 508, 504, 484, 1544, 512, 1512, 484, 532, 512, 500, 512, 1512, 488, 528, 508, 508, 512, 1512, 532, 476, 488, 1540, 484, 528, 488};

    irsend.sendRaw(irSignal, sizeof(irSignal) / sizeof(irSignal[0]), khz); //Note the approach used to automatically calculate the size of the array.
  }
  else
  {
    Serial.print(temperatura);

  }
  delay(300000); //In this example, the signal will be repeated every 5 seconds, approximately.
}
