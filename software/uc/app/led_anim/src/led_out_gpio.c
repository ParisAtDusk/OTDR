#include "led_out_gpio.h"

#include <stddef.h>

static void GpioWrite(void *ctx, uint16_t level) {
  GpioPin_t *pin = (GpioPin_t *)ctx;
  if (pin->type == GpioTypePWM) {
    (void)GpioSetPwm(pin, level);
  } else {
    GpioSet(pin, (level != 0U) ? GpioStateOn : GpioStateOff);
  }
}

LedOut_t LedOutGpio_Make(GpioPin_t *pin) {
  LedOut_t out = {.write = NULL, .ctx = pin, .maxLevel = 1U};

  if (pin == NULL)
    return out;
  if (pin->type == GpioTypePWM) {
    out.maxLevel = (uint16_t)GPIO_PWM_DUTY_MAX;
    out.write = GpioWrite;
  } else if (pin->direction == GpioDirectionOutput) {
    out.write = GpioWrite;
  }
  return out;
}
