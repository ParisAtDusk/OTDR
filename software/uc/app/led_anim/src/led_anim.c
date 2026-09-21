#include "led_anim.h"

#include <stddef.h>

#include "queue.h"

typedef enum { CMD_ATTACH = 0, CMD_ON, CMD_OFF, CMD_BLINK, CMD_FADE } CmdType_e;

typedef struct {
  uint8_t type;
  uint8_t led;
  union {
    LedOut_t out;
    struct {
      LedTick_t onT, offT;
      uint32_t count;
    } blink;
    struct {
      LedTick_t riseT, fallT;
      uint32_t cycles;
      uint8_t peak;
    } fade;
  } u;
} Cmd_t;

static LedCore_t s_core;
static QueueHandle_t s_queue;
static uint8_t s_alloc;

static TickType_t s_lastRaw;
static LedTick_t s_ext;

static LedTick_t Now(void) {
  TickType_t raw = xTaskGetTickCount();
  s_ext += (LedTick_t)(TickType_t)(raw - s_lastRaw);
  s_lastRaw = raw;
  return s_ext;
}

static LedTick_t MsToTicks(uint32_t ms) { return (LedTick_t)pdMS_TO_TICKS(ms); }

static void HandleCmd(const Cmd_t *c, LedTick_t now) {
  switch (c->type) {
  case CMD_ATTACH:
    (void)LedCore_Attach(&s_core, c->led, &c->u.out);
    break;
  case CMD_ON:
    (void)LedCore_On(&s_core, c->led);
    break;
  case CMD_OFF:
    (void)LedCore_Off(&s_core, c->led);
    break;
  case CMD_BLINK:
    (void)LedCore_Blink(&s_core, c->led, now, c->u.blink.onT, c->u.blink.offT,
                        c->u.blink.count);
    break;
  case CMD_FADE:
    (void)LedCore_Fade(&s_core, c->led, now, c->u.fade.riseT, c->u.fade.fallT,
                       c->u.fade.cycles, c->u.fade.peak);
    break;
  default:
    break;
  }
}

static void LedTask(void *arg) {
  (void)arg;
  Cmd_t cmd;

  for (;;) {
    LedTick_t now = Now();
    LedTick_t next;
    TickType_t wait = portMAX_DELAY;

    if (LedCore_Process(&s_core, now, &next)) {
      LedTick_t d = next - now;
      wait = (d >= (LedTick_t)portMAX_DELAY) ? (TickType_t)(portMAX_DELAY - 1U)
                                             : (TickType_t)d;
    }

    if (xQueueReceive(s_queue, &cmd, wait) == pdTRUE) {
      HandleCmd(&cmd, Now());
    }
  }
}

// API

bool LedAnim_Init(UBaseType_t taskPriority) {
  LedCore_Init(&s_core);
  s_alloc = 0U;
  s_ext = 0U;
  s_lastRaw = xTaskGetTickCount();
  s_queue = xQueueCreate(LED_ANIM_CMD_QUEUE_LEN, sizeof(Cmd_t));
  if (s_queue == NULL)
    return false;
  return xTaskCreate(LedTask, "LedAnim", LED_ANIM_TASK_STACK, NULL,
                     taskPriority, NULL) == pdPASS;
}

static bool Post(const Cmd_t *c) { return xQueueSend(s_queue, c, 0) == pdPASS; }

LedHandle_t LedAnim_Add(const LedOut_t *out) {
  if (out == NULL || out->write == NULL || out->maxLevel < 1U)
    return LED_ANIM_INVALID;

  LedHandle_t idx = LED_ANIM_INVALID;
  taskENTER_CRITICAL();
  if (s_alloc < LED_ANIM_MAX_LEDS)
    idx = s_alloc++;
  taskEXIT_CRITICAL();
  if (idx == LED_ANIM_INVALID)
    return idx;

  Cmd_t c = {.type = CMD_ATTACH, .led = idx};
  c.u.out = *out;
  (void)xQueueSend(s_queue, &c, portMAX_DELAY);
  return idx;
}

bool LedAnim_On(LedHandle_t led) {
  Cmd_t c = {.type = CMD_ON, .led = led};
  return Post(&c);
}

bool LedAnim_Off(LedHandle_t led) {
  Cmd_t c = {.type = CMD_OFF, .led = led};
  return Post(&c);
}

bool LedAnim_Blink(LedHandle_t led, uint32_t onMs, uint32_t offMs,
                   uint32_t count) {
  Cmd_t c = {.type = CMD_BLINK, .led = led};
  c.u.blink.onT = MsToTicks(onMs);
  c.u.blink.offT = MsToTicks(offMs);
  c.u.blink.count = count;
  return Post(&c);
}

bool LedAnim_Fade(LedHandle_t led, uint32_t riseMs, uint32_t fallMs,
                  uint32_t cycles, uint8_t peak) {
  Cmd_t c = {.type = CMD_FADE, .led = led};
  c.u.fade.riseT = MsToTicks(riseMs);
  c.u.fade.fallT = MsToTicks(fallMs);
  c.u.fade.cycles = cycles;
  c.u.fade.peak = peak;
  return Post(&c);
}
