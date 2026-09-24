#ifndef TRACE_H
#define TRACE_H

#include <stdint.h>

// typedef enum{
//   POWER_RAW,
//   POWER_DBM,
//   POWER_W,
// } PowerType_e;

typedef struct {
  // PowerType_e type;
  uint32_t length;
  uint32_t *time_ps;   // max 4,294 ms
  uint16_t *power_raw; // adc is 16 bit
} trace_t;

#endif // !TRACE_H
