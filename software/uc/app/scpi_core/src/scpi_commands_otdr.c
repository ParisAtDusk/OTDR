#include "scpi_commands_otdr.h"
#include "result.h"
#include "scpi/parser.h"
#include "scpi/types.h"
#include <stddef.h>
#include <stdint.h>

const scpi_otdr_api_t *_api;

scpi_result_t AcqStart(scpi_t *context) {
  (void)context;
  return ResSuccess(_api->acq_start()) ? SCPI_RES_OK : SCPI_RES_ERR;
}

scpi_result_t AcqStop(scpi_t *context) {
  (void)context;
  return ResSuccess(_api->acq_stop()) ? SCPI_RES_OK : SCPI_RES_ERR;
}

scpi_result_t AcqIters(scpi_t *context) {
  uint32_t iters;
  if (!SCPI_ParamUInt32(context, &iters, TRUE))
    return SCPI_RES_ERR;
  return ResSuccess(_api->acq_iters(iters)) ? SCPI_RES_OK : SCPI_RES_ERR;
}

scpi_result_t AcqItersQ(scpi_t *context) {
  uint32_t iters;
  if (ResIsError(_api->acq_iters_query(&iters)))
    return SCPI_RES_ERR;
  SCPI_ResultUInt32(context, iters);
  return SCPI_RES_OK;
}

scpi_result_t AcqPulseWidth(scpi_t *context) {
  uint32_t pw;
  if (!SCPI_ParamUInt32(context, &pw, TRUE))
    return SCPI_RES_ERR;
  return ResSuccess(_api->acq_pulsewidth(pw)) ? SCPI_RES_OK : SCPI_RES_ERR;
}

scpi_result_t AcqPulseWidthQ(scpi_t *context) {
  uint32_t pw;
  if (ResIsError(_api->acq_pulsewidth_query(&pw)))
    return SCPI_RES_ERR;
  SCPI_ResultUInt32(context, pw);
  return SCPI_RES_OK;
}
scpi_result_t AcqLaserPower(scpi_t *context) {
  uint32_t pwr;
  if (!SCPI_ParamUInt32(context, &pwr, TRUE))
    return SCPI_RES_ERR;
  return ResSuccess(_api->acq_laserpower(pwr)) ? SCPI_RES_OK : SCPI_RES_ERR;
}

scpi_result_t AcqLaserPowerQ(scpi_t *context) {
  uint32_t pwr;
  if (ResIsError(_api->acq_laserpower_query(&pwr)))
    return SCPI_RES_ERR;
  SCPI_ResultUInt32(context, pwr);
  return SCPI_RES_OK;
}

void RegisterOtdrApi(const scpi_otdr_api_t *api) { _api = api; }
