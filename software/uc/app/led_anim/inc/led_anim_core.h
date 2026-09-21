#ifndef LED_ANIM_CORE_H
#define LED_ANIM_CORE_H

#include <stdbool.h>
#include <stdint.h>

#include "led_out.h"

#ifndef LED_ANIM_MAX_LEDS
#define LED_ANIM_MAX_LEDS 8U
#endif

#ifndef LED_ANIM_PWM_PERIOD_TICKS
#define LED_ANIM_PWM_PERIOD_TICKS 16U
#endif

#ifndef LED_ANIM_FADE_STEP_TICKS
#define LED_ANIM_FADE_STEP_TICKS 10U
#endif

#ifndef LED_ANIM_GAMMA_CORRECT
#define LED_ANIM_GAMMA_CORRECT 1
#endif

typedef uint32_t LedTick_t;

typedef struct {
  LedOut_t out;
  bool attached;
  bool active;
  bool written;
  uint16_t level;
  uint8_t mode;
  LedTick_t deadline;

  /* blink */
  LedTick_t onT, offT;
  uint32_t remaining;
  bool infinite;

  /* fade */
  LedTick_t riseT, fallT, stepT, cycleStart;
  uint32_t pos, virtMax;
  bool hw, pwmOffPending, done;
} LedCoreLed_t;

typedef struct {
  LedCoreLed_t leds[LED_ANIM_MAX_LEDS];
} LedCore_t;

void LedCore_Init(LedCore_t *core);

bool LedCore_Attach(LedCore_t *core, uint8_t idx, const LedOut_t *out);

bool LedCore_On(LedCore_t *core, uint8_t idx);
bool LedCore_Off(LedCore_t *core, uint8_t idx);

bool LedCore_Blink(LedCore_t *core, uint8_t idx, LedTick_t now, LedTick_t onT,
                   LedTick_t offT, uint32_t count);

bool LedCore_Fade(LedCore_t *core, uint8_t idx, LedTick_t now, LedTick_t riseT,
                  LedTick_t fallT, uint32_t cycles, uint8_t peak);

bool LedCore_Process(LedCore_t *core, LedTick_t now, LedTick_t *next);

#endif /* LED_ANIM_CORE_H */
