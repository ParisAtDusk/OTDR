#ifndef HARDWARE_IO_H
#define HARDWARE_IO_H

#include <stdbool.h>
#include <stdint.h>

#ifndef GPIO_PWM_DUTY_MAX
#define GPIO_PWM_DUTY_MAX 1000U
#endif

typedef enum {
  GpioStateOff = 0,
  GpioStateOn = 1,
} GpioState_e;

typedef enum {
  GpioDirectionInput,
  GpioDirectionOutput,
} GpioDirection_e;

typedef enum {
  GpioTypeDigital,
  GpioTypePWM,
} GpioType_e;

typedef struct {
  GpioType_e type;
  GpioDirection_e direction; /* meaningful for digital pins only */
  bool activeLow;
  GpioState_e state;

  union {
    struct {
      void *port;
      uint16_t pin;
    } digital;

    struct {
      void *timer;
      uint32_t channel;
      uint16_t duty;
    } pwm;
  } config;
} GpioPin_t;

void GpioInit(GpioPin_t *pin);

void GpioSet(GpioPin_t *pin, GpioState_e state);
GpioState_e GpioGet(const GpioPin_t *pin);
void GpioToggle(GpioPin_t *pin);

// Returns false (and does nothing) when called on a digital pin
bool GpioSetPwm(GpioPin_t *pin, uint16_t duty);

#endif /* HARDWARE_IO_H */
