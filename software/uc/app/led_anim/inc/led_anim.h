#ifndef LED_ANIM_H
#define LED_ANIM_H

#include <stdbool.h>
#include <stdint.h>

#include "FreeRTOS.h"
#include "task.h"

#include "led_anim_core.h"
#include "led_out.h"

#ifndef LED_ANIM_CMD_QUEUE_LEN
#define LED_ANIM_CMD_QUEUE_LEN 16U
#endif

#ifndef LED_ANIM_TASK_STACK
#define LED_ANIM_TASK_STACK 256U
#endif

#define LED_ANIM_INVALID 0xFFU
typedef uint8_t LedHandle_t;

bool LedAnim_Init(UBaseType_t taskPriority);

LedHandle_t LedAnim_Add(const LedOut_t *out);

bool LedAnim_On(LedHandle_t led);
bool LedAnim_Off(LedHandle_t led);

bool LedAnim_Blink(LedHandle_t led, uint32_t onMs, uint32_t offMs,
                   uint32_t count);

bool LedAnim_Fade(LedHandle_t led, uint32_t riseMs, uint32_t fallMs,
                  uint32_t cycles, uint8_t peak);

#endif /* LED_ANIM_H */
