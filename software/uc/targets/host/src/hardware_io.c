#define _POSIX_C_SOURCE 200809L
#include <stdio.h>
#include <time.h>

#include "hardware_io.h"

static long NowMs(void) {
  static struct timespec t0;
  static int init;
  struct timespec t;
  clock_gettime(CLOCK_MONOTONIC, &t);
  if (!init) {
    t0 = t;
    init = 1;
  }
  return (long)((t.tv_sec - t0.tv_sec) * 1000L +
                (t.tv_nsec - t0.tv_nsec) / 1000000L);
}

static void PrintDigital(const GpioPin_t *p, bool physicalHigh) {
  printf("[%6ld ms] DIG port=%p pin=%-2u -> %-3s (physical %s%s)\r\n", NowMs(),
         p->config.digital.port, (unsigned)p->config.digital.pin,
         p->state == GpioStateOn ? "ON" : "OFF", physicalHigh ? "HIGH" : "LOW",
         p->activeLow ? ", active-low" : "");
}

static void PrintPwm(const GpioPin_t *p, unsigned physicalDuty) {
  printf("[%6ld ms] PWM tim=%p ch=%-2lu -> duty %4u/%u (physical %4u%s)\r\n",
         NowMs(), p->config.pwm.timer, (unsigned long)p->config.pwm.channel,
         (unsigned)p->config.pwm.duty, (unsigned)GPIO_PWM_DUTY_MAX,
         physicalDuty, p->activeLow ? ", active-low" : "");
}

static void ApplyPwm(GpioPin_t *pin, uint16_t duty) {
  if (duty > GPIO_PWM_DUTY_MAX)
    duty = GPIO_PWM_DUTY_MAX;
  pin->config.pwm.duty = duty;
  pin->state = (duty > 0U) ? GpioStateOn : GpioStateOff;
  unsigned physical = pin->activeLow ? (GPIO_PWM_DUTY_MAX - duty) : duty;
  PrintPwm(pin, physical);
}

void GpioInit(GpioPin_t *pin) {
  if (pin->type == GpioTypePWM) {
    printf("[%6ld ms] init PWM tim=%p ch=%lu\r\n", NowMs(),
           pin->config.pwm.timer, (unsigned long)pin->config.pwm.channel);
    ApplyPwm(pin, 0);
  } else {
    printf("[%6ld ms] init DIG port=%p pin=%u (%s)\r\n", NowMs(),
           pin->config.digital.port, (unsigned)pin->config.digital.pin,
           pin->direction == GpioDirectionOutput ? "output" : "input");
    GpioSet(pin, GpioStateOff);
  }
}

void GpioSet(GpioPin_t *pin, GpioState_e state) {
  if (pin->type == GpioTypePWM) {
    ApplyPwm(pin, state == GpioStateOn ? GPIO_PWM_DUTY_MAX : 0U);
    return;
  }
  pin->state = state;
  bool logicalHigh = (state == GpioStateOn);
  PrintDigital(pin, pin->activeLow ? !logicalHigh : logicalHigh);
}

GpioState_e GpioGet(const GpioPin_t *pin) { return pin->state; }

void GpioToggle(GpioPin_t *pin) {
  GpioSet(pin, (pin->state == GpioStateOff) ? GpioStateOn : GpioStateOff);
}

bool GpioSetPwm(GpioPin_t *pin, uint16_t duty) {
  if (pin->type != GpioTypePWM)
    return false;
  ApplyPwm(pin, duty);
  return true;
}
