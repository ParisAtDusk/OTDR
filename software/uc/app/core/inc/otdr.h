#ifndef OTDR_H
#define OTDR_H

#include "result.h"
#include <stddef.h>
#include <stdint.h>

Result acquire_start(void);
Result acquire_stop(void);
Result acquire_set_iterations(uint32_t iter);
Result acquire_get_iterations(uint32_t *iter);
Result acquire_set_pulse_width(uint32_t width_ns);
Result acquire_get_pulse_width(uint32_t *width_ns);
Result acquire_set_laser_power(uint32_t power_uw);
Result acquire_get_laser_power(uint32_t *power_uw);

#endif // !OTDR_H
