#include <Arduino.h>

// pins
const int CLK = 2;
const int LATCH1 = 3;
const int DATA1 = 4;
const int LATCH2 = 5;
const int DATA2 = 6;
const int LED1 = 7;
const int LED2 = 8;

const int TICK = 2;
const int DATA_BITS = 8;
const int DEBOUNCE_DELAY = 5;
const int LED_DELAY = 10;
const unsigned int BAUD_RATE = 57600;

long led1_time = 0;
long led2_time = 0;
int current_keys1;
int current_keys2;

void init_joystick(int data, int latch, int led, int CLK)
{
    pinMode(data, INPUT);
    digitalWrite(data, HIGH); // защита от наводок
    pinMode(CLK, OUTPUT);
    pinMode(latch, OUTPUT);
    pinMode(led, OUTPUT);
    digitalWrite(CLK, HIGH);
}

int get_keys_state_joystick(int data, int latch, int CLK)
{
    digitalWrite(latch, HIGH);
    delayMicroseconds(TICK);
    digitalWrite(latch, LOW);

    int keys_state = 0;

    for (int i = 0; i < DATA_BITS; ++i) {
        delayMicroseconds(TICK);
        digitalWrite(CLK, LOW);

        keys_state <<= 1;
        keys_state += digitalRead(data);

        delayMicroseconds(TICK);
        digitalWrite(CLK, HIGH);
    }

    return keys_state;
}

int get_filtered_keys(int data, int latch, int CLK, int &current_keys)
{
    int new_keys = get_keys_state_joystick(data, latch, CLK);
    if (current_keys != new_keys) {
      delay(DEBOUNCE_DELAY);
      new_keys = get_keys_state_joystick(data, latch, CLK);
      current_keys = new_keys;
    }
}

void setup()
{
    init_joystick(DATA1, LATCH1, LED1, CLK);
    init_joystick(DATA2, LATCH2, LED2, CLK);
    Serial.begin(BAUD_RATE);
}

void led_blink(int keys, int led_pin, long &led_time)
{
    // нажата кнопка
    if (keys != 0xFF) {
      led_time = millis();
      digitalWrite(led_pin, HIGH);
    }

    // кнопка отпущена
    if (keys == 0xFF) {
      long t = millis();
      if (t - led_time >= LED_DELAY) {
        digitalWrite(led_pin, LOW);
      }
    }
}

void handle_joystick(int data, int latch, int clk, int &current_keys, int led, long &led_time) {
    int keys = get_filtered_keys(data, latch, clk, current_keys);
    led_blink(keys, led, led_time);   
    Serial.write(keys);
}

void loop()
{
    handle_joystick(DATA1, LATCH1, CLK, current_keys1, LED1, led1_time);
    handle_joystick(DATA2, LATCH2, CLK, current_keys2, LED2, led2_time);
}
