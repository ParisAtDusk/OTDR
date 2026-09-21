#ifndef LED_OUT_GPIO_H
#define LED_OUT_GPIO_H

#include "hardware_io.h"
#include "led_out.h"

LedOut_t LedOutGpio_Make(GpioPin_t *pin);

#endif /* LED_OUT_GPIO_H */
