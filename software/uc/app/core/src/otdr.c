#include "otdr.h"
#include "result.h"

Result acquire_start(void) { return R_ErrorNotImplemented; }

Result acquire_stop(void) { return R_ErrorNotImplemented; }

Result acquire_get_iterations(uint32_t *iter) {
  (void)iter;
  return R_ErrorNotImplemented;
}

Result acquire_set_iterations(uint32_t iter) {
  (void)iter;
  return R_ErrorNotImplemented;
}

Result acquire_set_pulse_width(uint32_t width_ns) {
  (void)width_ns;
  return R_ErrorNotImplemented;
}

Result acquire_get_pulse_width(uint32_t *width_ns) {
  *width_ns = 9999;
  return R_Success;
}

Result acquire_set_laser_power(uint32_t power_uw) {
  (void)power_uw;
  return R_ErrorNotImplemented;
}

Result acquire_get_laser_power(uint32_t *power_uw) {
  (void)power_uw;
  return R_ErrorNotImplemented;
}
