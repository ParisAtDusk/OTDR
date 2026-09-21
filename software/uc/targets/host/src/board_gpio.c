#include "board_gpio.h"

// digital LED, active-high
GpioPin_t led_status = {
    .type = GpioTypeDigital,
    .direction = GpioDirectionOutput,
    .activeLow = false,
    .config.digital = {.port = (void *)0xA000, .pin = 5},
};

// digital LED, active-low (driver inverts, engine doesn't care)
GpioPin_t led_rx = {
    .type = GpioTypeDigital,
    .direction = GpioDirectionOutput,
    .activeLow = true,
    .config.digital = {.port = (void *)0xC000, .pin = 9},
};

// PWM LED (hardware dimming)
GpioPin_t led_fault = {
    .type = GpioTypePWM,
    .direction = GpioDirectionOutput, // ignored for PWM
    .activeLow = false,
    .config.pwm = {.timer = (void *)0xB000, .channel = 1},
};
