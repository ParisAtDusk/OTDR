#ifndef LED_OUT_H
#define LED_OUT_H

#include <stdint.h>

typedef void (*LedOutWrite_f)(void *ctx, uint16_t level);

typedef struct {
  LedOutWrite_f write;
  void *ctx;
  uint16_t maxLevel;
} LedOut_t;

#endif /* LED_OUT_H */
