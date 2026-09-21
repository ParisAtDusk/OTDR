#include "led_anim_core.h"

#include <stddef.h>
#include <string.h>

#define PWM_P ((LedTick_t)LED_ANIM_PWM_PERIOD_TICKS)
#define HALF ((LedTick_t)(UINT32_MAX / 2U))

enum { MODE_IDLE = 0, MODE_BLINK, MODE_FADE };

/* ---------- helpers ---------- */

/* Wrap-safe "t has been reached" */
static bool Reached(LedTick_t now, LedTick_t t) {
  return (LedTick_t)(now - t) <= HALF;
}

static LedTick_t AtLeast1(LedTick_t t) { return (t == 0U) ? 1U : t; }

static LedCoreLed_t *Get(LedCore_t *core, uint8_t idx) {
  if (core == NULL || idx >= LED_ANIM_MAX_LEDS)
    return NULL;
  return core->leds[idx].attached ? &core->leds[idx] : NULL;
}

/* The only place the output is touched; suppresses redundant writes. */
static void Write(LedCoreLed_t *l, uint16_t level) {
  if (l->written && l->level == level)
    return;
  l->written = true;
  l->level = level;
  l->out.write(l->out.ctx, level);
}

static void Stop(LedCoreLed_t *l) {
  l->mode = MODE_IDLE;
  l->active = false;
  l->pwmOffPending = false;
}

/* ---------- BLINK ---------- */

static void StartBlink(LedCoreLed_t *l, LedTick_t now, LedTick_t onT,
                       LedTick_t offT, uint32_t count) {
  Stop(l);
  l->mode = MODE_BLINK;
  l->onT = AtLeast1(onT);
  l->offT = AtLeast1(offT);
  l->infinite = (count == 0U);
  l->remaining = count;
  Write(l, l->out.maxLevel);
  l->deadline = now + l->onT;
  l->active = true;
}

static void StepBlink(LedCoreLed_t *l) {
  if (l->level != 0U) { /* on-phase finished */
    Write(l, 0U);
    if (!l->infinite && --l->remaining == 0U) {
      Stop(l);
      return;
    }
    l->deadline += l->offT;
  } else {
    Write(l, l->out.maxLevel);
    l->deadline += l->onT;
  }
}

/* ---------- FADE ---------- */

static void StartFade(LedCoreLed_t *l, LedTick_t now, LedTick_t riseT,
                      LedTick_t fallT, uint32_t cycles, uint8_t peak) {
  Stop(l);
  l->mode = MODE_FADE;
  l->hw = (l->out.maxLevel > 1U);
  l->stepT = l->hw ? (LedTick_t)LED_ANIM_FADE_STEP_TICKS : PWM_P;
  l->riseT = (riseT < l->stepT) ? l->stepT : riseT;
  l->fallT = (fallT < l->stepT) ? l->stepT : fallT;
  l->infinite = (cycles == 0U);
  l->remaining = cycles;
  /* full scale in "virtual levels": hardware steps, or ticks of one PWM period
   */
  uint32_t base = l->hw ? (uint32_t)l->out.maxLevel : (uint32_t)PWM_P;
  l->virtMax = (base * (uint32_t)peak + 127U) / 255U;
  l->pos = 0U;
  l->done = false;
  l->deadline = now; /* first step happens immediately */
  l->active = true;
}

/* Linear triangle 0..virtMax at the current position */
static uint32_t Ramp(const LedCoreLed_t *l) {
  uint64_t total = (uint64_t)l->riseT + l->fallT;
  if (l->pos < l->riseT) {
    return (uint32_t)(((uint64_t)l->virtMax * l->pos) / l->riseT);
  }
  return (uint32_t)(((uint64_t)l->virtMax * (total - l->pos)) / l->fallT);
}

static uint32_t Gamma(uint32_t x, uint32_t maxv) {
#if LED_ANIM_GAMMA_CORRECT
  if (maxv == 0U)
    return 0U;
  return (uint32_t)((((uint64_t)x * x) + maxv / 2U) / maxv);
#else
  (void)maxv;
  return x;
#endif
}

static void AdvanceFade(LedCoreLed_t *l) {
  uint32_t total = (uint32_t)(l->riseT + l->fallT);
  l->pos += l->stepT;
  if (l->pos >= total) {
    l->pos -= total;
    if (!l->infinite && --l->remaining == 0U)
      l->done = true;
  }
}

static void StepFade(LedCoreLed_t *l) {
  if (l->pwmOffPending) { /* falling edge inside a software-PWM period */
    l->pwmOffPending = false;
    Write(l, 0U);
    l->deadline = l->cycleStart + l->stepT;
    return;
  }

  l->cycleStart = l->deadline;
  if (l->done) {
    Write(l, 0U);
    Stop(l);
    return;
  }

  uint32_t x = Gamma(Ramp(l), l->virtMax);
  AdvanceFade(l);

  if (l->hw) { /* hardware dimming: just set the level */
    Write(l, (uint16_t)x);
    l->deadline = l->cycleStart + l->stepT;
    return;
  }

  /* software PWM: x = on-time in ticks inside this period */
  if (x == 0U) {
    Write(l, 0U);
    l->deadline = l->cycleStart + PWM_P;
  } else if (x >= PWM_P) {
    Write(l, l->out.maxLevel);
    l->deadline = l->cycleStart + PWM_P;
  } else {
    Write(l, l->out.maxLevel);
    l->pwmOffPending = true;
    l->deadline = l->cycleStart + (LedTick_t)x;
  }
}

/* ---------- public API ---------- */

void LedCore_Init(LedCore_t *core) { memset(core, 0, sizeof(*core)); }

bool LedCore_Attach(LedCore_t *core, uint8_t idx, const LedOut_t *out) {
  if (core == NULL || idx >= LED_ANIM_MAX_LEDS || out == NULL ||
      out->write == NULL || out->maxLevel < 1U) {
    return false;
  }
  LedCoreLed_t *l = &core->leds[idx];
  memset(l, 0, sizeof(*l));
  l->out = *out;
  l->attached = true;
  Write(l, 0U);
  return true;
}

bool LedCore_On(LedCore_t *core, uint8_t idx) {
  LedCoreLed_t *l = Get(core, idx);
  if (l == NULL)
    return false;
  Stop(l);
  Write(l, l->out.maxLevel);
  return true;
}

bool LedCore_Off(LedCore_t *core, uint8_t idx) {
  LedCoreLed_t *l = Get(core, idx);
  if (l == NULL)
    return false;
  Stop(l);
  Write(l, 0U);
  return true;
}

bool LedCore_Blink(LedCore_t *core, uint8_t idx, LedTick_t now, LedTick_t onT,
                   LedTick_t offT, uint32_t count) {
  LedCoreLed_t *l = Get(core, idx);
  if (l == NULL)
    return false;
  StartBlink(l, now, onT, offT, count);
  return true;
}

bool LedCore_Fade(LedCore_t *core, uint8_t idx, LedTick_t now, LedTick_t riseT,
                  LedTick_t fallT, uint32_t cycles, uint8_t peak) {
  LedCoreLed_t *l = Get(core, idx);
  if (l == NULL)
    return false;
  StartFade(l, now, riseT, fallT, cycles, peak);
  return true;
}

bool LedCore_Process(LedCore_t *core, LedTick_t now, LedTick_t *next) {
  bool has = false;
  LedTick_t best = 0U;

  for (uint8_t i = 0; i < LED_ANIM_MAX_LEDS; i++) {
    LedCoreLed_t *l = &core->leds[i];
    if (!l->active)
      continue;

    if (Reached(now, l->deadline)) {
      if (l->mode == MODE_BLINK)
        StepBlink(l);
      else if (l->mode == MODE_FADE)
        StepFade(l);
      if (!l->active)
        continue; /* animation finished */
    }

    LedTick_t rem =
        Reached(now, l->deadline) ? 0U : (LedTick_t)(l->deadline - now);
    if (!has || rem < best) {
      best = rem;
      has = true;
    }
  }

  if (has && next != NULL)
    *next = now + best;
  return has;
}
